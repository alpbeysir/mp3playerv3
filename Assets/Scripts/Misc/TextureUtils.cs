using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using WebP;

public static class TextureUtils
{
    public static string ImagePath => Utils.RootPath + "images/";

    public static string TryGetLocalUri(string path)
    {
        string localPath = ImagePath + Utils.ReplaceInvalidChars(path);
        if (File.Exists(localPath)) return localPath;
        else return null;
    }

    //Flow control with exceptions, good practice!
    public static async UniTask<Texture2D> Texture2DFromUrlAsync(string path)
    {
        Utils.CreateDirFromPath(ImagePath);

        //Load locally if possible
        bool isLocal = false;
        if (File.Exists(ImagePath + Utils.ReplaceInvalidChars(path)))
        {
            path = ImagePath + Utils.ReplaceInvalidChars(path);
            isLocal = true;
        }

        Texture2D tex = null;
        try
        {
            if (!isLocal)
            {
                //Debug.Log($"Loading JPG/PNG from network, {path}");
                using UnityWebRequest www = UnityWebRequestTexture.GetTexture(path);
                await www.SendWebRequest();

                //Save to cache for later use
                _ = File.WriteAllBytesAsync(ImagePath + Utils.ReplaceInvalidChars(path), www.downloadHandler.data);

                tex = DownloadHandlerTexture.GetContent(www);
            }
            else
            {
                //Debug.Log($"Loading JPG/PNG locally, {path}");
                byte[] data = await File.ReadAllBytesAsync(path);
                tex = new Texture2D(1, 1);
                if (!ImageConversion.LoadImage(tex, data)) throw new Exception("ImageConversion failed!");
            }
        }
        catch (Exception e)
        {
            //If UnityWebRequestTexture fails, try to use WebP handler instead
            try
            {
                byte[] data;
                if (!isLocal)
                {
                    //Debug.Log($"Loading WebP from network, {path}");
                    using UnityWebRequest www = UnityWebRequest.Get(path);
                    await www.SendWebRequest();

                    data = www.downloadHandler.data;

                    _ = File.WriteAllBytesAsync(ImagePath + Utils.ReplaceInvalidChars(path), data);
                }
                else
                {
                    //Debug.Log($"Loading WebP locally, {path}");
                    data = await File.ReadAllBytesAsync(path);
                }

                tex = Texture2DExt.CreateTexture2DFromWebP(data, false, false, out Error error);
            }
            catch (Exception e2)
            {
                if (isLocal)
                    File.Delete(path);

                Debug.LogException(e);
                Debug.LogException(e2);
            }
        }
 
        return tex;
    }
}
