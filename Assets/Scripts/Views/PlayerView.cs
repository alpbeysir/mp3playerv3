using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Cysharp.Threading.Tasks;
using MP3Player.Managers;
using MP3Player.Components;
using MP3Player.Playback;
using System.Threading;

namespace MP3Player.Views
{
    public class PlayerView : UIView
    {
        [SerializeField] private RectTransform imageParent;
        [SerializeField] private ImageView imageView;
        [SerializeField] private VideoView videoView;
        [SerializeField] private TextMeshProUGUI titleDisplay;
        [SerializeField] private TextMeshProUGUI channelDisplay;
        [SerializeField] private Button[] controlButtons;
        [SerializeField] private Slider seekBar;

        [SerializeField] private TextMeshProUGUI currentTimeDisplay, durationDisplay;
        [SerializeField] private GameObject playIcon, pauseIcon;

        private float targetImageParentHeight;

        private void Start()
        {
            PlayerController.Initialize();

            PlayerController.OnPlayerStateChanged += OnPlayerStateChanged;
            PlayerController.OnTrackChanged += OnTrackChanged;

            targetImageParentHeight = imageParent.rect.height;
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
            
            if (Mathf.Abs(imageParent.rect.height - targetImageParentHeight) > 5)
            {
                Vector2 v = imageParent.sizeDelta;
                v.y = Mathf.Lerp(v.y, targetImageParentHeight, 0.2f);
                imageParent.sizeDelta = v;
                LayoutRebuilder.MarkLayoutForRebuild((RectTransform)imageParent.parent.transform);
            }

            //Keyboard playback controls
            if (Input.GetKeyDown(KeyCode.Space))
                PlayPausePressed();

            if (Input.GetKeyDown(KeyCode.LeftArrow))
                PreviousPressed();

            if (Input.GetKeyDown(KeyCode.RightArrow))
                NextPressed();
        }

        private void OnTrackChanged()
        {
            SetPlayerControlInteractivity(true);

            var track = PlayerController.Current;

            imageView.SetLoading();
            _ = TextureManager.Texture2DFromUrlAsync(track.HighResThumbnailUrl).ContinueWith(mt =>
            {
                imageView.SetImage(mt);
                TranslucentBackground.Instance.SetImage(mt);
            });

            titleDisplay.text = track.Title;
            channelDisplay.text = track.ChannelName;
            durationDisplay.text = new TimeSpan(long.Parse(track.Duration)).ToString("mm\\:ss");

            if (videoView.gameObject.activeSelf)
                videoView.RequestTrackUpdate();
            else
                videoView.Release();
        }

        private void OnPlayerStateChanged()
        {
            playIcon.SetActive(PlayerController.IsPaused);
            pauseIcon.SetActive(!PlayerController.IsPaused);

            if (PlayerController.IsPaused)
                videoView.Pause();
            else
                videoView.Play();
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
            _ = PlayerController.RequestTrackChange(TrackChangeDirection.Previous);
        }
        public void PlayPausePressed()
        {
            if (PlayerController.IsPaused) PlayerController.Resume();
            else PlayerController.Pause();
        }
        public void NextPressed()
        {
            SetPlayerControlInteractivity(false);
            _ = PlayerController.RequestTrackChange(TrackChangeDirection.Next);
        }
        public void SeekBarPressed()
        {
            PlayerController.CurPos = seekBar.value * PlayerController.Duration;
        }

        public void ImagePressed()
        {
            imageView.transform.parent.gameObject.SetActive(!imageView.transform.parent.gameObject.activeSelf);
            videoView.gameObject.SetActive(!videoView.gameObject.activeSelf);

            if (videoView.gameObject.activeSelf) 
            {
                targetImageParentHeight = 350;

                videoView.RequestTrackUpdate();
            }
            else
            {
                targetImageParentHeight = 250;
            }
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
}