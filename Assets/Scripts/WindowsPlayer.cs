using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
public class WindowsPlayer : AudioPlayer
{
    private WaveOutEvent output = new WaveOutEvent();
    private MediaFoundationReader reader;
    
    public override float CurPos
    { 
        get => reader != null ? (float)reader.CurrentTime.TotalSeconds : 0;
        set 
        { 
            if (reader != null) reader.CurrentTime = TimeSpan.FromSeconds(value); 
        } 
    }
    
    public override string CurFile {
        get => "";
        set
        {
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

            
            reader = new MediaFoundationReader(value);

            output = new WaveOutEvent();
            output.Init(reader);
            output.PlaybackStopped += StoppedCallback;

            output.Play();
            OnPrepared?.Invoke();
            OnStart?.Invoke();
        }
    }

    public override float Volume { get => output.Volume; set => output.Volume = value; }

    public override float Duration => reader != null ? (float)reader.TotalTime.TotalSeconds : 0;

    public override bool IsPaused => output.PlaybackState == PlaybackState.Paused;

    public override void Pause() { output.Pause(); OnPause?.Invoke(); }

    public override void Resume() { output.Play(); OnResume?.Invoke(); }

    private void StoppedCallback(object s, StoppedEventArgs a) => OnStop?.Invoke();

    public bool IsStopped => output.PlaybackState == PlaybackState.Stopped;

    public WindowsPlayer()
    {
        //output.PlaybackStopped += StoppedCallback;
    }
    public override void Dispose()
    {
        output.PlaybackStopped -= StoppedCallback;
        output?.Dispose();
        reader?.Dispose();
    }

}
#endif