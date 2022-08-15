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
    [SerializeField] private GameObject searchSomethingView;
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
        listView.RowCount = 0;
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
                    info.Populate(Track.CreatorFromSearch(searchResults[idx]), OnClick);
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
        searchSomethingView.SetActive(false);
        
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
        if (searchBar.text.Length < 1 || cts.IsCancellationRequested) 
        {
            if (searchBar.text.Length == 0)
            {
                await DisableLoadingView();
                listView.gameObject.SetActive(false);
                searchSomethingView.gameObject.SetActive(true);
            }
            Clear();
            return;
        }

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
        loadingView.SetActive(false);
    }
    private void PopulateDelegate(RecyclingListViewItem item, int index)
    {
        (item as VideoInfo).ShowLoading();

        if (searchResults.ContainsKey(index))
            (item as VideoInfo).Populate(Track.CreatorFromSearch(searchResults[index]), OnClick);
    }

    private void OnClick(Track meta)
    {
        PlayerManager.playlist.Add(meta.id);
    }

    private void Clear()
    {
        searchResults.Clear();
        curLoadedResults = 0;
        listView.RowCount = 0;
        listView.Refresh();
    }

    public override void Show()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            searchBar.ActivateInputField();
    }

    public override void Hide()
    {
        //cts.Cancel();
        //cts = new CancellationTokenSource();

        //Clear();

        //listView.gameObject.SetActive(false);
        //loadingView.SetActive(false);
    }

    private void OnDestroy()
    {
        cts.Cancel();
    }
}
