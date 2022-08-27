﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode.Videos.Streams;
using System.Threading;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;
using Cysharp.Threading.Tasks;
using System.IO;

public static class DownloadManager
{
    public static Action<string> OnDownloadStarted;
    public static Action<string> OnDownloadComplete;
    public static Action<string> OnDownloadFailed;
    public static Action<string, float> OnDownloadProgress;
  
    private static Dictionary<string, (Progress<double> p, CancellationTokenSource cts)> downloads = new();

    public static void CancelDownload(string id)
    {
        downloads[id].cts.Cancel();
    }

    public static bool IsDownloading(string id)
    {
        return downloads.ContainsKey(id);
    }

    public static void Delete(Track track)
    {
        if (track.AvailableOffline()) File.Delete(track.GetMediaUri().Result);
    }

    public static async Task DownloadAsync(Track track, AudioOnlyStreamInfo streamInfo = null)
    {
        string id = track.Id;
        string temp;
        if (downloads.ContainsKey(id) || track.TryGetExistingMedia(out temp))
        {
            OnDownloadComplete(id);
            return;
        }

        downloads.Add(id, (new(), new()));
        var download = downloads[id];

        download.p.ProgressChanged += (obj, progress) =>
        {
            OnDownloadProgress(id, (float)progress);
        };

        try
        {
            download.cts.Token.ThrowIfCancellationRequested();
            Stopwatch timer = new();
            timer.Start();

            if (streamInfo == null) streamInfo = await track.GetStreamInfoAsync(download.cts.Token);
            var path = Utils.MediaPath + string.Format("{0}.{1}", id, streamInfo.Container.Name);
            Utils.CreateDirFromPath(path);
            await Youtube.Instance.Videos.Streams.DownloadAsync(streamInfo, path, download.p, download.cts.Token);

            //Download thumbnails as well, will cause memory leak!!! Need to refactor TextureUtils anyway
            _ = TextureUtils.Texture2DFromUrlAsync(track.LowResThumbnailUrl).ContinueWith(tex => UnityEngine.Object.DestroyImmediate(tex));
            _ = TextureUtils.Texture2DFromUrlAsync(track.HighResThumbnailUrl).ContinueWith(tex => UnityEngine.Object.DestroyImmediate(tex));

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
        }
    }
}