using System;
using System.Collections.Generic;
using YoutubeExplode;
using YoutubeExplode.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;

[Serializable]
public partial class Metadata
{
    public string id;
    public string title;
    public string channelName;
    public TimeSpan duration;
    public byte[] thumbJpg;
    public DateTimeOffset uploadDate;

    public static async UniTask<Metadata> FromVideo(YoutubeExplode.Videos.Video video)
    {
        Metadata meta = new Metadata();
        meta.id = video.Id;
        meta.title = video.Title;
        meta.channelName = video.Author.ChannelTitle;
        meta.duration = (TimeSpan)video.Duration;
        meta.thumbJpg = await Utils.DownloadFromURL(video.Thumbnails.GetWithHighestResolution().Url);
        meta.uploadDate = video.UploadDate;
        return meta;
    }
}