using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class ImageView : MonoBehaviour
{
    [SerializeField] private RawImage image;
    [SerializeField] private Texture2D loadingTex;
    [SerializeField] private AspectRatioFitter aspectRatioFitter;
    public async UniTask Set(string url)
    {
        //Set loading state
        if (image.texture != loadingTex)
        {
            DestroyImmediate(image.texture, true);
            image.texture = loadingTex;
        }

        var tex = await TextureUtils.Texture2DFromUrlAsync(url);
        if (tex != null)
        {
            image.texture = tex;
            aspectRatioFitter.aspectRatio = (float)tex.width / tex.height;
        }
        else image.texture = loadingTex;
    }
}
