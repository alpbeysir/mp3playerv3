using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using MP3Player.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using YoutubeExplode.Search;
using Playlist = MP3Player.Models.Playlist;

namespace MP3Player.Youtube
{
    public static class RealYoutube
    {
        private const string CLIENT_SECRET = "{\"installed\":{\"client_id\":\"1011550511606-i0gb1pvsfpesnh79q117gdbd7lab2mpg.apps.googleusercontent.com\",\"project_id\":\"mp3playerv3\",\"auth_uri\":\"https://accounts.google.com/o/oauth2/auth\",\"token_uri\":\"https://oauth2.googleapis.com/token\",\"auth_provider_x509_cert_url\":\"https://www.googleapis.com/oauth2/v1/certs\",\"client_secret\":\"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\",\"redirect_uris\":[\"http://localhost\"]}}";
        private const string USER_DB_ID = "user";
        private static YouTubeService _userService;
        private static YouTubeService _apiKeyService;

        public static bool HasAuthenticated => _userService != null || SecureDataStore.Instance.GetAsync<TokenResponse>(USER_DB_ID) != null;
        public static YouTubeService GetApiKeyServiceAsync()
        {
            if (_apiKeyService == null)
            {
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = "AIzaSyDcSUkeeXoKcFXH4IZSHG8IbtDvESzoacM",
                    ApplicationName = "mp3playerv3"
                });
                _apiKeyService = youtubeService;
            }
            return _apiKeyService;
        }
        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        public static async Task<YouTubeService> GetUserServiceAsync()
        {
            if (_userService == null)
            {
                try
                {
                    UserCredential credential;
                    using (var stream = GenerateStreamFromString(CLIENT_SECRET))
                    {
                        credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.FromStream(stream).Secrets,
                            new[] { YouTubeService.Scope.YoutubeReadonly },
                            USER_DB_ID,
                            CancellationToken.None,
                            SecureDataStore.Instance,
                            new CompatibleCodeReciever()
                        );
                    }

                    _userService = new YouTubeService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "mp3playerv3"
                    });
                }
                catch (System.Exception e)
                {
                    //TODO handle error
                    Debug.LogException(e);
                    _userService = null;
                }
            }
            await ((UserCredential)_userService.HttpClientInitializer).RefreshTokenAsync(CancellationToken.None);
            return _userService;
        }

        public static async Task SyncPlaylistsAsync()
        {  
            try
            {
                var service = await GetUserServiceAsync();

                var playlistReq = service.Playlists.List("snippet, contentDetails");
                playlistReq.Mine = true;
                playlistReq.MaxResults = 50;
                var playlistResp = await playlistReq.ExecuteAsync();

                foreach (var existingPlaylist in DB.Instance.GetCollection<Playlist>().FindAll().Where(p => p.Source == PlaylistSource.Youtube))
                {
                    //Check if this playlist was deleted from YouTube
                    if (!playlistResp.Items.Any(yp => yp.Id == existingPlaylist.Id))                 
                        existingPlaylist.Delete();      
                }

                foreach (var youtubePlaylist in playlistResp.Items)
                {
                    await Playlist.FromYoutubePlaylist(youtubePlaylist);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        public class SearchEnumerator : IAsyncEnumerator<ISearchResult>
        {
            private readonly CancellationToken token;

            private string nextPage;
            private readonly List<SearchResult> cache = new();
            private int currentIndex = -1;
            private readonly string query;
            private readonly string relatedToVideoId;

            public ISearchResult Current => FromYoutubeSearchResult(cache[currentIndex]);

            private VideoSearchResult FromYoutubeSearchResult(SearchResult res)
            {
                return new(res.Id.VideoId,
                    res.Snippet.Title,
                    new YoutubeExplode.Common.Author(res.Snippet.ChannelId, res.Snippet.ChannelTitle),
                    TimeSpan.Zero,
                    FromYoutubeThumbnails(res.Snippet.Thumbnails)
                    );
            }

            private List<YoutubeExplode.Common.Thumbnail> FromYoutubeThumbnails(ThumbnailDetails td)
            {
                List<YoutubeExplode.Common.Thumbnail> t = new();

                t.Add(new YoutubeExplode.Common.Thumbnail(td.Default__.Url, new YoutubeExplode.Common.Resolution((int)td.Default__.Width.Value, (int)td.Default__.Height.Value)));
                if (td.Standard != null) t.Add(new YoutubeExplode.Common.Thumbnail(td.Standard.Url, new YoutubeExplode.Common.Resolution((int)td.Standard.Width.Value, (int)td.Standard.Height.Value)));
                if (td.High != null) t.Add(new YoutubeExplode.Common.Thumbnail(td.High.Url, new YoutubeExplode.Common.Resolution((int)td.High.Width.Value, (int)td.High.Height.Value)));
                if (td.Maxres != null) t.Add(new YoutubeExplode.Common.Thumbnail(td.Maxres.Url, new YoutubeExplode.Common.Resolution((int)td.Maxres.Width.Value, (int)td.Maxres.Height.Value)));
                return t;
            }

            public ValueTask DisposeAsync()
            {
                return new ValueTask(Task.CompletedTask);
            }

            public async ValueTask<bool> MoveNextAsync()
            {
                if (currentIndex < 0 || currentIndex >= cache.Count - 1)
                {
                    var service = GetApiKeyServiceAsync();

                    var searchReq = service.Search.List("snippet");
                    searchReq.Q = query;
                    searchReq.Type = "video";
                    searchReq.VideoCategoryId = "10";
                    searchReq.MaxResults = 12;
                    searchReq.PageToken = nextPage;
                    searchReq.SafeSearch = SearchResource.ListRequest.SafeSearchEnum.None;
                    searchReq.TopicId = "/m/04rlf";

                    if (relatedToVideoId != "") 
                        searchReq.RelatedToVideoId = relatedToVideoId;

                    // Maybe test this?
                    //searchReq.RelatedToVideoId = "QN1odfjtMoo%7CQbf8d-yC3yM";

                    Debug.Log("Made YouTube search API call");

                    var searchResp = await searchReq.ExecuteAsync(token);
                    nextPage = searchResp.NextPageToken;
                    if (searchResp.Items.Count == 0)
                        return false;

                    cache.AddRange(searchResp.Items);
                }

                currentIndex++;
                return true;
            }

            public enum SearchFilter
            {
                VideoAndPlaylists, Downloads, Smart
            }

            public SearchEnumerator(string _query, CancellationToken _token = default, string _relatedToVideoId = "")
            {
                query = _query;
                token = _token;
                relatedToVideoId = _relatedToVideoId;
            }
        }
    }
}