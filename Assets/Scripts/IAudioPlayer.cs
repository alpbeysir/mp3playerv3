using System;
using System.Collections.Generic;

public interface IAudioPlayer
{
    public void Init();
    public void Play();
    public void Pause();
    public float CurPos { get; set; }
    public Metadata CurTrack { get; set; }
    public float Volume { get; set; }  
}
