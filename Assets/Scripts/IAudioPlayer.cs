using System;

public interface IAudioPlayer
{
    public void Init();
    public void Play();
    public void Pause();
    public float CurPos { get; set; }
    public string CurFile { get; set; }
    public float Volume { get; set; }

    public float Duration { get; }

    public event PlayerEvent OnStart;
    public event PlayerEvent OnStop;
    public event PlayerEvent OnPause;
    public event PlayerEvent OnResume;
}

public delegate void PlayerEvent();