using System.Threading;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Collections.Generic;

public enum Direction
{
    Previous, Current, Next
}

public static class PlayerManager
{
    public static Action OnTrackChanged;
    public static Action OnPlayerStateChanged;

    public static float CurPos { get => player.CurPos; set => player.CurPos = value; }
    public static bool IsPaused => player.IsPaused;
    public static float Duration => duration;
    public static void Pause() => player.Pause();
    public static void Resume() => player.Resume();

    public static Track current;

    private static AudioPlayer player;
    private static CancellationTokenSource cts;

    private static List<Track> history;
    private static Playlist playlist;
    private static List<Track> playQueue;

    private static float duration;
    private static float posCache;

    public static void SetPlaylist(Playlist _playlist)
    {
        playlist = _playlist;
        PlayOverride(playlist.GetCurrent());
        playlist.Next();
    }

    public static void PlayOverride(Track track)
    {
        playQueue.Insert(0, track);
        RequestTrackChange(Direction.Next);
    }

    public static void AddToQueue(Track track)
    {
        playQueue.Add(track);
        if (current == null)
        {
            RequestTrackChange(Direction.Next);
        }
    }
    public static bool IsInQueue(Track track) => playQueue.Contains(track);

    public static void RequestTrackChange(Direction dir)
    {
        switch (dir)
        {
            case Direction.Previous:
                playQueue.Insert(0, current);
                current = history[history.Count - 1];
                history.RemoveAt(history.Count - 1);
                break;
            case Direction.Current:
                break;
            case Direction.Next:
                if (current != null)
                    history.Add(current);
                current = GetNextTrack();
                break;
        }

        if (cts.IsCancellationRequested) return;
        cts.Cancel();
        cts = new CancellationTokenSource();
        Task.Run(() => ChangeTrack(current, cts.Token), cts.Token);
    }

    public static void Initialize()
    {
        if (Application.platform == RuntimePlatform.Android) player = new AndroidPlayer();
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) player = new WindowsPlayer();
        #endif
        else Debug.LogError("Platform not supported!");

        AudioPlayer.OnStop += OnPlayerStop;
        AudioPlayer.OnResume += OnPlayerResume;
        AudioPlayer.OnPause += OnPlayerPause;
        AudioPlayer.OnPrepared += OnPlayerPrepared;
        DownloadManager.OnDownloadComplete += OnDownloadComplete;

        cts = new();

        //TODO load from database here
        playQueue = new();
        history = new();
    }

    public static void Dispose()
    {
        player.Dispose();
        cts.Cancel();
        cts.Dispose();

        AudioPlayer.OnStop -= OnPlayerStop;
        AudioPlayer.OnResume -= OnPlayerResume;
        AudioPlayer.OnPause -= OnPlayerPause;
        AudioPlayer.OnPrepared = OnPlayerPrepared;
        DownloadManager.OnDownloadComplete -= OnDownloadComplete;

        current = null;
    }

    private static void OnPlayerResume()
    {
        UniTask.Post(OnPlayerStateChanged);
    }
    private static void OnPlayerPause()
    {
        UniTask.Post(OnPlayerStateChanged);
    }
    private static void OnPlayerStop()
    {
        RequestTrackChange(Direction.Next);
    }
    private static void OnPlayerPrepared()
    {
        duration = player.Duration;
        if (posCache != 0)
        {
            player.CurPos = posCache;
            posCache = 0;
        }
    }

    private static Track GetNextTrack()
    {
        if (playQueue.Count > 0)
        {
            var track = playQueue[0];
            playQueue.RemoveAt(0);
            return track;
        }
        else
        {
            var track = playlist.GetCurrent();
            if (track == null)
            {
                //TODO handle end of playlist (switch to recommendations) for now just restart playlist
                Debug.Log("Reached playlist end");
                playlist.ResetPosition();
                return playlist.GetCurrent();
            }
            playlist.Next();
            return track;
        }
    }

    private static void OnDownloadComplete(string id)
    {
        if (current != null && current.Id == id)
        {
            posCache = player.CurPos;
            RequestTrackChange(Direction.Current);
        }
    }

    private static void ChangeTrack(Track track, CancellationToken token)
    {
        current = track;
        token.ThrowIfCancellationRequested();
        try
        {
            UniTask.Post(OnTrackChanged);
            var mediaUri = track.GetMediaUri(token).Result;
            token.ThrowIfCancellationRequested();

            //TODO refactor notification back into original format, current design causes issues
            if (Application.platform == RuntimePlatform.Android)
                AndroidPlayer.ShowNotification(current.Title, current.ChannelName, TextureUtils.TryGetLocalUri(current.LowResThumbnailUrl));

            player.CurFile = mediaUri;
            UniTask.Post(OnPlayerStateChanged);
        }
        catch (Exception e)
        {
            //TODO skip track if unloadable
            if (!e.GetBaseException().IsOperationCanceledException()) 
            { 
                Debug.LogException(e);
                Debug.Log("Error while loading track");
            }
        }
    }
}
