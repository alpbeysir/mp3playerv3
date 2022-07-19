using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackgroundAudio;
using UnityEngine;

public class AndroidPlayer : AudioPlayer
{
    private BackgroundAudio.Base.BackgroundAudioImplementation player;
    public override float CurPos { get => player.GetCurrentPosition(); set => player.Seek(value); }
    public override float Volume { get => player.GetVolume(); set => player.SetVolume(value); }
    public override string CurFile
    {
        get => _curFile;
        set
        {
            _curFile = value;
            player.Play(_curFile, "Break My Heart", "Dua Lipa");
        }
    }

    public string _curFile;

    public override float Duration => player.GetDuration();

    public override bool IsPaused => player.IsPaused();
    
    public AndroidPlayer()
    {
        if (Application.platform != RuntimePlatform.Android) return;

        player = BackgroundAudioManager.NewInstance();
        player.OnAudioStarted += Player_OnAudioStarted;
        player.OnAudioStopped += Player_OnAudioStopped;
        player.OnAudioPaused += Player_OnAudioPaused;
        player.OnAudioResumed += Player_OnAudioResumed;
    }
    private static void Player_OnAudioResumed() => OnResume?.Invoke();
    private static void Player_OnAudioPaused() => OnPause?.Invoke();
    private static void Player_OnAudioStopped() => OnStop?.Invoke();
    private static void Player_OnAudioStarted() => OnStart?.Invoke();

    public override void Pause() => player.Pause();
    public override void Resume() => player.Resume();

    public override void Dispose()
    {
        player.Stop();
    }
}
