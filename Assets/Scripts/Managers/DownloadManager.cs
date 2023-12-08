using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using YoutubeExplode.Videos.Streams;
using System.Threading;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;
using Cysharp.Threading.Tasks;
using System.IO;
using MP3Player.Models;
using MP3Player.Misc;
using MP3Player.Playback;
using MP3Player.Youtube;

namespace MP3Player.Managers
{

    //TODO handle app closing while downloading
    public static class DownloadManager
    {
        public static Action<string> OnDownloadStarted;
        public static Action<string> OnDownloadComplete;
        public static Action<string> OnDownloadFailed;
        public static Action<string, float> OnDownloadProgress;

        private const int MAX_CONCURRENT_DOWNLOADS = 5;

        private static readonly Dictionary<string, (Progress<double> p, CancellationTokenSource cts)> downloads = new();

        private static readonly ConcurrentQueue<Track> dlQueue = new();

        public static void CancelDownload(string id)
        {
            downloads[id].cts.Cancel();
            downloads.Remove(id);
        }

        public static bool IsDownloading(string id)
        {
            return downloads.ContainsKey(id);
        }

        public static void Delete(Track track)
        {
            if (track.AvailableOffline() && PlayerController.Current.Id != track.Id) File.Delete(track.GetMediaUri().Result);
        }

        private static void PurgeTempFiles()
        {
            foreach(var tempFile in Directory.EnumerateFiles(Utils.MediaPath, "*.tmp"))
            {
                File.Delete(tempFile);
            }
        }

        public static async Task DownloadAsync(Track track)
        {
            string id = track.Id;
            string temp;
            if (downloads.ContainsKey(id) || track.TryGetExistingMedia(out temp))
            {
                OnDownloadComplete(id);
                return;
            }

            if (downloads.Count > MAX_CONCURRENT_DOWNLOADS)
            {
                dlQueue.Enqueue(track);
                return;
            }

            downloads.Add(id, (new(), new()));
            var download = downloads[id];

            OnDownloadStarted.Invoke(id);

            download.p.ProgressChanged += (obj, progress) =>
            {
                OnDownloadProgress(id, (float)progress);
            };

            try
            {
                download.cts.Token.ThrowIfCancellationRequested();
                Stopwatch timer = new();
                timer.Start();

                IStreamInfo streamInfo = await track.GetAudioOnlyStreamInfoAsync(download.cts.Token);
                var finalPath = $"{Utils.MediaPath}{id}.{streamInfo.Container.Name}";
                var tempPath =  $"{finalPath}.tmp";

                Utils.CreateDirFromPath(tempPath);
                await FakeYoutube.Instance.Videos.Streams.DownloadAsync(streamInfo, tempPath, download.p, download.cts.Token);

                download.cts.Token.ThrowIfCancellationRequested();

                _ = TextureManager.Texture2DFromUrlAsync(track.LowResThumbnailUrl);
                _ = TextureManager.Texture2DFromUrlAsync(track.HighResThumbnailUrl);

                File.Move(tempPath, finalPath);

                timer.Stop();
                Debug.Log("Download of " + id + " complete, took " + timer.Elapsed.TotalSeconds + "s");
            }
            catch (Exception e)
            {
                if (!download.cts.Token.IsCancellationRequested)
                {
                    Debug.LogException(e);
                    OnDownloadFailed.Invoke(id);
                }

            }
            finally
            {
                downloads.Remove(id);
                OnDownloadComplete.Invoke(id);
                PurgeTempFiles();

                if (dlQueue.TryDequeue(out Track t)) _ = DownloadAsync(t);
            }
        }
    }
}