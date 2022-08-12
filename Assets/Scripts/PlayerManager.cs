using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks; 
using System;

public class PlayerManager : UIScreen
{
    [SerializeField] private NetworkedSprite thumbnail;
    [SerializeField] private TextMeshProUGUI titleDisplay;
    [SerializeField] private TextMeshProUGUI channelDisplay;
    [SerializeField] private Button[] controlButtons;
    [SerializeField] private Slider seekBar;
    [SerializeField] private GameObject playIcon, pauseIcon;

    private static AudioPlayer player;

    public static Playlist playlist = new();

    private MediaInfo currentInfo;
    private MediaInfo lastInfo;

    private float duration;
    private bool uiUpdateRequested;
    private bool playerControlsEnabled;

    private Thread playerThread;

    public void Start()
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
    }

    public override void Show()
    {
        if (currentInfo == null && playlist.Length > 0)
            StartPlayerThread();
    }

    public override void Hide() { }

    private void Update()
    {
        float asd = player.CurPos / duration;
        if (0 <= asd && asd <= 1) seekBar.SetValueWithoutNotify(asd);

        SetPlayerControlInteractivity(playerControlsEnabled);
        if (uiUpdateRequested)
        {
            uiUpdateRequested = false;
            UpdateMediaInfoDisplays();
            UpdatePlayPauseState();
        }
    }
    
    public void PreviousPressed()
    {
        player.Pause();
        playlist.Previous();
        StartPlayerThread();
    } 
    public void PlayPausePressed()
    {
        if (player.IsPaused) player.Resume();
        else player.Pause();
    } 
    //May not be called on UnityMain
    public void NextPressed()
    {
        player.Pause();
        playlist.Next();
        StartPlayerThread();
    }   
    public void SeekBarPressed()
    {
        player.CurPos = seekBar.value * duration;
    }
    
    //Not called on UnityMain
    public void OnPlayerResume()
    {
        uiUpdateRequested = true;
    }
    //Not called on UnityMain
    public void OnPlayerPause()
    {
        uiUpdateRequested = true;
    }
    //Not called on UnityMain
    public void OnPlayerStop()
    {
        playlist.Next();
        StartPlayerThread();
    }   
    public void OnPlayerPrepared()
    {
        duration = player.Duration;
        playerControlsEnabled = true;
        uiUpdateRequested = true;
    }
    public void StartPlayerThread()
    {
        playerThread?.Abort();
        playerThread = new Thread(UpdatePlayerState);
        playerThread.Start();
    }
    
    private void UpdatePlayerState()
    {
        if (playlist.Length == 0) return;

        try
        {
            playerControlsEnabled = false;
            
            currentInfo = MediaInfo.Creator(Cache.GetOrCreate<Metadata>(playlist.GetCurrent()).Result).Result;
            if (currentInfo == null) throw new Exception("currentInfo cannot be null!");
            
            uiUpdateRequested = true;

            if (Application.platform == RuntimePlatform.Android)
            {
                //Need to attach to JVM before calling Java code
                AndroidJNI.AttachCurrentThread();

                AndroidPlayer.title = currentInfo.metadata.title;
                AndroidPlayer.desc = currentInfo.metadata.channelName;
                AndroidPlayer.iconUri = currentInfo.metadata.sdThumbnailUrl;
            }

            player.CurFile = currentInfo.mediaUri;
        }
        catch (Exception e)
        {
            Debug.LogError("An error occured in SetMediaInfo!\n" + e.Message);
            playerControlsEnabled = true;

        }
        finally
        {
            if (Application.platform == RuntimePlatform.Android)
                AndroidJNI.DetachCurrentThread();
        }
    }

    private void UpdatePlayPauseState()
    {
        playIcon.SetActive(player.IsPaused);
        pauseIcon.SetActive(!player.IsPaused);
    }

    private void UpdateMediaInfoDisplays()
    {
        if (currentInfo == null) return;
        
        thumbnail.ShowLoading(null);
        _ = thumbnail.Set(currentInfo.metadata.sdThumbnailUrl);
        titleDisplay.text = currentInfo.metadata.title;
        channelDisplay.text = currentInfo.metadata.channelName;
        lastInfo = currentInfo;
    }

    private void SetPlayerControlInteractivity(bool state)
    {
        seekBar.interactable = state;
        foreach (var b in controlButtons) b.interactable = state;
    }

    public void OnApplicationFocus(bool focus)
    {
        if (focus && lastInfo != currentInfo)
        {
            UpdateMediaInfoDisplays();
            UpdatePlayPauseState();
        }
    }

    public void OnDestroy()
    {
        player.Dispose();
        currentInfo = null;
        lastInfo = null;
    }
}
