using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using WebP;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text.Json;

public static class Utils
{
    public static class FileUtil
    {
        public static bool ReadBinary<T>(string path, out T result)
        {
            BinaryFormatter bf = new BinaryFormatter();

            try
            {
                using FileStream fs = File.Open(path, FileMode.Open);
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

            using FileStream fs = File.Create(path);
            bf.Serialize(fs, obj);
        }

        static JsonSerializerOptions options = new JsonSerializerOptions
        { 
            IncludeFields = true,
            #if UNITY_EDITOR
            WriteIndented = true
            #endif 
        };

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

    public static string ReplaceInvalidChars(string filename)
    {
        return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
    }

    public static void CreateDirFromPath(string path)
    {
        var dirName = Path.GetDirectoryName(path);
        if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
    }

    public static void ClampPosition(this Transform tr, float minX = float.MinValue, float maxX = float.MaxValue,
                                                        float minY = float.MinValue, float maxY = float.MaxValue,
                                                        float minZ = float.MinValue, float maxZ = float.MaxValue)
    {
        tr.position = new Vector3(Mathf.Clamp(tr.position.x, minX, maxX), Mathf.Clamp(tr.position.y, minY, maxY), Mathf.Clamp(tr.position.z, minZ, maxZ));
    }

    public static float Map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        x = Mathf.Clamp(x, in_min, in_max);
        float value = (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;

        bool bigger = out_max > out_min;
        return Mathf.Clamp(value, bigger ? out_min : out_max, bigger ? out_max : out_min);
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
