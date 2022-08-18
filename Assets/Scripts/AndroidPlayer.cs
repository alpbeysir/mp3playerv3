using System;
using System.Threading;
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
        
        private void Prepared() 
        {
            Debug.Log("Prepared called");
            prepared = true;
            OnPrepared?.Invoke();
        }
    }

    public override string CurFile
    {
        get => _curFile;
        set
        {
            _curFile = value;
            prepared = false;

            AndroidJNI.AttachCurrentThread();
            CallOnService("start", _curFile);
            AndroidJNI.DetachCurrentThread();
        }
    }

    public override float CurPos 
    {
        get => prepared ? CallOnService<float>("getPosition") : 0;
        set
        {
            if (prepared)
                CallOnService("setPosition", value);
        }
    }

    public override float Volume { get => 1.0f; set => _ = value; }
    public static void ShowNotification(string _title, string _desc, string _iconUri)
    {
        AndroidJNI.AttachCurrentThread();
        CallOnService("showNotification", _title, _desc, _iconUri);
        AndroidJNI.DetachCurrentThread();
    }

    public override float Duration => prepared ? CallOnService<float>("getDuration") : 0;
    public override bool IsPaused => CallOnService<bool>("isPaused");
    public override bool IsPrepared => prepared;
    public override void Pause()
    {
        if (prepared)
            CallOnService("pause");
    }
    public override void Resume()
    {
        if (prepared)
            CallOnService("resume");
    }
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
    private static bool prepared;

    private static void CallOnService(string method, params object[] args) => service.CallStatic(method, args);
    private static T CallOnService<T>(string method, params object[] args) => service.CallStatic<T>(method, args);
}
