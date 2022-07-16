using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

public class Test : MonoBehaviour
{
    public void Start()
    {
        _ = TestFunc();
    }

    public async UniTask TestFunc()
    {
        var youtube = new YoutubeClient();
        var videos = youtube.Search.GetVideosAsync("time to make history").GetAsyncEnumerator();
        await videos.MoveNextAsync();
        await videos.MoveNextAsync();
        await videos.MoveNextAsync();
        await videos.MoveNextAsync();
        Debug.Log(videos.Current.Title);
        var streamInfo = (await youtube.Videos.Streams.GetManifestAsync(videos.Current.Id)).GetAudioOnlyStreams().TryGetWithHighestBitrate();
        var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
        var dl = youtube.Videos.Streams.DownloadAsync(streamInfo, Application.persistentDataPath + "/test." + streamInfo.Container.Name);
        await dl;
        if (dl.IsCompletedSuccessfully) Debug.Log("Download successful!");
    }
    
}
