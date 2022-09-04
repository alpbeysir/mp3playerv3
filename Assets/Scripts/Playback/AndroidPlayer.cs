using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MP3Player.Playback
{
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

        public override void SetDataSource(string uri)
        {
            prepared = false;
            _curPos = 0;
            AndroidJNI.AttachCurrentThread();
            CallOnService("showNotification", title, desc, iconUri);
            CallOnService("start", uri);
            AndroidJNI.DetachCurrentThread();
        }

        private float _curPos;

        private async Task UpdatePosition(CancellationToken token = default)
        {
            while (true)
            {
                _curPos = prepared ? CallOnService<float>("getPosition") : 0;
                if (token.IsCancellationRequested) return;
                await Task.Delay(500);
            }
        }
        public override float CurPos
        {
            get => _curPos;
            set
            {
                if (prepared)
                    CallOnService("setPosition", value);
            }
        }

        public override float Volume { get => 1.0f; set => _ = value; }
        public static void SetNotificationData(string _title, string _desc, string _iconUri)
        {
            title = _title;
            desc = _desc;
            iconUri = _iconUri;
        }
        private static string title = "Playing", desc = "Tap to return to player", iconUri = "";

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
        public override void Dispose()
        {
            updatePosCts.Cancel();
            CallOnService("dispose");
        }

        private CancellationTokenSource updatePosCts;

        public AndroidPlayer()
        {
            if (Application.platform != RuntimePlatform.Android) return;

            service = new AndroidJavaClass("com.alpbeysir.backgroundaudio.BackgroundAudioService");

            AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            CallOnService("initialize", unityClass.GetStatic<AndroidJavaObject>("currentActivity"), baInterface);

            updatePosCts = new();
            _ = UpdatePosition(updatePosCts.Token);
        }

        private static BackgroundAudioInterface baInterface = new BackgroundAudioInterface();
        private static AndroidJavaClass service;
        private static bool prepared;

        private static void CallOnService(string method, params object[] args) => service.CallStatic(method, args);
        private static T CallOnService<T>(string method, params object[] args) => service.CallStatic<T>(method, args);
    }
}