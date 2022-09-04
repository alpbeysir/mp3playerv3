using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using MP3Player.Models;
using MP3Player.Misc;
using MP3Player.Managers;
using MP3Player.Components;
using MP3Player.Playback;

namespace MP3Player.Views
{
    public class MiniplayerView : Singleton<MiniplayerView>, IPointerClickHandler
    {
        [SerializeField] private ImageView thumbnail;
        [SerializeField] private TextMeshProUGUI titleDisplay;
        [SerializeField] private TextMeshProUGUI channelDisplay;
        [SerializeField] private Image fillBg;
        [SerializeField] private Button[] controlButtons;
        [SerializeField] private GameObject playIcon, pauseIcon;
        [SerializeField] private PlayerView playerView;
        [SerializeField] private RectTransform viewsParent;

        private float targetViewParentHeight;
        void Start()
        {
            PlayerController.OnPlayerStateChanged += OnPlayerStateChanged;
            PlayerController.OnTrackChanged += OnTrackChanged;
        }

        private void Update()
        {
            //Collapse views to accomodate for miniplayer
            //Debug.Log(viewsParent.sizeDelta);
            if (viewsParent.offsetMin.y != targetViewParentHeight)
            {
                var z = viewsParent.offsetMin;
                z.y = Mathf.Lerp(z.y, targetViewParentHeight, 10f * Time.deltaTime);
                viewsParent.offsetMin = z;
                LayoutRebuilder.MarkLayoutForRebuild(viewsParent);
            }

            float ratio = PlayerController.CurPos / PlayerController.Duration;
            if (0 <= ratio && ratio <= 1)
                fillBg.fillAmount = ratio;
            else
                fillBg.fillAmount = 0;
        }
        private void OnTrackChanged()
        {
            UpdateTrackDisplay(PlayerController.Current);
            SetPlayerControlInteractivity(true);
            if (!playerView.gameObject.activeSelf)
                Show();
        }

        private void OnPlayerStateChanged()
        {
            UpdatePlayPauseState();
        }

        private void UpdateTrackDisplay(Track track)
        {
            thumbnail.SetLoading();
            _ = TextureManager.Texture2DFromUrlAsync(track.HighResThumbnailUrl).ContinueWith(tex => thumbnail.SetImage(tex));

            titleDisplay.text = track.Title;
            channelDisplay.text = track.ChannelName;
        }

        private void SetPlayerControlInteractivity(bool state)
        {
            foreach (var b in controlButtons) b.interactable = state;
        }
        private void UpdatePlayPauseState()
        {
            playIcon.SetActive(PlayerController.IsPaused);
            pauseIcon.SetActive(!PlayerController.IsPaused);
        }

        public void Show()
        {
            if (PlayerController.Current != null)
            {
                targetViewParentHeight = -1150;
            }
        }

        public void Hide()
        {
            targetViewParentHeight = -1280;
        }

        private void OnDestroy()
        {
            PlayerController.OnPlayerStateChanged -= OnPlayerStateChanged;
            PlayerController.OnTrackChanged -= OnTrackChanged;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Vector2.Distance(eventData.pressPosition, eventData.position) / Screen.width < 0.03f)
                ScreenManager.Instance.ShowOther(playerView);
        }
    }
}