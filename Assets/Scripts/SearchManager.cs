using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YoutubeExplode;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using System;
using UnityEngine.Events;

public class SearchManager : UIScreen
{
    [SerializeField] private RecyclingListView listView;
    [SerializeField] private GameObject loadingView;
    [SerializeField] private TMPro.TMP_InputField searchBar;
    [SerializeField] private int disableLoadThreshold;
    
    public int maxResults;
    public int curLoadedResults;

    private Dictionary<int, YoutubeExplode.Search.VideoSearchResult> searchResults = new();

    private CancellationTokenSource cts = new CancellationTokenSource();

    public bool searching;

    private Queue<int> toBeUpdated = new Queue<int>();

    private IAsyncEnumerator<YoutubeExplode.Search.VideoSearchResult> searchEnumerator;
    
    private void Start()
    {
        listView.ItemCallback += PopulateDelegate;
        listView.RowCount = maxResults;
    }

    private void Update()
    {
        if (toBeUpdated.Count > 0)
        {
            var idx = toBeUpdated.Dequeue();
            var item = listView.GetRowItem(idx);
            if (item)
            {
                var info = item as VideoInfo;
                if (info != null)
                    info.Populate(Metadata.CreatorFromSearch(searchResults[idx]), OnClick);
            }
        }
    }

    public async UniTask Search(string query, CancellationToken token)
    {
        Debug.Log("Searching for " + query);
        searching = true;
        searchResults.Clear();
        curLoadedResults = 0;
        listView.RowCount = maxResults;
        listView.Refresh();

        listView.gameObject.SetActive(false);
        loadingView.gameObject.SetActive(true);
        
        await UniTask.SwitchToThreadPool();
        searchEnumerator = Youtube.Instance.Search.GetVideosAsync(query, token).GetAsyncEnumerator();      
        try
        {
            //Search
            while (curLoadedResults < maxResults)
            {
                token.ThrowIfCancellationRequested();
                
                if (!await searchEnumerator.MoveNextAsync())
                {
                    await UniTask.SwitchToMainThread();
                    listView.RowCount = curLoadedResults;
                    break; 
                }

                if (curLoadedResults == disableLoadThreshold)
                    _ = DisableLoadingView();


                if (searchEnumerator.Current != null)
                {
                    searchResults[curLoadedResults] = searchEnumerator.Current;
                    toBeUpdated.Enqueue(curLoadedResults);
                    //_ = OnSearchResultObtained(curLoadedResults);
                    curLoadedResults++;
                }
            }
        }
        catch (Exception e)
        {
            if (!e.IsOperationCanceledException())
                Debug.LogException(e);
            else
                Debug.Log("Search for " + query + " was canceled");           
        }
        finally
        {
            await searchEnumerator.DisposeAsync();
            searching = false;
        }
    }

    public async UniTask SearchInit()
    {
        if (searchBar.text.Length < 1 || cts.IsCancellationRequested) return;

        cts.Cancel();
        await UniTask.WaitUntil(() => !searching);
        cts = new CancellationTokenSource();
        _ = Search(searchBar.text, cts.Token);
    }


    public void OnSearchBarChanged() => _ = SearchInit();

    public async UniTask DisableLoadingView()
    {
        await UniTask.SwitchToMainThread();
        listView.gameObject.SetActive(true);
        loadingView.gameObject.SetActive(false);
    }
    private void PopulateDelegate(RecyclingListViewItem item, int index)
    {
        if (searchResults.ContainsKey(index))
            (item as VideoInfo).Populate(Metadata.CreatorFromSearch(searchResults[index]), OnClick);
    }

    private void OnClick(Metadata meta)
    {
        PlayerManager.playlist.Add(meta.id);
    }

    public override void Show()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            searchBar.ActivateInputField();
    }

    public override void Hide()
    {
        cts.Cancel();
        cts = new CancellationTokenSource();

        searchResults.Clear();
        curLoadedResults = 0;
        listView.RowCount = maxResults;
        listView.Refresh();

        listView.gameObject.SetActive(false);
        loadingView.gameObject.SetActive(false);
    }
}
