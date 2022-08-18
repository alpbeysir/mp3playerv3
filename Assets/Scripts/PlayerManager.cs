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
    [SerializeField] private TextMeshProUGUI currentTimeDisplay, durationDisplay;
    [SerializeField] private GameObject playIcon, pauseIcon;

    private void Start()
    {
        PlayerController.Initialize();

        PlayerController.OnPlayerStateChanged += OnPlayerStateChanged;
        PlayerController.OnTrackChanged += OnTrackChanged;
    }

    private void OnTrackChanged()
    {
        UpdateTrackDisplay(PlayerController.current);
        SetPlayerControlInteractivity(true);
    }

    private void OnPlayerStateChanged()
    {
        UpdatePlayPauseState();
    }

    public override void Show(params object[] args)
    {
        MiniplayerView.Instance.Hide();
    }
    public override void Hide()
    {
        MiniplayerView.Instance.Show();
    }

    private void Update()
    {
        float ratio = PlayerController.CurPos / PlayerController.Duration;
        if (0 <= ratio && ratio <= 1) { 
            seekBar.SetValueWithoutNotify(ratio);
            currentTimeDisplay.text = TimeSpan.FromSeconds(PlayerController.CurPos).ToString("mm\\:ss");
        }
    }
    
    public void PreviousPressed()
    {
        SetPlayerControlInteractivity(false);
        PlayerController.RequestTrackChange(Direction.Previous);
    } 
    public void PlayPausePressed()
    {
        if (PlayerController.IsPaused) PlayerController.Resume();
        else PlayerController.Pause();
    } 
    //May not be called on UnityMain
    public void NextPressed()
    {
        SetPlayerControlInteractivity(false);
        PlayerController.RequestTrackChange(Direction.Next);
    }   
    public void SeekBarPressed()
    {
        PlayerController.CurPos = seekBar.value * PlayerController.Duration;
    }  

    private void UpdatePlayPauseState()
    {
        playIcon.SetActive(PlayerController.IsPaused);
        pauseIcon.SetActive(!PlayerController.IsPaused);
    }

    private void UpdateTrackDisplay(Track track)
    {
        _ = thumbnail.Set(track.HighResThumbnailUrl);
        titleDisplay.text = track.Title;
        channelDisplay.text = track.ChannelName;
        durationDisplay.text = new TimeSpan(long.Parse(track.Duration)).ToString("mm\\:ss");
    }

    private void SetPlayerControlInteractivity(bool state)
    {
        seekBar.interactable = state;
        foreach (var b in controlButtons) b.interactable = state;
    }

    private void OnDestroy()
    {
        PlayerController.OnPlayerStateChanged -= OnPlayerStateChanged;
        PlayerController.OnTrackChanged -= OnTrackChanged;
        PlayerController.Dispose();
    }
}
