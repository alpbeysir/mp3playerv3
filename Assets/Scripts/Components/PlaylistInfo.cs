using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using System;

public class PlaylistInfo : RecyclingListViewItem, IPointerClickHandler
{
    //TODO icon
    [SerializeField] private ImageView icon;
    
    [SerializeField] private TextMeshProUGUI nameDisplay;
    [SerializeField] private TextMeshProUGUI countDisplay;

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

        //TODO replace with collage of images, or maybe custom image from internal storage
        var iconUri = playlist.GetIconUri();
        if (iconUri != null) _ = icon.Set(iconUri);

        onClick = _onClick;
    }
}
