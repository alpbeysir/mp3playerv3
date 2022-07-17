using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using BackgroundAudio;

public class Test : MonoBehaviour
{
    private AndroidPlayer player = new AndroidPlayer();
    public void Start()
    {
        player.Init();
        player.CurFile = Application.persistentDataPath + "/test.webm";
        player.Play();

        // Get an instance of the BackgroundAudioImplementation class for the current build platform
        //var instance = BackgroundAudioManager.NewInstance();

        // To play an mp3 file
        // NOTE: Network playback currently not supported
        //instance.Play(Application.persistentDataPath + "/test.mp3");

        // Callbacks
        // NOTE: Callbacks on Android are not invoked on the main thread (use a main thread dispatcher to update UI)
        //instance.OnAudioStarted += () => Debug.Log("Audio started playing");
        //instance.OnAudioStopped += () => Debug.Log("Audio stopped playing");
        //instance.OnAudioPaused += () => Debug.Log("Audio paused");
        //instance.OnAudioResumed += () => Debug.Log("Audio resumed");
    }

    //public async UniTask TestFunc()
    //{
    //    var youtube = new YoutubeClient();
    //    var streamInfo = (await youtube.Videos.Streams.GetManifestAsync("ux9-_2SN1h0")).GetAudioOnlyStreams().TryGetWithHighestBitrate();
    //    var dl = youtube.Videos.Streams.DownloadAsync(streamInfo, Application.persistentDataPath + "/test." + streamInfo.Container.Name);
    //    await dl;
    //    if (dl.IsCompletedSuccessfully) Debug.Log("Download successful!");
    //}

}
