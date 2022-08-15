using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;
using System.Text;
using UnityEngine.UI;

public class VideoInfo : RecyclingListViewItem, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject loadingParent;
    [SerializeField] private GameObject infoParent;
    [SerializeField] private NetworkedSprite thumbnailDisplay;
    [SerializeField] private TextMeshProUGUI titleDisplay;
    [SerializeField] private TextMeshProUGUI extraInfoDisplay;

    //[SerializeField] private TMP_FontAsset font;

    private Track metadata;
    private Action<Track> onClick;

    private static VideoInfo swiped;
    
    public void ShowLoading()
    {
        loadingParent.SetActive(true);
        infoParent.SetActive(false);
    }
    
    public void DisplayData()
    {
        loadingParent.SetActive(false);
        infoParent.SetActive(true);
        _ = thumbnailDisplay.Set(metadata.LowResThumbnailUrl);
        titleDisplay.text = RemoveUnsupportedChars(metadata.Title);
        if (DownloadManager.IsDownloading(metadata.id))
        {
            extraInfoDisplay.text = "Downloading...";
        }
        else
        {
            extraInfoDisplay.text = RemoveUnsupportedChars(string.Format("{0} • {1}", metadata.ChannelName, metadata.Duration.ToString("mm\\:ss")));
        }
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

    public void Populate(Track meta, Action<Track> _onClick = null)
    {
        metadata = meta;
        onClick = _onClick;
        DisplayData();
    }

    private Vector2 initialPos;
    private Vector2 initialMousePos;

    public void OnBeginDrag(PointerEventData eventData)
    {
        ParentList.GetComponent<ScrollRect>().OnBeginDrag(eventData);
        if (swiped != null) return;
        swiped = this;
        initialMousePos = Input.mousePosition;
        initialPos = transform.localPosition;
    }

    private bool passedThreshold;
    public void OnDrag(PointerEventData eventData)
    {
        if (Mathf.Abs(initialMousePos.x - Input.mousePosition.x) > 200) passedThreshold = true;
        if (Mathf.Abs(initialMousePos.y - Input.mousePosition.y) > 150 && swiped == this)
        {
            transform.DOLocalMove(initialPos, 0.1f);
            swiped = null;
            passedThreshold = false;
        }
        if (swiped == null || !passedThreshold)
        {
            ParentList.GetComponent<ScrollRect>().OnDrag(eventData);
            return;
        }
        if (infoParent.activeSelf)
            transform.position = Vector2.Lerp(transform.position, new Vector2(transform.position.x + (eventData.delta.x * 5), transform.position.y), 0.20f);
    }

    private void EndSwipe()
    {
        var delta = (Vector2)transform.localPosition - initialPos;
        if (Mathf.Abs(delta.x) > 100)
        {
            if (delta.x > 0)
            {
                //Left action
                onClick.Invoke(metadata);
            }
            else
            {
                //Right action
                _ = DownloadManager.DownloadAsync(metadata);
                extraInfoDisplay.text = "Downloading...";
                DownloadManager.OnDownloadComplete += (string id) =>
                {
                    if (id == metadata.id) extraInfoDisplay.text = "Download complete";
                };
            }
        }
        transform.DOLocalMove(initialPos, 0.1f);
        passedThreshold = false;
        swiped = null;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ParentList.GetComponent<ScrollRect>().OnEndDrag(eventData);
        if (swiped == this) EndSwipe();
    }

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    throw new NotImplementedException();
    //}

    //public void OnPointerUp(PointerEventData eventData)
    //{
    //    throw new NotImplementedException();
    //}
}
