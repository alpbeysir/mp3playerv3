using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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

    public static Metadata Creator(YoutubeExplode.Search.VideoSearchResult sr)
    {
        return new Metadata(sr.Id)
        {
            title = sr.Title,
            channelName = sr.Author.ChannelTitle,
            uploadDate = System.DateTimeOffset.Now,
            duration = (System.TimeSpan)sr.Duration,
            sdThumbnailUrl = sr.Thumbnails[0].Url
        };
    }
}