using MP3Player.Misc;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using WebP;

namespace MP3Player.Managers
{
    public class ManagedTexture : IDisposable
    {
        private readonly Texture2D texture;
        private int refCount;
        private bool isValid;
        private string id;

        public void Dispose()
        {
            if (refCount <= 0 && isValid)
            {
                //Debug.Log($"Freed managed texture {id}");
                isValid = false;
                TextureManager.memCache.Remove(id);
                UnityEngine.Object.DestroyImmediate(texture, true);
            }
        }

        public Texture2D Get()
        {
            if (!isValid) { Debug.LogError("This texture has been freed!"); return null; }
            refCount++;
            return texture;
        }

        public void Release()
        {
            refCount--;
            //Wait some time and dispose [disabled as UniTask.Delay seems to eat resources]
            //if (refCount <= 0) UniTask.Post(async () =>
            //{
            //    await UniTask.Delay(10000);
            //    Dispose();
            //});
        }

        public ManagedTexture(Texture2D value, string _id)
        {
            //Debug.Log($"Created managed texture {_id}");
            id = _id;
            texture = value;
            isValid = true;
        }
    }

    public static class TextureManager
    {
        public static string ImagePath => Utils.RootPath + "images/";

        public static string TryGetLocalUri(string path)
        {
            string localPath = ImagePath + Utils.ReplaceInvalidChars(path);
            if (File.Exists(localPath)) return localPath;
            else return null;
        }

        private static readonly Dictionary<string, Task<ManagedTexture>> inProgressTasks = new();

        public static readonly Dictionary<string, ManagedTexture> memCache = new();

        public static void TextureGCCollect()
        {
            ManagedTexture[] copy = new ManagedTexture[memCache.Values.Count];
            memCache.Values.CopyTo(copy, 0);
            foreach (var mt in copy)
            {
                mt.Dispose();
            }
        }

        public static async UniTask<ManagedTexture> Texture2DFromUrlAsync(string path)
        {
            if (memCache.ContainsKey(path))
            {
                //Debug.Log($"Already in cache, texture {path}");
                return memCache[path];
            }

            Task<ManagedTexture> task;
            if (inProgressTasks.ContainsKey(path))
            {
                //Debug.Log($"Already getting texture {path}, returning original task");
                task = inProgressTasks[path];
            }
            else
            {
                //Debug.Log($"Creating texture get task {path}");
                task = Texture2DFromUrlAsyncInternal(path);
                inProgressTasks.Add(path, task);
            }

            var res = await task;
            memCache[path] = res;
            inProgressTasks.Remove(path);
            return res;
        }

        //Flow control with exceptions, good practice!
        private static async Task<ManagedTexture> Texture2DFromUrlAsyncInternal(string path)
        {
            string id = path;

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
                    await UniTask.SwitchToThreadPool();
                    byte[] data = await File.ReadAllBytesAsync(path);
                    await UniTask.SwitchToMainThread();

                    tex = new Texture2D(1, 1);
                    if (!tex.LoadImage(data)) throw new Exception("ImageConversion failed!");
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

            tex.name = id;
            var mt = new ManagedTexture(tex, id);
            return mt;
        }
    }
}