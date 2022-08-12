using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PolyAndCode.UI;
using TMPro;
using DG.Tweening;
using System;

public class PlaylistInfo : MonoBehaviour, ICell, IPointerClickHandler
{
    //TODO icon
    [SerializeField] private Image icon;
    
    [SerializeField] private TextMeshProUGUI nameDisplay;
    [SerializeField] private TextMeshProUGUI countDisplay;

    private string guid;  
    private Action<string> onClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.DOScale(0.8f, 0.1f).OnComplete(() => { transform.DOScale(1f, 0.1f); });
        onClick?.Invoke(guid);
    }

    public void Populate(Playlist playlist, Action<string> _onClick = null)
    {
        guid = playlist.Guid;
        nameDisplay.text = playlist.Name;
        countDisplay.text = playlist.Length.ToString();
        onClick = _onClick;
    }
}
