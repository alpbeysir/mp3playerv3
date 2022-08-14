using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode.Videos.Streams;
using System.Threading;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public static class DownloadManager
{
    public static Action<string> OnDownloadStarted;
    public static Action<string> OnDownloadComplete;
    public static Action<string> OnDownloadFailed;
  
    private static Dictionary<string, (Progress<double> p, CancellationTokenSource cts)> downloads = new();

    public static void CancelDownload(string id)
    {
        downloads[id].cts.Cancel();
    }

    public static bool IsDownloading(string id)
    {
        return downloads.ContainsKey(id);
    }

    public static async Task DownloadAsync(string id, AudioOnlyStreamInfo streamInfo)
    {
        downloads.Add(id, (new(), new()));
        var download = downloads[id];

        try
        {
            download.cts.Token.ThrowIfCancellationRequested();

            Stopwatch timer = new();
            timer.Start();

            var path = Utils.MediaPath + string.Format("{0}.{1}", id, streamInfo.Container.Name);
            Utils.CreateDirFromPath(path);
            await Youtube.Instance.Videos.Streams.DownloadAsync(streamInfo, path, download.p, download.cts.Token);

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
