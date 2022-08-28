using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

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
        PlayerManager.OnPlayerStateChanged += OnPlayerStateChanged;
        PlayerManager.OnTrackChanged += OnTrackChanged;
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

        float ratio = PlayerManager.CurPos / PlayerManager.Duration;
        if (0 <= ratio && ratio <= 1)
            fillBg.fillAmount = ratio;
        else
            fillBg.fillAmount = 0;
    }
    private void OnTrackChanged()
    {
        UpdateTrackDisplay(PlayerManager.Current);
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
        _ = thumbnail.Set(track.HighResThumbnailUrl);
        titleDisplay.text = track.Title;
        channelDisplay.text = track.ChannelName;
    }

    private void SetPlayerControlInteractivity(bool state)
    {
        foreach (var b in controlButtons) b.interactable = state;
    }
    private void UpdatePlayPauseState()
    {
        playIcon.SetActive(PlayerManager.IsPaused);
        pauseIcon.SetActive(!PlayerManager.IsPaused);
    }

    public void Show()
    {
        if (PlayerManager.Current != null)
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
        PlayerManager.OnPlayerStateChanged -= OnPlayerStateChanged;
        PlayerManager.OnTrackChanged -= OnTrackChanged;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Vector2.Distance(eventData.pressPosition, eventData.position) / Screen.width < 0.03f)
            ScreenManager.Instance.ShowOther(playerView);
    }
}
