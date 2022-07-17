using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YoutubeExplode;
using Cysharp.Threading.Tasks;
using TMPro;

public class VideoInfo : RecyclingListViewItem
{
    [SerializeField] private NetworkedSprite thumbnailDisplay;
    [SerializeField] private TextMeshProUGUI titleDisplay;
    [SerializeField] private TextMeshProUGUI extraInfoDisplay;

    private Metadata metadata;
    public void ShowLoading()
    {
        thumbnailDisplay.ShowLoading(null);
        titleDisplay.text = "...";
        extraInfoDisplay.text = "...";
    }
    
    public void DisplayData()
    {
        _ = thumbnailDisplay.Set(metadata.sdThumbnailUrl);
        titleDisplay.text = metadata.title;
        extraInfoDisplay.text = string.Format("{0} • {1}", metadata.channelName, metadata.duration.ToString("mm\\:ss"));
    }

    public void Populate(Metadata meta)
    {
        metadata = meta;

        DisplayData();
    }
}
