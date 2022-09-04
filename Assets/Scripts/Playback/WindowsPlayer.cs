#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace MP3Player.Playback
{
    public class WindowsPlayer : AudioPlayer
    {
        private WaveOutEvent output;
        private MediaFoundationReader reader;

        private Mutex fileLock = new Mutex();

        public override float CurPos
        {
            get => reader != null ? (float)reader.CurrentTime.TotalSeconds : 0;
            set
            {
                if (reader != null) reader.CurrentTime = TimeSpan.FromSeconds(value);
            }
        }

        public override void SetDataSource(string uri)
        {
            fileLock.WaitOne();

            //Stop previous playback
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }

            if (output != null)
            {
                output.PlaybackStopped -= StoppedCallback;
                output.Stop();
            }

            reader = new MediaFoundationReader(uri);

            //output = new WaveOutEvent();
            output.Init(reader);
            output.PlaybackStopped += StoppedCallback;

            output.Play();

            OnPrepared?.Invoke();
            OnStart?.Invoke();

            fileLock.ReleaseMutex();
        }

        public override float Volume { get => output.Volume; set => output.Volume = value; }

        public override float Duration => reader != null ? (float)reader.TotalTime.TotalSeconds : 0;

        public override bool IsPaused => output == null ? true : output.PlaybackState == PlaybackState.Paused;

        public override void Pause() { output?.Pause(); OnPause?.Invoke(); }

        public override void Resume() { output?.Play(); OnResume?.Invoke(); }

        private void StoppedCallback(object s, StoppedEventArgs a) => OnStop?.Invoke();

        public bool IsStopped => output == null ? true : output.PlaybackState == PlaybackState.Stopped;

        public override bool IsPrepared => output != null && output.PlaybackState != PlaybackState.Stopped;

        public WindowsPlayer()
        {
            Task.Run(() => { output = new WaveOutEvent(); });
        }
        public override void Dispose()
        {
            output.PlaybackStopped -= StoppedCallback;
            output?.Dispose();
            reader?.Dispose();
        }

    }
}

#endif