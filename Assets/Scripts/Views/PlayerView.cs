using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public class PlayerView : UIScreen
{
    [SerializeField] private ImageView thumbnail;
    [SerializeField] private TextMeshProUGUI titleDisplay;
    [SerializeField] private TextMeshProUGUI channelDisplay;
    [SerializeField] private Button[] controlButtons;
    [SerializeField] private Slider seekBar;
    [SerializeField] private TextMeshProUGUI currentTimeDisplay, durationDisplay;
    [SerializeField] private GameObject playIcon, pauseIcon;

    private void Start()
    {
        PlayerManager.Initialize();

        PlayerManager.OnPlayerStateChanged += OnPlayerStateChanged;
        PlayerManager.OnTrackChanged += OnTrackChanged;
    }

    private void OnTrackChanged()
    {
        UpdateTrackDisplay(PlayerManager.Current);
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
        float ratio = PlayerManager.CurPos / PlayerManager.Duration;
        if (0 <= ratio && ratio <= 1) { 
            seekBar.SetValueWithoutNotify(ratio);
            currentTimeDisplay.text = TimeSpan.FromSeconds(PlayerManager.CurPos).ToString("mm\\:ss");
        }
        else
        {
            seekBar.SetValueWithoutNotify(0);
            currentTimeDisplay.text = TimeSpan.FromSeconds(0).ToString("mm\\:ss");
        }
    }
    
    public void PreviousPressed()
    {
        SetPlayerControlInteractivity(false);
        PlayerManager.RequestTrackChange(Direction.Previous);
    } 
    public void PlayPausePressed()
    {
        if (PlayerManager.IsPaused) PlayerManager.Resume();
        else PlayerManager.Pause();
    } 
    public void NextPressed()
    {
        SetPlayerControlInteractivity(false);
        PlayerManager.RequestTrackChange(Direction.Next);
    }   
    public void SeekBarPressed()
    {
        PlayerManager.CurPos = seekBar.value * PlayerManager.Duration;
    }  

    private void UpdatePlayPauseState()
    {
        playIcon.SetActive(PlayerManager.IsPaused);
        pauseIcon.SetActive(!PlayerManager.IsPaused);
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
        PlayerManager.OnPlayerStateChanged -= OnPlayerStateChanged;
        PlayerManager.OnTrackChanged -= OnTrackChanged;
        PlayerManager.Dispose();
    }
}
