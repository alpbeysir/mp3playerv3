using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MP3Player.Models;
using MP3Player.Misc;
using MP3Player.Managers;
using MP3Player.Components;
using MP3Player.Playback;

namespace MP3Player.Views
{
    public class PlaylistView : UIView
    {
        [SerializeField] private RecyclingListView listView;
        [SerializeField] private SearchView searchView;
        [SerializeField] private OptionsView optionsView;

        [SerializeField] private TMP_InputField nameDisplay;
        [SerializeField] private Button editButton;
        [SerializeField] private ImageView iconDisplay;

        [SerializeField] private VerticalLayoutGroup topZoneLayout;

        private Playlist playlist;

        private bool fixedBug;

        /// <summary>
        /// Takes guid of playlist as argument, shows track list
        /// </summary>
        /// <param name="args"></param>
        public override void Show(params object[] args)
        {
            playlist = Playlist.Get(args[0] as string);

            iconDisplay.SetLoading();
            _ = TextureManager.Texture2DFromUrlAsync(playlist.GetIconUri()).ContinueWith(tex => iconDisplay.SetImage(tex));

            if (listView.RowCount != playlist.Count) listView.RowCount = playlist.Count;
            else listView.Refresh();

            if (listView.RowCount > 8)
                _ = FixListViewBug();
            else
            {
                topZoneLayout.padding.top = 65;
                LayoutRebuilder.MarkLayoutForRebuild((RectTransform)topZoneLayout.transform);
            }

            editButton.interactable = playlist.Source == PlaylistSource.Local;
            nameDisplay.interactable = playlist.Source == PlaylistSource.Local;
            nameDisplay.SetTextWithoutNotify(playlist.Name);
        }

        private async UniTask FixListViewBug()
        {
            if (fixedBug) return;
            fixedBug = true;

            topZoneLayout.padding.top = -380;
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)topZoneLayout.transform);
            await UniTask.DelayFrame(1);
            topZoneLayout.padding.top = 65;
        }

        private void Awake()
        {
            listView.ItemCallback += PopulateDelegate;
        }

        private void Update()
        {
            if (listView.RowCount > 8)
            {
                var rc = listView.RowCount;
                var ratio = (int)Utils.Map(Mathf.Clamp(listView.VerticalNormalizedPosition * rc, rc - 5, rc), rc - 5, rc, -325, 65);
                topZoneLayout.padding.top = (int)Mathf.Lerp(topZoneLayout.padding.top, ratio, 0.5f);
                LayoutRebuilder.MarkLayoutForRebuild((RectTransform)topZoneLayout.transform);
            }
        }

        private void PopulateDelegate(RecyclingListViewItem item, int rowIndex)
        {
            var info = item as TrackListViewItem;

            var track = playlist[rowIndex];
            info.Populate(track);
            info.SetOnClickAction(OnClickInfo);
            info.SetButtonAction("e5d4", ShowTrackOptionsView);
        }
        private void ShowTrackOptionsView(Track t, TrackListViewItem ti)
        {
            OptionsViewArgs args = new();

            args.options.Add(new()
            {
                iconUnicode = "e145",
                title = "Add to Queue",
                onClick = () =>
                {
                    PlayerController.AddToQueue(t);
                }
            });

            if (playlist.Source == PlaylistSource.Local)
                args.options.Add(new()
                {
                    iconUnicode = "eb80",
                    title = "Remove From Playlist",
                    onClick = () =>
                    {
                        playlist.Remove(t);
                    }
                });

            if (!t.AvailableOffline())
            {
                if (!DownloadManager.IsDownloading(t.Id))
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
                        iconUnicode = "e5cd",
                        title = "Cancel Download",
                        onClick = () =>
                        {
                            DownloadManager.CancelDownload(t.Id);
                            ti.Populate(t);
                        }
                    });
            }
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

        private void OnClickInfo(Track t, TrackListViewItem ti)
        {
            playlist.Goto(t);
            PlayerController.SetPlaylist(playlist);
        }

        public void OnEditBarChanged()
        {
            playlist.Name = nameDisplay.text == "" ? "New Playlist" : nameDisplay.text;
            playlist.Save();
        }

        public void Play()
        {
            playlist.ResetPosition();
            PlayerController.SetPlaylist(playlist);
        }

        public void Edit()
        {
            ScreenManager.Instance.ShowOther(searchView, playlist);
        }
        public void MoreOptions()
        {
            OptionsViewArgs args = new();
            args.options.Add(new()
            {
                iconUnicode = "f090",
                title = "Download Playlist",
                onClick = () =>
                {
                    playlist.GetAll().ForEach(t =>
                    {
                        if (!t.AvailableOffline())
                        {
                            _ = DownloadManager.DownloadAsync(t);
                            //When download is done update at index
                            DownloadManager.OnDownloadComplete += (id) =>
                            {
                                int toBeUpdatedIndex = playlist.Data.IndexOf(id);
                                listView.Refresh(toBeUpdatedIndex, 1);
                            };
                        }
                    });
                    listView.Refresh();
                }
            });

            if (playlist.Source == PlaylistSource.Local)
                args.options.Add(new()
                {
                    iconUnicode = "e92e",
                    title = "Delete Playlist",
                    onClick = () =>
                    {
                        playlist.Delete();
                        ScreenManager.Instance.Back();
                    }
                });

            ScreenManager.Instance.ShowOther(optionsView, args);
        }
    }
}