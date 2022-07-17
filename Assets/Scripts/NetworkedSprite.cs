using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

[System.Serializable]
public class Thumbnail : CacheObject<Thumbnail>
{   
    public byte[] data;

    public Thumbnail(string id) : base(id) { }


    public static async Task<Thumbnail> Creator(string id)
    {
        Thumbnail thumbnail = new Thumbnail(id);
        thumbnail.data = await Utils.DownloadFromURL(id);
        return thumbnail;
    }
}


public class NetworkedSprite : MonoBehaviour
{
    [SerializeField] private Image image;
    public async UniTask Set(string url)
    {
        //Set loading state
        image.sprite = null;
        
        image.sprite = Utils.SpriteFromFile((await Cache.Get<Thumbnail>(url)).data);
    }
}
