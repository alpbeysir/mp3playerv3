using MP3Player.Managers;
using MP3Player.Misc;
using Cysharp.Threading.Tasks;
using LeTai.Asset.TranslucentImage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace MP3Player.Components
{
    public class TranslucentBackground : Singleton<TranslucentBackground>
    {
        [SerializeField] private MeshRenderer bgRenderer;
        [SerializeField] private Texture2D loadingTex;
        [SerializeField] private TranslucentImage translucentImage;

        private ManagedTexture mt;

        public void SetLoading()
        {
            mt?.Release();
            bgRenderer.material.SetTexture("_MainTex", loadingTex);
        }
        public void SetImage(ManagedTexture tex)
        {
            mt?.Release();
            mt = tex;
            bgRenderer.material.SetTexture("_MainTex", mt.Get());
        }
    }
}