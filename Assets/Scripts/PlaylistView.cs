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
    [SerializeField] private SearchManager searchView;

    [SerializeField] private TMP_InputField nameDisplay;
    [SerializeField] private NetworkedSprite iconDisplay;

    private Playlist playlist;

    /// <summary>
    /// Takes guid of playlist as argument, shows track list
    /// </summary>
    /// <param name="args"></param>
    public override void Show(params object[] args)
    {
        Playlist.TryLoad(args[0] as string, out playlist);
        listView.RowCount = playlist.Count;
    }

    private void Awake()
    {
        listView.ItemCallback += PopulateDelegate;
    }

    private void PopulateDelegate(RecyclingListViewItem item, int rowIndex)
    {
        var info = item as TrackInfo;

        info.ShowLoading();

        var track = playlist[rowIndex];
        if (item.CurrentRow == rowIndex)
            info.Populate(track, OnClickInfo);
    }

    private void OnClickInfo(Track t, TrackInfo ti)
    {
        playlist.Goto(t);
        playlist.Previous();
        PlayerController.SetPlaylist(playlist);
    }

    public void Play()
    {
        playlist.ResetPosition();
        PlayerController.SetPlaylist(playlist);
    }

    public void Download()
    {
        playlist.GetAll().ForEach(t =>
        {
            if (!t.AvailableOffline()) _ = DownloadManager.DownloadAsync(t);
        });
        listView.Refresh();

    }
    public void Edit()
    {
        ScreenManager.Instance.ShowOther(searchView, playlist);
    }
    public void MoreOptions()
    {

    }
}
