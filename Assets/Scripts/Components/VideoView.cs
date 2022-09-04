using Cysharp.Threading.Tasks;
using MP3Player.Models;
using MP3Player.Playback;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace MP3Player.Components
{
    public class VideoView : MonoBehaviour
    {
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private AspectRatioFitter aspectRatioFitter;
        [SerializeField] private RawImage videoRenderImage;
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private Texture2D loadingTex;

        [SerializeField] private TextMeshProUGUI debugText;

        private bool videoSeeking;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        readonly int videoDelayMs = 150;
        readonly float seekThreshold = 0.500f;
        readonly float distortThreshold = 0.015f;
        readonly float maxDistortion = 0.25f;
#else
        readonly int videoDelayMs = 750;
        readonly float seekThreshold = 0.750f;
        readonly float distortThreshold = 0.015f;
        readonly float maxDistortion = 0.4f;
#endif

        private bool lastPrepared;

        void Start()
        {
            //videoPlayer.prepareCompleted += OnVideoPlayerPrepared;
            videoPlayer.seekCompleted += OnVideoSeekCompleted;
            videoPlayer.errorReceived += OnVideoErrorRecieved;
        }
        public async UniTask SetTrack(Track track, CancellationToken token = default)
        {
            videoRenderImage.CrossFadeAlpha(0.1f, 0.1f, true);
            loadingIndicator.SetActive(true);
            videoPlayer.Stop();
            await UniTask.SwitchToThreadPool();
            var url = (await track.GetVideoOnlyStreamInfoAsync(token)).Url;
            await UniTask.SwitchToMainThread();
            videoPlayer.url = url;
            videoPlayer.Prepare();
        }

        public void Play() => videoPlayer.Play();
        public void Pause() => videoPlayer.Pause();

        private async void OnVideoSeekCompleted(VideoPlayer vp)
        {
            await UniTask.Delay(videoDelayMs);
            videoSeeking = false;
        }
        private void OnVideoPlayerPrepared(VideoPlayer vp)
        {
            videoPlayer.targetTexture?.Release();
            var tex = new RenderTexture((int)videoPlayer.width, (int)videoPlayer.height, 32);
            tex.depth = 0;
            tex.filterMode = FilterMode.Bilinear;
            videoPlayer.targetTexture = tex;
            videoRenderImage.texture = videoPlayer.targetTexture;

            aspectRatioFitter.aspectRatio = (float)videoPlayer.width / videoPlayer.height;
            videoPlayer.Play();
            loadingIndicator.SetActive(false);
            videoRenderImage.CrossFadeAlpha(1f, 0.1f, true);
            videoSeeking = false;
        }

        private void OnVideoErrorRecieved(VideoPlayer vp, string message)
        {
            videoRenderImage.texture = loadingTex;
            loadingIndicator.SetActive(true);
        }

        void Update()
        {
            var diff = (float)videoPlayer.time - PlayerController.CurPos;
            if (videoPlayer.isPrepared)
            {
                if (Mathf.Abs(diff) > seekThreshold && !videoSeeking)
                {
                    Debug.Log("Syncing video and audio");
                    videoSeeking = true;
                    videoPlayer.time = PlayerController.CurPos;
                    videoPlayer.playbackSpeed = 1;
                }
                //Biraz fark varsa
                else if (Mathf.Abs(diff) > distortThreshold)
                {
                    videoPlayer.playbackSpeed = 1.0f - Mathf.Sign(diff) * Misc.Utils.Map(Mathf.Abs(diff), distortThreshold, seekThreshold, 0f, maxDistortion);
                }
                else
                {
                    videoPlayer.playbackSpeed = 1.0f;
                }

                if (!lastPrepared) OnVideoPlayerPrepared(videoPlayer);
            }
            lastPrepared = videoPlayer.isPrepared;
            //debugText.text = ($"A/V Delay: {(int)(diff * 1000)} ms\nSeeking: {videoSeeking}\nPrepared: {videoPlayer.isPrepared}");
        }
    }
}