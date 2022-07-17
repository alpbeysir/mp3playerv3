using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YoutubeExplode;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class SearchManager : Singleton<SearchManager>
{
    [SerializeField] private RecyclingListView listView;
    [SerializeField] private TMPro.TMP_InputField searchBar;

    public int maxResults;
    public int curLoadedResults;
    public int maxSeenView;

    private IAsyncEnumerator<YoutubeExplode.Search.VideoSearchResult> searchEnumerator;

    private Dictionary<int, YoutubeExplode.Search.VideoSearchResult> searchResults = new();

    private CancellationTokenSource cts = new CancellationTokenSource();

    public int runningSrcCount;

    private void Start()
    {
        ApplicationChrome.statusBarState = ApplicationChrome.States.Visible;
        ApplicationChrome.navigationBarState = ApplicationChrome.States.Visible;
        listView.ItemCallback += PopulateDelegate;
        listView.RowCount = maxResults;
    }

    public async UniTask Search(string query, CancellationToken token)
    {
        try
        {
            //await UniTask.SwitchToThreadPool();
            Debug.Log("Searching for " + query);
            runningSrcCount++;
            searchResults.Clear();
            maxSeenView = 0;
            curLoadedResults = 0;
            listView.Refresh();
            searchEnumerator = Youtube.Instance.Search.GetVideosAsync(query).GetAsyncEnumerator();
            while (curLoadedResults < maxResults)
            {
                token.ThrowIfCancellationRequested();
                await UniTask.WaitUntil(() => maxSeenView + 3 >= curLoadedResults, cancellationToken: token);
                await searchEnumerator.MoveNextAsync();
                searchResults[curLoadedResults] = searchEnumerator.Current;

                //var updatedDisplay = listView.GetRowItem(curLoadedResults);
                //if (updatedDisplay != null) (updatedDisplay as VideoInfo).Populate(FromSearchResult(searchResults[curLoadedResults]));

                curLoadedResults++;
            }
        }
        catch
        {
            Debug.Log("Search for " + query + " was canceled!");
        }
        finally
        {
            _ = searchEnumerator.DisposeAsync();
            runningSrcCount--;
        }
    }

    public async void OnSearchBarChanged()
    {
        if (searchBar.text.Length < 1 || cts.IsCancellationRequested) return;

        cts.Cancel();
        await UniTask.WaitUntil(() => runningSrcCount == 0);
        cts = new CancellationTokenSource();
        _ = Search(searchBar.text, cts.Token).AttachExternalCancellation(cts.Token);
    }

    private async UniTask InternalPopulate(VideoInfo info, int index)
    {
        //Wait for search data to come
        if (index > maxSeenView) maxSeenView = index;
        info.ShowLoading();
        await UniTask.WaitUntil(() => searchResults.ContainsKey(index), cancellationToken: cts.Token);

        //Populate view
        YoutubeExplode.Search.VideoSearchResult res;
        lock (searchResults)
        {
            if (searchResults.TryGetValue(index, out res))
                info.Populate(Metadata.Creator(res));
        }
    }

    private void PopulateDelegate(RecyclingListViewItem item, int index) => InternalPopulate(item as VideoInfo, index).AttachExternalCancellation(cts.Token);
}
