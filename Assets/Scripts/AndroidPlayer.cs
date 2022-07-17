using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackgroundAudio;

public class AndroidPlayer : IAudioPlayer
{
    private BackgroundAudio.Base.BackgroundAudioImplementation player;
    public float CurPos { get => player.GetCurrentPosition(); set => player.Seek(value); }
    public float Volume { get => player.GetVolume(); set => player.SetVolume(value); }
    public string CurFile { get; set; }

    public float Duration => player.GetDuration();

    public event PlayerEvent OnStart;
    public event PlayerEvent OnStop;
    public event PlayerEvent OnPause;
    public event PlayerEvent OnResume;

    public void Init()
    {
        player = BackgroundAudioManager.NewInstance();
        //player.OnAudioStarted += OnStart.Invoke;
        //player.OnAudioStopped += OnStop.Invoke;
        //player.OnAudioPaused += OnPause.Invoke;
        //player.OnAudioResumed += OnResume.Invoke;
    }

    public void Pause() => player.Pause();
    public void Play() => player.Play(CurFile);
}
