using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;
using System.Text;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using MP3Player.Models;
using MP3Player.Managers;

namespace MP3Player.Components
{

    public class TrackListViewItem : RecyclingListViewItem, IPointerClickHandler
    {
        [SerializeField] private GameObject loadingParent;
        [SerializeField] private GameObject infoParent;
        [SerializeField] private GameObject statusParent;
        [SerializeField] private ImageView thumbnailDisplay;
        [SerializeField] private TextMeshProUGUI titleDisplay;
        [SerializeField] private TextMeshProUGUI extraTextDisplay;
        [SerializeField] private GameObject downloadedIcon;

        [SerializeField] private LoadingBar loadingBar;

        private Track track;
        private Action<Track, TrackListViewItem> onClick, onButton;

        [SerializeField] private MaterialIcon buttonActionIcon;
        [SerializeField] private GameObject buttonActionParent;

        private const float clickThreshold = 0.03f;

        public void ShowLoading()
        {
            loadingParent.SetActive(true);
            infoParent.SetActive(false);
        }

        public void Populate(Track t)
        {
            track = t;

            loadingParent.SetActive(false);
            infoParent.SetActive(true);
            thumbnailDisplay.SetLoading();
            _ = TextureManager.Texture2DFromUrlAsync(track.LowResThumbnailUrl).ContinueWith(tex => thumbnailDisplay.SetImage(tex));
            titleDisplay.text = RemoveUnsupportedChars(track.Title);
            extraTextDisplay.text = RemoveUnsupportedChars(string.Format("{0} • {1}", track.ChannelName, new TimeSpan(long.Parse(track.Duration)).ToString("mm\\:ss")));

            SetStatusState(DownloadManager.IsDownloading(track.Id));
            DownloadManager.OnDownloadStarted += (id) =>
            {
                if (id == t.Id) SetStatusState(true);
            };
            DownloadManager.OnDownloadComplete += (id) =>
            {
                if (id == t.Id) SetStatusState(false);
            };
            DownloadManager.OnDownloadFailed += (id) =>
            {
                if (id == t.Id) SetStatusState(false);
            };
        }

        public void SetOnClickAction(Action<Track, TrackListViewItem> _onClick)
        {
            onClick = _onClick;
        }

        public void SetButtonAction(string iconUnicode, Action<Track, TrackListViewItem> _onButton)
        {
            buttonActionParent.SetActive(true);
            buttonActionIcon.iconUnicode = iconUnicode;
            onButton = _onButton;
        }

        public void RemoveButtonAction()
        {
            buttonActionParent.SetActive(false);
        }

        private void SetStatusState(bool state)
        {
            if (state)
            {
                loadingBar.Init();
                DownloadManager.OnDownloadProgress += (id, progress) =>
                {
                    if (track.Id == id) loadingBar.SetProgress(progress);
                };
                statusParent.SetActive(false);
            }
            else
            {
                loadingBar.Disable();
                statusParent.SetActive(true);
                downloadedIcon.SetActive(track.AvailableOffline());
            }
        }

        private string RemoveUnsupportedChars(string input)
        {
            StringBuilder sb = new StringBuilder(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                if (titleDisplay.font.HasCharacter(input[i])) sb.Append(input[i]);
                else sb.Append(' ');
            }
            return sb.ToString();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Vector2.Distance(eventData.pressPosition, eventData.position) / Screen.width < clickThreshold)
                onClick?.Invoke(track, this);
        }

        public void OnButtonAction()
        {
            onButton?.Invoke(track, this);
        }
    }
}