using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private NetworkedSprite thumbnail;
    [SerializeField] private TextMeshProUGUI titleDisplay;
    [SerializeField] private TextMeshProUGUI channelDisplay;
    [SerializeField] private Slider seekBar;

    private IAudioPlayer player;

    public void Start()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) player = new WindowsPlayer();
        else player = new AndroidPlayer();

        player.Init();
        player.CurFile = Application.persistentDataPath + "/test.webm";
        player.Play();
    }

    private void Update()
    {
        seekBar.SetValueWithoutNotify(player.CurPos / player.Duration);
        titleDisplay.text = player.CurPos.ToString();
        channelDisplay.text = player.Duration.ToString();
    }
    public void OnPrevious()
    {
        
    }
    public void OnPlayPause()
    {
        
    }
    public void OnNext()
    {
        
    }
    public void OnSeek()
    {
        
    }
}
