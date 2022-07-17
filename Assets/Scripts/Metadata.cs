using System;
using System.Collections.Generic;
using YoutubeExplode;
using YoutubeExplode.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading.Tasks;

[Serializable]
public class Metadata : CacheObject<Metadata>
{
    public string title;
    public string channelName;
    public TimeSpan duration;
    public string thumbnailUrl;
    public DateTimeOffset uploadDate;

    public Metadata(string id) : base(id) { }
    
    public static async Task<Metadata> Creator(string id)
    {
        var video = await Youtube.Instance.Videos.GetAsync(id);
        Metadata meta = new Metadata(video.Id);
        meta.title = video.Title;
        meta.channelName = video.Author.ChannelTitle;
        meta.duration = (TimeSpan)video.Duration;
        meta.thumbnailUrl = video.Thumbnails.GetWithHighestResolution().Url;
        meta.uploadDate = video.UploadDate;
        return meta;
    }
}