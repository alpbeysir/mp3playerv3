using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private NetworkedSprite thumbnail;
    [SerializeField] private TextMeshProUGUI titleDisplay;
    [SerializeField] private TextMeshProUGUI channelDisplay;
    [SerializeField] private Slider seekBar;

    private static AudioPlayer player;

    private static string playing;

    private bool keepPlaying;

    public void Start()
    {
        Application.runInBackground = true;
        
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            player = new WindowsPlayer();
#else
            player = new AndroidPlayer();
#endif

        //if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        //else 
        AudioPlayer.OnStop += OnPlayerStop;

        playing = Application.persistentDataPath + "/test.mp3";
        player.CurFile = playing;
        
    }

    private void Update()
    {
        float asd = (float)player.CurPos / (float)player.Duration;
        titleDisplay.text = asd.ToString();
        seekBar.SetValueWithoutNotify(float.Parse(titleDisplay.text));
    }
    public void OnPrevious()
    {
        keepPlaying = false;
    }
    public void OnPlayPause()
    {
        if (player.IsPaused) player.Resume();
        else player.Pause();
    }
    public void OnNext()
    {
        keepPlaying = true;
    }
    public void OnSeek()
    {
        player.CurPos = seekBar.value * player.Duration;
    }
    public void OnPlayerStop()
    {
        Debug.Log("OnStop called");
        if (keepPlaying)
        {
            player.CurFile = playing;
            Debug.Log("Restarted");
        }
    }

    public void OnDestroy()
    {
        player.Dispose();
    }
}
