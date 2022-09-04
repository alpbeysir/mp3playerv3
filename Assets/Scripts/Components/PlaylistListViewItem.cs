using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using System;
using MP3Player.Models;
using Cysharp.Threading.Tasks;
using MP3Player.Managers;

namespace MP3Player.Components
{
    public class PlaylistListViewItem : RecyclingListViewItem, IPointerClickHandler
    {
        [SerializeField] private ImageView icon;

        [SerializeField] private TextMeshProUGUI nameDisplay;
        [SerializeField] private TextMeshProUGUI countDisplay;
        [SerializeField] private GameObject youtubePlaylistIndicator;

        private Playlist playlist;
        private Action<Playlist> onClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(playlist);
        }

        public void Populate(Playlist _playlist, Action<Playlist> _onClick = null)
        {
            playlist = _playlist;
            nameDisplay.text = playlist.Name;
            countDisplay.text = playlist.Count.ToString() + " items";
            //playlist.OnChanged += DataBinding;
            youtubePlaylistIndicator.SetActive(playlist.Source == PlaylistSource.Youtube);

            //TODO replace with collage of images, or maybe custom image from internal storage
            icon.SetLoading();
            _ = TextureManager.Texture2DFromUrlAsync(playlist.GetIconUri()).ContinueWith(tex => icon.SetImage(tex));

            onClick = _onClick;
        }

        //private void DataBinding(Playlist p)
        //{
        //    Populate(p, onClick);
        //}

        //private void OnDisable()
        //{
        //    if (playlist != null && playlist.OnChanged != null)
        //        playlist.OnChanged -= DataBinding;
        //}
    }
}