using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Mp3Player;

public class NetworkedSprite : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite loadingSprite;
    public async UniTask Set(string url)
    {
        //Set loading state
        if (image.sprite != loadingSprite)
        {
            DestroyImmediate(image.sprite.texture, true);
            DestroyImmediate(image.sprite, true);
            image.sprite = loadingSprite;
 
        }
        else image.sprite = loadingSprite;

        image.sprite = Utils.SpriteFromFile((await Cache.GetOrCreate<Thumbnail>(url)).data);
    }
}
