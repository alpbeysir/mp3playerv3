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

    private AudioPlayer player;

    public static Playlist playlist = new();

    private MediaInfo currentInfo;
    private MediaInfo lastInfo;

    private float duration;
    private bool uiUpdateRequested;
    private bool playerControlsEnabled;

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

        //DownloadManager.OnDownloadComplete += OnDownloadComplete;
    }

    private void OnDownloadComplete(string id)
    {
        if (currentInfo != null && currentInfo.metadata.id == id)
        {
            var posCache = player.CurPos;
            player.Pause();
            StartPlayerThread();
        }
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

    private CancellationTokenSource cts = new();
    public void StartPlayerThread()
    {
        cts.Cancel();
        cts = new CancellationTokenSource();
        Task.Run(() => UpdatePlayerState(cts.Token));
    }

    private void UpdatePlayerState(CancellationToken token)
    {
        if (playlist.Length == 0) return;

        try
        {
            playerControlsEnabled = false;

            token.ThrowIfCancellationRequested();

            var meta = Cache.GetOrCreate<Metadata>(playlist.GetCurrent()).Result;

            currentInfo = MediaInfo.Creator(meta, token).Result;

            token.ThrowIfCancellationRequested();

            uiUpdateRequested = true;

            if (Application.platform == RuntimePlatform.Android)
                AndroidPlayer.SetNotifData(currentInfo.metadata.title, currentInfo.metadata.channelName, currentInfo.metadata.sdThumbnailUrl);

            player.CurFile = currentInfo.mediaUri;
        }
        catch (Exception e)
        {
            Debug.LogError("An error occurred in UpdatePlayerStateAsync!\n" + e.ToString());
            playerControlsEnabled = true;
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
        
        _ = thumbnail.Set(currentInfo.metadata.hdThumbnailUrl);
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
        playlist = new();
        currentInfo = null;
        lastInfo = null;
    }
}
