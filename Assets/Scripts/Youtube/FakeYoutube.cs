using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Search;
using UnityEngine;
using MP3Player.Misc;

namespace MP3Player.Youtube
{
    public static class FakeYoutube
    {
        private static YoutubeClient _instance;
        public static YoutubeClient Instance
        {
            get
            {
                if (_instance == null) _instance = new YoutubeClient();
                return _instance;
            }
        }

        public class SearchEnumerator : IAsyncEnumerator<ISearchResult>
        {
            private readonly IAsyncEnumerator<ISearchResult> searchEnumerator;
            private readonly SearchFilter filter;
            private readonly CancellationToken token;

            private readonly int maxLoadCount;
            private int loadedResults;

            public ISearchResult Current => searchEnumerator.Current;

            public ValueTask DisposeAsync()
            {
                return searchEnumerator.DisposeAsync();
            }

            private async ValueTask<bool> SafeMoveNextAsync()
            {
                if (loadedResults >= maxLoadCount) return false;
                if (!await searchEnumerator.MoveNextAsync() || token.IsCancellationRequested)
                    return false;
                loadedResults++;
                return true;
            }

            private async ValueTask<bool> MoveUntilType<T>()
            {
                if (!await SafeMoveNextAsync()) return false;
                while (searchEnumerator.Current is not T)
                {
                    if (!await SafeMoveNextAsync())
                        return false;
                }
                return true;
            }

            private async ValueTask<bool> MoveUntilNotType<T>()
            {
                if (!await SafeMoveNextAsync()) return false;
                while (searchEnumerator.Current is T)
                {
                    if (!await SafeMoveNextAsync())
                        return false;
                }
                return true;
            }

            private async Task<bool> SmartFilter(VideoSearchResult result)
            {
                if (result.Title.Contains(result.Author.ChannelTitle)) return true;

                if (Utils.ContainsArray(result.Title, "|@!/")) return false;

                return (await Instance.Videos.GetAsync(result.Id)).Description.Contains("Provided to YouTube");
            }

            public async ValueTask<bool> MoveNextAsync()
            {
                if (!await SafeMoveNextAsync()) return false;

                //Skip channels
                if (!await MoveUntilNotType<ChannelSearchResult>()) return false;

                if (filter == SearchFilter.Smart)
                {
                    VideoSearchResult result;
                    do
                    {
                        if (!await MoveUntilType<VideoSearchResult>()) return false;
                        result = searchEnumerator.Current as VideoSearchResult;
                    }
                    while (!await SmartFilter(result) && !token.IsCancellationRequested);
                }

                return true;
            }

            public enum SearchFilter
            {
                VideoAndPlaylists, Downloads, Smart
            }

            public SearchEnumerator(string query, SearchFilter _filter = SearchFilter.VideoAndPlaylists, int _maxLoadCount = 500, CancellationToken _token = default)
            {
                filter = _filter;
                token = _token;
                maxLoadCount = _maxLoadCount;
                searchEnumerator = Instance.Search.GetResultsAsync(filter == SearchFilter.Smart ? query + " topic" : query, token).GetAsyncEnumerator(token);
            }
        }

    }
}