using System;
using UnityEngine;

public class AndroidPlayer : AudioPlayer
{
    public class BackgroundAudioInterface : AndroidJavaProxy
    {
        public BackgroundAudioInterface() : base("com.alpbeysir.backgroundaudio.BackgroundAudioInterface") { }

        //Called from native code
        private void Started() => OnStart?.Invoke();
        private void Stopped() => OnStop?.Invoke();
        private void Paused() => OnPause?.Invoke();
        private void Resumed() => OnResume?.Invoke();
        
        //TODO Handle Info and Error
        private void Info(int what, int extra) { Debug.Log("Info called with " + what + extra); }
        private void Error(int what, int extra) { Debug.Log("Error called with " + what + extra); }
        
        private void Prepared() { Debug.Log("Prepared called"); OnPrepared?.Invoke(); }
    }

    public override string CurFile
    {
        get => _curFile;
        set
        {          
            _curFile = value;

            AndroidJNI.AttachCurrentThread();
            CallOnService("start", _curFile, title, desc, iconUri);
            AndroidJNI.DetachCurrentThread();
        }
    }

    public override float CurPos { get => CallOnService<float>("getPosition"); set => CallOnService("setPosition", value); }
    public override float Volume { get => 1.0f; set => _ = value; }
    
    private static string title = "Playing", desc = "Tap to return to player", iconUri;
    public static void SetNotifData(string _title, string _desc, string _iconUri)
    {
        title = _title;
        desc = _desc;
        iconUri = _iconUri;
    }

    public override float Duration => CallOnService<float>("getDuration");
    public override bool IsPaused => CallOnService<bool>("isPaused");
    public override void Pause() => CallOnService("pause");
    public override void Resume() => CallOnService("resume");
    public override void Dispose() => CallOnService("dispose");

    public AndroidPlayer()
    {
        if (Application.platform != RuntimePlatform.Android) return;

        service = new AndroidJavaClass("com.alpbeysir.backgroundaudio.BackgroundAudioService");

        AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        CallOnService("initialize", unityClass.GetStatic<AndroidJavaObject>("currentActivity"), baInterface);
    }

    private static BackgroundAudioInterface baInterface = new BackgroundAudioInterface();
    private static AndroidJavaClass service;
    private static string _curFile;
    private static float _duration;
    private static void CallOnService(string method, params object[] args) => service.CallStatic(method, args);
    private static T CallOnService<T>(string method, params object[] args) => service.CallStatic<T>(method, args);
}
