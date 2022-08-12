using System;
using System.Collections.Generic;
using System.Threading.Tasks;
[Serializable]
public class Metadata : CacheObject<Metadata>
{
    public string title;
    public string channelName;
    public TimeSpan duration;
    public string sdThumbnailUrl;
    public string hdThumbnailUrl;
    public DateTimeOffset uploadDate;

    public Metadata(string id) : base(id) { }

    [UnityEngine.Scripting.Preserve]
    public static async Task<Metadata> Creator(string id)
    {
        var video = await Youtube.Instance.Videos.GetAsync(id);
        Metadata meta = new Metadata(video.Id);
        meta.title = video.Title;
        meta.channelName = video.Author.ChannelTitle;
        meta.duration = (TimeSpan)video.Duration;
        meta.uploadDate = video.UploadDate;

        YoutubeExplode.Common.Thumbnail lowest = video.Thumbnails[0], highest = video.Thumbnails[0];
        foreach (var t in video.Thumbnails)
        {
            if (lowest.Resolution.Area > t.Resolution.Area) lowest = t;
            if (highest.Resolution.Area < t.Resolution.Area) highest = t;
        }
        
        meta.sdThumbnailUrl = lowest.Url;
        meta.hdThumbnailUrl = highest.Url;

        return meta;
    }

    public static Metadata CreatorFromSearch(YoutubeExplode.Search.VideoSearchResult sr)
    {
        var meta = new Metadata(sr.Id.ToString());
        meta.title = sr.Title;
        meta.channelName = sr.Author.ChannelTitle;
        meta.uploadDate = DateTime.Now;
        meta.duration = sr.Duration != null ? (TimeSpan)sr.Duration : TimeSpan.Zero;
        meta.sdThumbnailUrl = sr.Thumbnails[0].Url;
        return meta;
    }
}