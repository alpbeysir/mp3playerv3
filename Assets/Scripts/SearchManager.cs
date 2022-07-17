using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YoutubeExplode;
using Cysharp.Threading.Tasks;

public class SearchManager : Singleton<SearchManager>
{
    [SerializeField] private RecyclingListView listView;

    public int maxResults;
    public int curLoadedResults;

    private IAsyncEnumerator<YoutubeExplode.Search.VideoSearchResult> searchEnumerator;

    private Dictionary<int, YoutubeExplode.Search.VideoSearchResult> searchResults = new Dictionary<int, YoutubeExplode.Search.VideoSearchResult>();

    private void Start()
    {
        listView.ItemCallback += PopulateDelegate;
        listView.RowCount = maxResults;
        Search();
    }

    public async UniTask Search()
    {
        await UniTask.SwitchToThreadPool();
        searchResults.Clear();
        curLoadedResults = 0;
        searchEnumerator = Youtube.Instance.Search.GetVideosAsync("c# multithreading").GetAsyncEnumerator();
        while (curLoadedResults < maxResults)
        {
            await searchEnumerator.MoveNextAsync();
            searchResults[curLoadedResults] = searchEnumerator.Current;
            curLoadedResults++;
        }
        await searchEnumerator.DisposeAsync();
    }
    
    private async UniTask InternalPopulate(VideoInfo info, int index)
    {
        //Wait for search data to come
        info.ShowLoading();
        await UniTask.WaitUntil(() => searchResults.ContainsKey(index));
        info.Populate(FromSearchResult(searchResults[index]));
    }

    private void PopulateDelegate(RecyclingListViewItem item, int index)
    {
        _ = InternalPopulate(item as VideoInfo, index);
    } 

    private Metadata FromSearchResult(YoutubeExplode.Search.VideoSearchResult sr)
    {
        return new Metadata(sr.Id)
        {
            title = sr.Title,
            channelName = sr.Author.ChannelTitle,
            uploadDate = System.DateTimeOffset.Now,
            duration = (System.TimeSpan)sr.Duration,
            thumbnailUrl = sr.Thumbnails[0].Url
        };
    }
}
