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

    private IAsyncEnumerator<YoutubeExplode.Search.VideoSearchResult> searchEnumerator;

    private Dictionary<int, YoutubeExplode.Search.VideoSearchResult> searchResults = new();

    private CancellationTokenSource cts = new CancellationTokenSource();

    public bool searching;

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
            searching = true;
            searchResults.Clear();
            //maxSeenView = 0;
            curLoadedResults = 0;
            listView.RowCount = maxResults;
            listView.Refresh();
            searchEnumerator = Youtube.Instance.Search.GetVideosAsync(query).GetAsyncEnumerator();
            while (curLoadedResults < maxResults)
            {
                token.ThrowIfCancellationRequested();
                //await UniTask.WaitUntil(() => maxSeenView + 3 >= curLoadedResults, cancellationToken: token);
                
                if (!await searchEnumerator.MoveNextAsync())
                {
                    listView.RowCount = curLoadedResults; 
                    break; 
                }
                
                searchResults[curLoadedResults] = searchEnumerator.Current;
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
            searching = false;
        }
    }

    public async UniTask SearchInit()
    {
        if (searchBar.text.Length < 1 || cts.IsCancellationRequested) return;

        cts.Cancel();
        await UniTask.WaitUntil(() => !searching);
        cts = new CancellationTokenSource();
        _ = Search(searchBar.text, cts.Token).AttachExternalCancellation(cts.Token);
    }


    public void OnSearchBarChanged() => _ = SearchInit();

    private async UniTask InternalPopulate(VideoInfo info, int index, CancellationToken token)
    {
        YoutubeExplode.Search.VideoSearchResult res;
        while (!searchResults.TryGetValue(index, out res) && info != null)
        {
            token.ThrowIfCancellationRequested();
            info.ShowLoading();
            await UniTask.Yield();
        }

        if (info != null)
            info.Populate(Metadata.Creator(res));
    }

    private void PopulateDelegate(RecyclingListViewItem item, int index) => InternalPopulate(item as VideoInfo, index, cts.Token).AttachExternalCancellation(cts.Token);
}
