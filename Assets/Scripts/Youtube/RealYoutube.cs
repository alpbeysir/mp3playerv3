using Google.Apis.YouTube.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Google.Apis.Services;
using MP3Player.Models;
using UnityEngine;

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
                            new[] { YouTubeService.Scope.Youtube },
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
    }
}