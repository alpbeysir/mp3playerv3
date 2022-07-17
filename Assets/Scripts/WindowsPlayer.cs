using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackgroundAudio;

public class WindowsPlayer : IAudioPlayer
{
    public float CurPos { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string CurFile { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public float Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public float Duration => throw new NotImplementedException();

    public event PlayerEvent OnStart;
    public event PlayerEvent OnStop;
    public event PlayerEvent OnPause;
    public event PlayerEvent OnResume;

    public void Init()
    {
        throw new NotImplementedException();
    }

    public void Pause()
    {
        throw new NotImplementedException();
    }

    public void Play()
    {
        throw new NotImplementedException();
    }
}