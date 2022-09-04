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
    [Serializable]
    public struct SwipeActionData
    {
        public string name;
        public string iconUnicode;
        public Color color;
        public Action<Track, TrackListViewItem> onActivate;
    }

    public class TrackListViewItem : RecyclingListViewItem, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
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
        private Action<Track, TrackListViewItem> onClick, onLeftSwipe, onRightSwipe, onButton;

        private static TrackListViewItem swiped;

        [SerializeField] private Image leftSwipeBg, rightSwipeBg;
        [SerializeField] private TextMeshProUGUI leftSwipeText, rightSwipeText;
        [SerializeField] private MaterialIcon leftSwipeIcon, rightSwipeIcon, buttonActionIcon;
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

        public void SetLeftSwipeAction(SwipeActionData left)
        {
            leftSwipeText.text = left.name;
            leftSwipeIcon.iconUnicode = left.iconUnicode;
            leftSwipeBg.color = left.color;
            onLeftSwipe = left.onActivate;
        }

        public void RemoveLeftSwipeAction() => onLeftSwipe = null;
        public void RemoveRightSwipeAction() => onRightSwipe = null;

        public void SetRightSwipeAction(SwipeActionData right)
        {
            rightSwipeText.text = right.name;
            rightSwipeIcon.iconUnicode = right.iconUnicode;
            rightSwipeBg.color = right.color;
            onRightSwipe = right.onActivate;
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

        private void LeftSwipe()
        {
            onLeftSwipe?.Invoke(track, this);
            EndSwipe(false);
        }

        private void RightSwipe()
        {
            onRightSwipe?.Invoke(track, this);
            EndSwipe(false);
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

        #region Swipe Action

        private Vector2 initialPos;
        private Vector2 initialMousePos;

        public void OnBeginDrag(PointerEventData eventData)
        {
            ParentList.GetComponent<ScrollRect>().OnBeginDrag(eventData);
            if (swiped != null) return;
            swiped = this;
            initialMousePos = Input.mousePosition;
            initialPos = transform.localPosition;
        }

        private bool passedThreshold;
        public void OnDrag(PointerEventData eventData)
        {
            if (Mathf.Abs(initialMousePos.x - Input.mousePosition.x) / Screen.width > 0.05f) passedThreshold = true;
            if (Mathf.Abs(initialMousePos.y - Input.mousePosition.y) / Screen.width > 0.1f && swiped == this)
            {
                transform.DOLocalMove(initialPos, 0.1f);
                swiped = null;
                passedThreshold = false;
            }
            if (swiped == null || !passedThreshold)
            {
                ParentList.GetComponent<ScrollRect>().OnDrag(eventData);
                return;
            }
            if (infoParent.activeSelf)
            {
                var delta = eventData.delta.x;
                transform.localPosition = Vector2.Lerp(transform.localPosition, new Vector2(transform.localPosition.x + delta * 4000 / Screen.width, transform.localPosition.y), 0.20f);
                if (onLeftSwipe == null)
                {
                    var x = transform.localPosition.x;
                    x = Mathf.Clamp(x, float.MinValue, initialPos.x);
                    transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
                }
                if (onRightSwipe == null)
                {
                    var x = transform.localPosition.x;
                    x = Mathf.Clamp(x, initialPos.x, float.MaxValue);
                    transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
                }
            }
        }

        private void EndSwipe(bool triggerAction = true)
        {
            var delta = (Vector2)transform.localPosition - initialPos;
            if (Mathf.Abs(delta.x) > 200 && triggerAction)
            {
                if (delta.x > 0)
                    LeftSwipe();
                else
                    RightSwipe();
            }
            transform.DOLocalMove(initialPos, 0.1f);
            passedThreshold = false;
            swiped = null;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            ParentList.GetComponent<ScrollRect>().OnEndDrag(eventData);
            if (swiped == this) EndSwipe();
        }

        #endregion Swipe Action
    }
}