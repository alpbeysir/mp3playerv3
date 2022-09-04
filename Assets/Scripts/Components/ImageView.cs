using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using MP3Player.Managers;

namespace MP3Player.Components
{
    public class ImageView : MonoBehaviour
    {
        [SerializeField] private RawImage image;
        [SerializeField] private Texture2D loadingTex;
        [SerializeField] private AspectRatioFitter aspectRatioFitter;

        private ManagedTexture mt;

        public void SetLoading()
        {
            mt?.Release();
            mt = null;
            image.texture = loadingTex;
        }
        public void SetImage(ManagedTexture _mt)
        {
            mt = _mt;
            //Set loading state
            if (image.texture != loadingTex)
            {
                mt?.Release();
                image.texture = loadingTex;
            }

            var newTex = mt.Get();
            image.texture = newTex;
            aspectRatioFitter.aspectRatio = (float)newTex.width / newTex.height;
        }
    }
}