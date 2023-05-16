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
using YoutubeExplode.Search;

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

        public int maxRealYoutubeSearches;
        public int maxResults;
        public int curLoadedResults;
        public int maxSeenIndex;

        private Dictionary<int, VideoSearchResult> searchResults = new();

        private CancellationTokenSource cts = new CancellationTokenSource();

        public bool searching;

        private Queue<int> toBeUpdated = new Queue<int>();

        private IAsyncEnumerator<ISearchResult> searchEnumerator;

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
            //So we don't exceed quota
            await UniTask.Delay(500, false, PlayerLoopTiming.Update, token);

            Debug.Log("Searching for " + query);
            searching = true;
            searchResults.Clear();
            curLoadedResults = 0;
            listView.RowCount = maxResults;
            listView.Refresh();
            listView.VerticalNormalizedPosition = 1;

            loadingView.gameObject.SetActive(true);
            listView.gameObject.SetActive(false);
            searchSomethingView.SetActive(false);
            failedView.SetActive(false);


            await UniTask.SwitchToThreadPool();
            bool switchedToFakeSearch = false;
            searchEnumerator = new RealYoutube.SearchEnumerator(query, token);

            try
            {
                //Search
                while (curLoadedResults < maxResults)
                {
                    if (maxSeenIndex <= curLoadedResults) continue;

                    if (curLoadedResults >= maxRealYoutubeSearches && !switchedToFakeSearch)
                    {
                        searchEnumerator = new FakeYoutube.SearchEnumerator(query, FakeYoutube.SearchEnumerator.SearchFilter.VideoAndPlaylists, maxResults, token);
                        switchedToFakeSearch = true;
                    }

                    token.ThrowIfCancellationRequested();

                    if (!await searchEnumerator.MoveNextAsync())
                    {
                        await UniTask.SwitchToMainThread();
                        listView.RowCount = curLoadedResults;
                        break;
                    }

                    if (curLoadedResults >= disableLoadThreshold)
                        _ = DisableLoadingView();

                    //Check duration to eliminate livestreams
                    if (searchEnumerator.Current is VideoSearchResult result && result.Duration != null)
                    {
                        searchResults[curLoadedResults] = result;
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

            if (index > maxSeenIndex) maxSeenIndex = index;

            if (!searchResults.ContainsKey(index)) return;

            Track track = new(searchResults[index]);

            ////Add related tracks
            //foreach (var item in searchResults.Values)
            //{
            //    Track a = new(item);
            //    a.Save();
            //    track.AddRelatedTrack(a);
            //}

            info.Populate(track);

            info.SetOnClickAction(OnClick);

            if (targetPlaylist != null && !targetPlaylist.Contains(track))
                info.SetButtonAction("e145", (t, ti) => { targetPlaylist.Add(t); ti.RemoveButtonAction(); });
            else
                info.SetButtonAction("e5d4", ShowTrackDetails);

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