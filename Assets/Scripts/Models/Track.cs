using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using SimpleDuration;
using MP3Player.Models;
using MP3Player.Misc;
using MP3Player.Managers;
using MP3Player.Youtube;

namespace MP3Player.Models
{
    public class Track : DBObject<Track>
    {
        public Track() : base() { }

        public string Title { get; set; }
        public string ChannelName { get; set; }

        //In ticks, string format to make ARM happy
        public string Duration { get; set; }
        public string LowResThumbnailUrl { get; set; }
        public string HighResThumbnailUrl { get; set; }

        public Track(Google.Apis.YouTube.v3.Data.Video item)
        {
            Id = item.Id;
            Title = item.Snippet.Title;
            ChannelName = item.Snippet.ChannelTitle;

            if (Iso8601Duration.TryParse(item.ContentDetails.Duration, out TimeSpan timeSpan))
                Duration = timeSpan.Ticks.ToString();
            else
                Duration = "0";


            LowResThumbnailUrl = item.Snippet.Thumbnails.Default__.Url;
            if (item.Snippet.Thumbnails.Maxres != null)
                HighResThumbnailUrl = item.Snippet.Thumbnails.Maxres.Url;
            else
                HighResThumbnailUrl = item.Snippet.Thumbnails.High.Url;
        }

        public Track(IVideo src)
        {
            Id = src.Id;
            Title = src.Title;
            ChannelName = src.Author.ChannelTitle;
            Duration = src.Duration.Value.Ticks.ToString();

            YoutubeExplode.Common.Thumbnail lowest = src.Thumbnails[0], highest = src.Thumbnails[0];
            foreach (var t in src.Thumbnails)
            {
                if (lowest.Resolution.Area > t.Resolution.Area) lowest = t;
                if (highest.Resolution.Area < t.Resolution.Area) highest = t;
            }

            LowResThumbnailUrl = lowest.Url;
            HighResThumbnailUrl = highest.Url;
        }

        public bool AvailableOffline()
        {
            string temp;
            return TryGetExistingMedia(out temp);
        }

        public bool TryGetExistingMedia(out string path)
        {
            if (DownloadManager.IsDownloading(Id))
            {
                Debug.Log("Still downloading");
                path = string.Empty;
                return false;
            }

            Container[] possibleContainers = { Container.WebM, Container.Mp3, Container.Mp4, Container.Tgpp };
            string pathWithoutExt = Utils.MediaPath + Id;
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
                //Debug.Log(string.Format("Found existing media: {0}", path));
                return path;
            }
            else return (await GetAudioOnlyStreamInfoAsync(token)).Url;
        }

        public async Task<IStreamInfo> GetAudioOnlyStreamInfoAsync(CancellationToken token = default)
        {
            StreamManifest streamManifest = await FakeYoutube.Instance.Videos.Streams.GetManifestAsync(Id, token);
            var streamInfo = streamManifest.GetMuxedStreams().OrderBy(s => s.Bitrate).TryGetWithHighestBitrate();
            return streamInfo;
        }

        public async Task<IStreamInfo> GetVideoOnlyStreamInfoAsync(CancellationToken token = default)
        {
            StreamManifest streamManifest = await FakeYoutube.Instance.Videos.Streams.GetManifestAsync(Id, token);
            var streamInfos = streamManifest.GetMuxedStreams().OrderBy(s => s.Bitrate).ToList();
            var streamInfo = streamInfos.TryGetWithHighestVideoQuality();
            return streamInfo;
        }
    }
}