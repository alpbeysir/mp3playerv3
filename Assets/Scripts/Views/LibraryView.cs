using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MP3Player.Models;
using MP3Player.Managers;
using MP3Player.Components;
using MP3Player.Youtube;
using Cysharp.Threading.Tasks;

namespace MP3Player.Views
{
    public class LibraryView : UIView
    {
        [SerializeField] private RecyclingListView listView;
        [SerializeField] private PlaylistView playlistView;

        private List<string> playlists;

        private void Awake()
        {
            listView.ItemCallback += PopulateDelegate;
        }

        private void PopulateDelegate(RecyclingListViewItem item, int rowIndex)
        {
            var info = item as PlaylistListViewItem;
            info.Populate(Playlist.Get(playlists[rowIndex]), PlaylistClicked);
        }

        private void UpdatePlaylists()
        {
            playlists = DB.Instance.GetCollection<Playlist>().FindAll().Select(p => p.Id).ToList();
        }

        public override async void Show(params object[] args)
        {
            await UniTask.RunOnThreadPool(UpdatePlaylists);

            if (listView.RowCount != playlists.Count) listView.RowCount = playlists.Count;
            else listView.Refresh();

            _ = UniTask.RunOnThreadPool(RealYoutube.SyncPlaylistsAsync);
        }

        private void PlaylistClicked(Playlist playlist)
        {
            ScreenManager.Instance.ShowOther(playlistView, playlist.Id);
        }

        public void Add()
        {
            //TODO new playlist wizard
            var added = new Playlist();
            added.Name = "New Playlist";
            added.Source = PlaylistSource.Local;
            added.Save();
            Show();
        }
    }
}