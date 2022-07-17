using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using WebP;

public static class Utils
{
    public static Sprite SpriteFromFile(byte[] data)
    { 
        var tex = new Texture2D(8, 8);
        try
        {
            Error error;
            tex = Texture2DExt.CreateTexture2DFromWebP(data, false, false, out error);
        }
        catch
        {
            tex.LoadImage(data);
        }
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        return sprite;
    }

    public static async UniTask<byte[]> DownloadFromURL(string url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            await www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"{www.error}, URL:{www.url}");
                return null;
            }
            else 
                return www.downloadHandler.data;
        }
    }
}
