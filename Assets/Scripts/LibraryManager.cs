using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyAndCode.UI;

public class LibraryManager : UIScreen, IRecyclableScrollRectDataSource
{
    [SerializeField] private RecyclableScrollRect gridView;

    private List<string> playlists = new List<string>();
    
    private void Awake()
    {
        gridView.DataSource = this;
    }
    public int GetItemCount()
    {
        return playlists.Count;
    }

    public override void Hide()
    {
        
    }

    public void SetCell(ICell cell, int index)
    {
        var info = cell as PlaylistInfo;
        //info.Populate(playlists[index], (p) => PlaylistClicked(p));
    }

    public override void Show()
    {
        InitializePlaylistList();
    }

    private void PlaylistClicked(Playlist playlist)
    {
        
    }
    private void InitializePlaylistList()
    {
        
    }
}
