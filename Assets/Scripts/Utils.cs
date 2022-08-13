using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using WebP;
using YoutubeExplode.Videos.Streams;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

public static class Utils
{
    public static class FileUtil
    {
        public static bool ReadBinary<T>(string path, out T result)
        {
            BinaryFormatter bf = new BinaryFormatter();

            try
            {
                using FileStream fs = System.IO.File.Open(path, FileMode.Open);
                result = (T)bf.Deserialize(fs);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public static void WriteBinary<T>(T obj, string path)
        {
            BinaryFormatter bf = new BinaryFormatter();

            CreateDirFromPath(path);

            using FileStream fs = System.IO.File.Create(path);
            bf.Serialize(fs, obj);
            //Debug.Log("Wrote file: " + Path.GetFileName(path));
        }

        static JsonSerializerOptions options = new JsonSerializerOptions { IncludeFields = true, WriteIndented = true };

        public static bool ReadJson<T>(string path, out T result)
        {
            try
            {
                result = JsonSerializer.Deserialize<T>(File.ReadAllText(path), options);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public static void WriteJson<T>(T obj, string path)
        {
            CreateDirFromPath(path);

            string jsonStr = JsonSerializer.Serialize(obj, options);
            File.WriteAllText(path, jsonStr);
        }
    }
    
    private static string _persistentDataPath;
    public static string RootPath
    {
        get
        {
            if (_persistentDataPath == null) _persistentDataPath = Application.persistentDataPath + "/";
            return _persistentDataPath;
        }
    }

    public static string DataPath => RootPath + "db/";
    public static string CachePath => RootPath + "cache/";

    public static string PlaylistPath => DataPath + "playlists/";
    public static string MediaPath => DataPath + "media/";


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
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, Vector4.one, false);
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
  
    private static bool TryGetExistingMedia(string id, out string path)
    {
        if (DownloadManager.IsDownloading(id))
        {
            Debug.Log("Downloading");
            path = "";
            return false;
        }
        
        Container[] possibleContainers = { Container.WebM, Container.Mp3, Container.Mp4, Container.Tgpp };
        string pathWithoutExt = MediaPath + id;
        foreach (var c in possibleContainers)
        {
            path = string.Format("{0}.{1}", pathWithoutExt, c.Name);
            if (File.Exists(path)) return true;
        }
        path = "";
        return false;
    }

    public static async Task<string> GetMediaUri(string id, CancellationToken token)
    {
        string path;
        if (TryGetExistingMedia(id, out path))
        {
            Debug.Log(string.Format("Found existing media: {0}", path));
            return path;
        }
        else
        {
            StreamManifest streamManifest = await Youtube.Instance.Videos.Streams.GetManifestAsync(id, token);
            var streamInfo = streamManifest.GetAudioOnlyStreams().OrderBy(s => s.Bitrate).TryGetWithHighestBitrate();
            _ = DownloadManager.DownloadAsync(id, (AudioOnlyStreamInfo)streamInfo);
            return streamInfo.Url;
        }
    }

    public static void CreateDirFromPath(string path)
    {
        var dirName = Path.GetDirectoryName(path);
        if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
    }
    public static string GetUniqueKey(int size = 24, string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
    {
        using (var crypto = new RNGCryptoServiceProvider())
        {
            var data = new byte[size];
            byte[] smallBuffer = null;
            int maxRandom = byte.MaxValue - ((byte.MaxValue + 1) % chars.Length);

            crypto.GetBytes(data);

            var result = new char[size];

            for (int i = 0; i < size; i++)
            {
                byte v = data[i];

                while (v > maxRandom)
                {
                    if (smallBuffer == null)
                    {
                        smallBuffer = new byte[1];
                    }

                    crypto.GetBytes(smallBuffer);
                    v = smallBuffer[0];
                }

                result[i] = chars[v % chars.Length];
            }

            return new string(result);
        }
    }
}
