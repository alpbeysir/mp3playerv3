using System;

public abstract class AudioPlayer : IDisposable
{
    public abstract void Resume();
    public abstract void Pause();
    public abstract void Dispose();
    public abstract void SetDataSource(string uri);

    public abstract float CurPos { get; set; }
    public abstract float Volume { get; set; }
    public abstract bool IsPaused { get; }

    public abstract float Duration { get; }

    public abstract bool IsPrepared { get; }

    public static Action OnStart;
    public static Action OnStop;
    public static Action OnPause;
    public static Action OnResume;
    public static Action OnPrepared;
}