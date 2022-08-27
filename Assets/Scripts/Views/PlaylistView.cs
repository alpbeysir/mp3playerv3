using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlaylistView : UIScreen
{
    [SerializeField] private RecyclingListView listView;
    [SerializeField] private SearchView searchView;
    [SerializeField] private OptionsView optionsView;

    [SerializeField] private TMP_InputField nameDisplay;
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
        Playlist.TryLoad(args[0] as string, out playlist);
        _ = iconDisplay.Set(playlist.GetIconUri());

        if (listView.RowCount != playlist.Count) listView.RowCount = playlist.Count;
        else listView.Refresh();

        if (listView.RowCount > 8)
            _ = FixListViewBug();
        else
        {
            topZoneLayout.padding.top = 65;
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)topZoneLayout.transform);
        }
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
            var z = (int)Utils.Map(Mathf.Clamp(listView.VerticalNormalizedPosition, 0.85f, 1f), 0.85f, 1, -325, 65);
            topZoneLayout.padding.top = (int)Mathf.Lerp(topZoneLayout.padding.top, z, 0.15f);
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)topZoneLayout.transform);
        }
    }

    private void PopulateDelegate(RecyclingListViewItem item, int rowIndex)
    {
        var info = item as TrackInfo;

        info.ShowLoading();

        var track = playlist[rowIndex];
        info.Populate(track, OnClickInfo);
    }

    private void OnClickInfo(Track t, TrackInfo ti)
    {
        playlist.Goto(t);
        PlayerManager.SetPlaylist(playlist);
    }

    public void Play()
    {
        playlist.ResetPosition();
        PlayerManager.SetPlaylist(playlist);
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
