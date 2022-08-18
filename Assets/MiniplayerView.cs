using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class MiniplayerView : Singleton<MiniplayerView>, IPointerClickHandler
{
    [SerializeField] private NetworkedSprite thumbnail;
    [SerializeField] private TextMeshProUGUI titleDisplay;
    [SerializeField] private TextMeshProUGUI channelDisplay;
    [SerializeField] private Button[] controlButtons;
    [SerializeField] private GameObject playIcon, pauseIcon;
    [SerializeField] private PlayerManager playerView;
    void Start()
    {
        PlayerController.OnPlayerStateChanged += OnPlayerStateChanged;
        PlayerController.OnTrackChanged += OnTrackChanged;
    }

    private void OnTrackChanged()
    {
        UpdateTrackDisplay(PlayerController.current);
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
        playIcon.SetActive(PlayerController.IsPaused);
        pauseIcon.SetActive(!PlayerController.IsPaused);
    }

    public void Show()
    {
        if (PlayerController.current != null)
            transform.DOMoveY(10, 0.2f);
    }

    public void Hide()
    {
        transform.DOMoveY(-350, 0.2f);
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
