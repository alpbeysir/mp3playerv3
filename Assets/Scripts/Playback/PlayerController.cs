using System.Threading;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using MP3Player.Managers;
using MP3Player.Models;

namespace MP3Player.Playback
{
    public enum TrackChangeDirection
    {
        Previous, Current, Next
    }

    public static class PlayerController
    {
        public static Action OnTrackChanged;
        public static Action OnPlayerStateChanged;

        public static float CurPos { get => player.CurPos; set => player.CurPos = value; }
        public static bool IsPaused => player.IsPaused;
        public static float Duration => duration;
        public static void Pause() => player.Pause();
        public static void Resume() => player.Resume();

        private static AudioPlayer player;
        private static CancellationTokenSource cts;

        private static PlayerState state;

        private const int MAX_HISTORY_LEN = 250;

        public static Track Current => Track.Get(state.Current);

        private static float duration;
        private static float posCache;

        public static void SetPlaylist(Playlist _playlist)
        {
            state.Playlist = _playlist.Id;
            PlayOverride(Playlist.Get(state.Playlist).GetCurrent());
            Playlist.Get(state.Playlist).Next();
            state.Save();
        }

        public static void PlayOverride(Track track)
        {
            state.PlayQueue.Insert(0, track.Id);
            RequestTrackChange(TrackChangeDirection.Next);
            state.Save();
        }

        public static void AddToQueue(Track track)
        {
            state.PlayQueue.Add(track.Id);
            if (Current == null)
            {
                RequestTrackChange(TrackChangeDirection.Next);
            }
            state.Save();
        }
        public static bool IsInQueue(Track track) => state.PlayQueue.Contains(track.Id);

        public static void RequestTrackChange(TrackChangeDirection dir)
        {
            switch (dir)
            {
                case TrackChangeDirection.Previous:
                    state.PlayQueue.Insert(0, Current.Id);
                    state.Current = state.History[state.History.Count - 1];
                    state.History.RemoveAt(state.History.Count - 1);
                    break;
                case TrackChangeDirection.Current:
                    break;
                case TrackChangeDirection.Next:
                    if (Current != null)
                    {
                        state.History.Add(Current.Id);
                        if (state.History.Count > MAX_HISTORY_LEN) state.History.RemoveRange(0, state.History.Count - MAX_HISTORY_LEN);
                    }
                    state.Current = GetNextTrack().Id;
                    break;
            }

            state.Save();

            if (cts.IsCancellationRequested) return;
            cts.Cancel();
            cts = new CancellationTokenSource();
            Task.Run(() => ChangeTrack(Current, cts.Token), cts.Token);
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
            state = PlayerState.Get("state");
            RequestTrackChange(TrackChangeDirection.Current);
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
            RequestTrackChange(TrackChangeDirection.Next);
        }
        private static void OnPlayerPrepared()
        {
            duration = player.Duration;
            if (posCache != 0)
            {
                player.CurPos = posCache;
                posCache = 0;
            }
            player.Resume();
        }

        private static Track GetNextTrack()
        {
            if (state.PlayQueue.Count > 0)
            {
                var trackId = state.PlayQueue[0];
                state.PlayQueue.RemoveAt(0);
                state.Save();
                return Track.Get(trackId);
            }
            else
            {
                var track = Playlist.Get(state.Playlist).GetCurrent();
                if (track == null)
                {
                    //TODO handle end of playlist (switch to recommendations) for now just restart playlist
                    Debug.Log("Reached playlist end");
                    Playlist.Get(state.Playlist).ResetPosition();
                    return Playlist.Get(state.Playlist).GetCurrent();
                }
                Playlist.Get(state.Playlist).Next();
                state.Save();
                return track;
            }
        }

        private static void OnDownloadComplete(string id)
        {
            if (Current != null && Current.Id == id)
            {
                posCache = player.CurPos;
                RequestTrackChange(TrackChangeDirection.Current);
            }
        }

        private static void ChangeTrack(Track track, CancellationToken token)
        {
            state.Current = track.Id;
            token.ThrowIfCancellationRequested();
            try
            {
                UniTask.Post(OnTrackChanged);
                var mediaUri = track.GetMediaUri(token).Result;
                token.ThrowIfCancellationRequested();

                //TODO refactor notification back into original format, current design causes issues
                if (Application.platform == RuntimePlatform.Android)
                    AndroidPlayer.SetNotificationData(Current.Title, Current.ChannelName, TextureManager.TryGetLocalUri(Current.LowResThumbnailUrl));

                player.SetDataSource(mediaUri);
                UniTask.Post(OnPlayerStateChanged);
            }
            catch (Exception e)
            {
                //TODO skip track if unloadable
                if (!e.GetBaseException().IsOperationCanceledException())
                {
                    Debug.LogError("Error while loading track!");
                    Debug.LogException(e);
                    RequestTrackChange(TrackChangeDirection.Next);
                }
            }
        }
    }
}