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
        titleDisplay.text = "...";
        extraInfoDisplay.text = "...";
    }
    
    public void DisplayData()
    {
        _ = thumbnailDisplay.Set(metadata.thumbnailUrl);
        titleDisplay.text = metadata.title;
        extraInfoDisplay.text = string.Format("{0} • {1} | {2}", metadata.channelName, metadata.uploadDate.LocalDateTime.ToString("d"), metadata.duration.ToString("mm\\:ss"));
    }

    public void Populate(Metadata meta)
    {
        metadata = meta;

        DisplayData();
    }
}
