using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YoutubeExplode;
using Cysharp.Threading.Tasks;
using TMPro;

public class VideoInfo : RecyclingListViewItem
{
    [SerializeField] private Image thumbnailDisplay;
    [SerializeField] private TextMeshProUGUI titleDisplay;
    [SerializeField] private TextMeshProUGUI extraInfoDisplay;

    private Metadata metadata;

    public async UniTask Populate(string id, int index)
    {
        thumbnailDisplay.sprite = null;
        titleDisplay.text = "...";
        extraInfoDisplay.text = "...";
        metadata = await MetadataCache.GetByID(id);
        thumbnailDisplay.sprite = Utils.SpriteFromJpg(metadata.thumbJpg);
        titleDisplay.text = metadata.title;
        extraInfoDisplay.text = string.Format("{0} • {1} | {2} ... {3}", metadata.channelName, metadata.uploadDate.LocalDateTime.ToString("d"), metadata.duration.ToString("mm\\:ss"), index);
    }
}
