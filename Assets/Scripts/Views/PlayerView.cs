using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Cysharp.Threading.Tasks;
using MP3Player.Models;
using MP3Player.Managers;
using MP3Player.Components;
using MP3Player.Playback;
using System.Threading.Tasks;

namespace MP3Player.Views
{
    public class PlayerView : UIView
    {
        [SerializeField] private ImageView thumbnail;
        [SerializeField] private VideoView videoView;
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
            SetPlayerControlInteractivity(true);

            var track = PlayerController.Current;

            thumbnail.SetLoading();
            //TranslucentBackground.Instance.SetLoading();
            _ = TextureManager.Texture2DFromUrlAsync(track.HighResThumbnailUrl).ContinueWith(mt =>
            {
                thumbnail.SetImage(mt);
                TranslucentBackground.Instance.SetImage(mt);
            });

            titleDisplay.text = track.Title;
            channelDisplay.text = track.ChannelName;
            durationDisplay.text = new TimeSpan(long.Parse(track.Duration)).ToString("mm\\:ss");

            _ = videoView.SetTrack(track);
        }

        private void Update()
        {
            float ratio = PlayerController.CurPos / PlayerController.Duration;
            if (0 <= ratio && ratio <= 1)
            {
                seekBar.SetValueWithoutNotify(ratio);
                currentTimeDisplay.text = TimeSpan.FromSeconds(PlayerController.CurPos).ToString("mm\\:ss");
            }
            else
            {
                seekBar.SetValueWithoutNotify(0);
                currentTimeDisplay.text = TimeSpan.FromSeconds(0).ToString("mm\\:ss");
            }
       
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
        public void PreviousPressed()
        {
            SetPlayerControlInteractivity(false);
            PlayerController.RequestTrackChange(TrackChangeDirection.Previous);
        }
        public void PlayPausePressed()
        {
            if (PlayerController.IsPaused) PlayerController.Resume();
            else PlayerController.Pause();
        }
        public void NextPressed()
        {
            SetPlayerControlInteractivity(false);
            PlayerController.RequestTrackChange(TrackChangeDirection.Next);
        }
        public void SeekBarPressed()
        {
            PlayerController.CurPos = seekBar.value * PlayerController.Duration;
        }

        private void UpdatePlayPauseState()
        {
            playIcon.SetActive(PlayerController.IsPaused);
            pauseIcon.SetActive(!PlayerController.IsPaused);

            if (PlayerController.IsPaused)
                videoView.Pause();
            else 
                videoView.Play();
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

        private void OnEnable()
        {
            videoView.Play();
        }
    }
}