using MP3Player.Components;
using MP3Player.Managers;
using MP3Player.Models;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using MP3Player.Playback;
using MP3Player.Youtube;

namespace MP3Player.Views
{
    public class SearchView : UIView
    {
        [SerializeField] private RecyclingListView listView;
        [SerializeField] private GameObject loadingView;
        [SerializeField] private GameObject searchSomethingView;
        [SerializeField] private GameObject failedView;
        [SerializeField] private TMPro.TMP_InputField searchBar;
        [SerializeField] private int disableLoadThreshold;

        [SerializeField] private OptionsView optionsView;

        [SerializeField] private SwipeActionData leftSwipeActionData, rightSwipeActionData;

        public int maxResults;
        public int curLoadedResults;

        private Dictionary<int, YoutubeExplode.Search.VideoSearchResult> searchResults = new();

        private CancellationTokenSource cts = new CancellationTokenSource();

        public bool searching;

        private Queue<int> toBeUpdated = new Queue<int>();

        private IAsyncEnumerator<YoutubeExplode.Search.VideoSearchResult> searchEnumerator;

        private Playlist targetPlaylist;

        public override void Show(params object[] args)
        {
            //Get playlist if adding songs
            if (args.Length > 0)
            {
                if (targetPlaylist == null)
                {
                    Clear();
                    searchBar.SetTextWithoutNotify("");
                }
                targetPlaylist = args[0] as Playlist;
                searchBar.placeholder.GetComponent<TMPro.TextMeshProUGUI>().text = "Search for songs to add";
            }
            else
            {
                if (targetPlaylist != null)
                {
                    Clear();
                    searchBar.SetTextWithoutNotify("");
                }
                targetPlaylist = null;
                searchBar.placeholder.GetComponent<TMPro.TextMeshProUGUI>().text = "Search YouTube";
            }

            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
                searchBar.ActivateInputField();
        }

        private void Start()
        {
            listView.ItemCallback += PopulateDelegate;
            listView.RowCount = 0;

            //leftSwipeActionData.onActivate += OnLeftSwipe;
            //rightSwipeActionData.onActivate += OnRightSwipe;
        }

        private void Update()
        {
            if (toBeUpdated.Count > 0)
            {
                var index = toBeUpdated.Dequeue();
                var item = listView.GetRowItem(index);
                if (item)
                {
                    var info = item as TrackListViewItem;
                    if (info != null)
                        InternalPopulate(info, index);
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

            loadingView.gameObject.SetActive(true);
            listView.gameObject.SetActive(false);
            searchSomethingView.SetActive(false);
            failedView.SetActive(false);

            await UniTask.SwitchToThreadPool();
            searchEnumerator = FakeYoutube.Instance.Search.GetVideosAsync(query, token).GetAsyncEnumerator();
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


                    //Check duration to eliminate livestreams
                    if (searchEnumerator.Current != null && searchEnumerator.Current.Duration != null)
                    {
                        searchResults[curLoadedResults] = searchEnumerator.Current;
                        toBeUpdated.Enqueue(curLoadedResults);
                        curLoadedResults++;
                    }
                }
            }
            catch (Exception e)
            {
                if (!e.IsOperationCanceledException())
                {
                    Debug.LogException(e);
                    _ = ShowFailedView();
                }
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
                    searchSomethingView.SetActive(true);
                    failedView.SetActive(false);
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
            failedView.SetActive(false);
        }

        public async UniTask ShowFailedView()
        {
            await UniTask.SwitchToMainThread();
            listView.gameObject.SetActive(false);
            loadingView.SetActive(false);
            searchSomethingView.SetActive(false);
            failedView.SetActive(true);
        }

        private void PopulateDelegate(RecyclingListViewItem item, int index) => InternalPopulate(item as TrackListViewItem, index);

        private void InternalPopulate(TrackListViewItem info, int index)
        {
            info.ShowLoading();

            if (!searchResults.ContainsKey(index)) return;
            Track track = new(searchResults[index]);

            info.Populate(track);

            info.SetOnClickAction(OnClick);

            if (targetPlaylist != null && !targetPlaylist.Contains(track))
                info.SetButtonAction("e145", (t, ti) => { targetPlaylist.Add(t); ti.RemoveButtonAction(); });
            else
                info.SetButtonAction("e5d4", ShowTrackDetails);

            //info.SetLeftSwipeAction(leftSwipeActionData);

            //if (!track.AvailableOffline())
            //    info.SetRightSwipeAction(rightSwipeActionData);
            //else 
            //    info.RemoveRightSwipeAction();

            track.Save();
        }

        private void ShowTrackDetails(Track t, TrackListViewItem ti)
        {
            OptionsViewArgs args = new();

            args.options.Add(new()
            {
                iconUnicode = "e145",
                title = "Add To Queue",
                onClick = () =>
                {
                    PlayerController.AddToQueue(t);
                }
            });

            if (!t.AvailableOffline())
                args.options.Add(new()
                {
                    iconUnicode = "f090",
                    title = "Download",
                    onClick = () =>
                    {
                        _ = DownloadManager.DownloadAsync(t);
                        ti.Populate(t);
                    }
                });
            else
                args.options.Add(new()
                {
                    iconUnicode = "e872",
                    title = "Delete From Storage",
                    onClick = () =>
                    {
                        DownloadManager.Delete(t);
                        ti.Populate(t);
                    }
                });

            ScreenManager.Instance.ShowOther(optionsView, args);
        }

        //private void OnLeftSwipe(Track track, TrackInfo info)
        //{
        //    //TODO replace with playlist editor
        //    PlayerManager.AddToQueue(track);
        //    info.RemoveLeftSwipeAction();
        //}

        //private void OnRightSwipe(Track track, TrackInfo info)
        //{
        //    info.SetStatusState(true);
        //    info.RemoveRightSwipeAction();

        //    //This is buggy, may set another track info under certain conditions
        //    DownloadManager.OnDownloadComplete += (string id) =>
        //    {
        //        if (id == track.Id) info.SetStatusState(false);
        //    };
        //    DownloadManager.OnDownloadFailed += (string id) =>
        //    {
        //        if (id == track.Id) info.SetStatusState(false);
        //    };

        //    _ = DownloadManager.DownloadAsync(track);
        //}

        private void OnClick(Track track, TrackListViewItem info)
        {
            if (PlayerController.Current != track)
                PlayerController.PlayOverride(track);
        }

        private void Clear()
        {
            searchResults.Clear();
            curLoadedResults = 0;
            listView.RowCount = 0;
            listView.Refresh();
        }

        private void OnDestroy()
        {
            cts.Cancel();
        }
    }
}