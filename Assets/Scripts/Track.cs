using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

[Serializable]
public class Track : CacheObject<Track>
{
    public string Title { get; set; }
    public string ChannelName { get; set; }
    public TimeSpan Duration { get; set; }
    public string LowResThumbnailUrl { get; set; }
    public string HighResThumbnailUrl { get; set; }

    public Track() { }
    public Track(string id) : base(id) { }

    public override async Task<Track> Creator(string id, CancellationToken token)
    {
        var video = await Youtube.Instance.Videos.GetAsync(id, token);
        return FromIVideo(video);
    }

    public static Track CreatorFromSearch(YoutubeExplode.Search.VideoSearchResult sr) => FromIVideo(sr);

    private static Track FromIVideo<T>(T src) where T : IVideo
    {
        Track track = new(src.Id);
        track.id = src.Id;
        track.Title = src.Title;
        track.ChannelName = src.Author.ChannelTitle;
        track.Duration = (TimeSpan)src.Duration != null ? (TimeSpan)src.Duration : TimeSpan.Zero;

        YoutubeExplode.Common.Thumbnail lowest = src.Thumbnails[0], highest = src.Thumbnails[0];
        foreach (var t in src.Thumbnails)
        {
            if (lowest.Resolution.Area > t.Resolution.Area) lowest = t;
            if (highest.Resolution.Area < t.Resolution.Area) highest = t;
        }

        track.LowResThumbnailUrl = lowest.Url;
        track.HighResThumbnailUrl = highest.Url;

        return track;
    }

    public bool TryGetExistingMedia(out string path)
    {
        if (DownloadManager.IsDownloading(id))
        {
            Debug.Log("Still downloading");
            path = string.Empty;
            return false;
        }

        Container[] possibleContainers = { Container.WebM, Container.Mp3, Container.Mp4, Container.Tgpp };
        string pathWithoutExt = Utils.MediaPath + id;
        foreach (var c in possibleContainers)
        {
            path = string.Format("{0}.{1}", pathWithoutExt, c.Name);
            if (File.Exists(path)) return true;
        }
        path = "";
        return false;
    }

    public async Task<string> GetMediaUri(CancellationToken token = default)
    {
        string path;
        if (TryGetExistingMedia(out path))
        {
            Debug.Log(string.Format("Found existing media: {0}", path));
            return path;
        }
        else return (await GetStreamInfoAsync(token)).Url;
    }
    public async Task<AudioOnlyStreamInfo> GetStreamInfoAsync(CancellationToken token = default)
    {
        StreamManifest streamManifest = await Youtube.Instance.Videos.Streams.GetManifestAsync(id, token);
        var streamInfo = streamManifest.GetAudioOnlyStreams().OrderBy(s => s.Bitrate).TryGetWithHighestBitrate();
        return (AudioOnlyStreamInfo)streamInfo;
    }


}