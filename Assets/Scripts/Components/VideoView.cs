using Cysharp.Threading.Tasks;
using MP3Player.Models;
using MP3Player.Playback;
using System.Threading;
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

        private bool videoSeeking;
        private float videoDelay;

        void Start()
        {
            videoPlayer.prepareCompleted += OnVideoPlayerPrepared;
            videoPlayer.seekCompleted += OnVideoSeekCompleted;
            videoPlayer.errorReceived += OnVideoErrorRecieved;

            if (Application.isMobilePlatform) videoDelay = 2000;
            else videoDelay = 150;
        }
        public async UniTask SetTrack(Track track, CancellationToken token = default)
        {
            videoRenderImage.CrossFadeAlpha(0.1f, 0.1f, true);
            loadingIndicator.SetActive(true);
            videoPlayer.Stop();
            var url = (await track.GetVideoOnlyStreamInfoAsync(token)).Url;
            videoPlayer.url = url;
        }

        public void Play() => videoPlayer.Play();
        public void Pause() => videoPlayer.Pause();

        private async void OnVideoSeekCompleted(VideoPlayer vp)
        {
            await UniTask.Delay((int)videoDelay);
            videoSeeking = false;
        }
        private void OnVideoPlayerPrepared(VideoPlayer vp)
        {
            videoPlayer.targetTexture?.Release();
            videoPlayer.targetTexture = new RenderTexture((int)videoPlayer.width, (int)videoPlayer.height, 32);
            videoRenderImage.texture = videoPlayer.targetTexture;
            aspectRatioFitter.aspectRatio = (float)videoPlayer.width / videoPlayer.height;
            videoPlayer.Play();
            loadingIndicator.SetActive(false);
            videoRenderImage.CrossFadeAlpha(1f, 0.1f, true);
            videoSeeking = false;
        }

        private void OnVideoErrorRecieved(VideoPlayer vp, string message)
        {
            Debug.LogError(message, gameObject);
        }

        void Update()
        {
            var diff = (float)videoPlayer.time - PlayerController.CurPos;
            if (videoPlayer.isPrepared)
            {
                if (Mathf.Abs(diff) > 1f && !videoSeeking)
                {
                    Debug.Log("Syncing video and audio");
                    videoSeeking = true;
                    videoPlayer.time = PlayerController.CurPos;
                    videoPlayer.playbackSpeed = 1;
                }
                //Biraz fark varsa
                else if (Mathf.Abs(diff) > 0.03f)
                {
                    videoPlayer.playbackSpeed = 1.0f - Mathf.Sign(diff) * Misc.Utils.Map(Mathf.Abs(diff), 0f, 1f, 0f, 0.4f);
                }
                else
                {
                    videoPlayer.playbackSpeed = 1;
                }
            }
            //Debug.Log($"{diff} {videoSeeking}");
        }
    }
}