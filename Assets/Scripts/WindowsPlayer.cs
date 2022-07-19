using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NAudio.Wave;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
public class WindowsPlayer : AudioPlayer
{
    private WaveOutEvent output = new WaveOutEvent();
    private AudioFileReader reader;
    
    public override float CurPos { get => (float)reader.CurrentTime.TotalSeconds; set => reader.CurrentTime = TimeSpan.FromSeconds(value); }
    public override string CurFile {
        get => reader.FileName;
        set
        {
            if (reader != null) Dispose();
            reader = new AudioFileReader(value);
            output.Init(reader);
            output.Play();
        }
    }
    public override float Volume { get => output.Volume; set => output.Volume = value; }

    public override float Duration => (float)reader.TotalTime.TotalSeconds;

    public override bool IsPaused => output.PlaybackState == PlaybackState.Paused;

    public override void Pause() => output.Pause();

    public override void Resume() => output.Play();

    private void StoppedCallback(object s, StoppedEventArgs a) => OnStop?.Invoke();

    public WindowsPlayer()
    {
        output.PlaybackStopped += StoppedCallback;
    }
    public override void Dispose()
    {
        output.PlaybackStopped -= StoppedCallback;
        output.Dispose();
        reader.Dispose();
    }

}
#endif