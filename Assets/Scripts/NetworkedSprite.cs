using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class NetworkedSprite : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite loadingSprite;
    public void ShowLoading(Sprite sprite) => image.sprite = loadingSprite;
    public async UniTask Set(string url)
    {
        //Set loading state
        image.sprite = null;
        
        image.sprite = Utils.SpriteFromFile((await Cache.GetOrCreate<Thumbnail>(url)).data);
    }
}
