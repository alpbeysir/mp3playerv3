using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;
using System.Text;

public class VideoInfo : RecyclingListViewItem, IPointerClickHandler
{
    [SerializeField] private GameObject loadingParent;
    [SerializeField] private GameObject infoParent;
    [SerializeField] private NetworkedSprite thumbnailDisplay;
    [SerializeField] private TextMeshProUGUI titleDisplay;
    [SerializeField] private TextMeshProUGUI extraInfoDisplay;

    //[SerializeField] private TMP_FontAsset font;

    private Metadata metadata;
    private Action<Metadata> onClick;
    
    public void ShowLoading()
    {
        loadingParent.SetActive(true);
        infoParent.SetActive(false);
    }
    
    public void DisplayData()
    {
        loadingParent.SetActive(false);
        infoParent.SetActive(true);
        _ = thumbnailDisplay.Set(metadata.sdThumbnailUrl);
        titleDisplay.text = RemoveUnsupportedChars(metadata.title);
        extraInfoDisplay.text = RemoveUnsupportedChars(string.Format("{0} • {1}", metadata.channelName, metadata.duration.ToString("hh\\:mm\\:ss")));

    }

    private string RemoveUnsupportedChars(string input)
    {
        StringBuilder sb = new StringBuilder(input.Length);
        for (int i = 0; i < input.Length; i++)
        {
            if (titleDisplay.font.HasCharacter(input[i])) sb.Append(input[i]);
            else sb.Append(' ');
        }
        return sb.ToString();
    }

    public void Populate(Metadata meta, Action<Metadata> _onClick = null)
    {
        metadata = meta;
        onClick = _onClick;
        DisplayData();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.DOScale(0.8f, 0.1f).OnComplete(() => { transform.DOScale(1f, 0.1f); });
        onClick?.Invoke(metadata);
    }
}
