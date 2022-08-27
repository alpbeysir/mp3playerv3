using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LibraryView : UIScreen
{
    [SerializeField] private RecyclingListView listView;
    [SerializeField] private PlaylistView playlistView;

    private List<Playlist> playlists;
    
    private void Awake()
    {
        listView.ItemCallback += PopulateDelegate;
    }

    private void PopulateDelegate(RecyclingListViewItem item, int rowIndex)
    {
        var info = item as PlaylistInfo;
        info.Populate(playlists[rowIndex], PlaylistClicked);
    }

    public override void Show(params object[] args)
    {
        playlists = DB.Instance.GetCollection<Playlist>().FindAll().ToList();
        if (listView.RowCount != playlists.Count()) listView.RowCount = playlists.Count();
        else listView.Refresh();
    }

    private void PlaylistClicked(Playlist playlist)
    {
        ScreenManager.Instance.ShowOther(playlistView, playlist.Id);
    }

    public void Add()
    {
        //TODO new playlist wizard
        var added = new Playlist();
        added.Id = Utils.GetUniqueKey();
        added.Name = "New Playlist";
        added.Save();
        listView.ScrollToRow(listView.RowCount - 1);
        Show();
    }
}
