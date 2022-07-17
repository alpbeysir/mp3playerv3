using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
    
[Serializable]
public abstract class CacheObject<T>
{
    public readonly string id;

    public CacheObject(string id) => this.id = id;
}

public static class Cache
{
    private static string cachePath => Application.persistentDataPath + "/cache/";

    public static bool TryGet<T>(string id, out T result) where T : CacheObject<T>
    {
        return FileUtil.ReadFile(GetPath<T>(id), out result);
    }
    public static void Clear()
    {
        if (Directory.Exists(cachePath))
        {
            Directory.Delete(cachePath, true);
        }
    }
    public static async Task<T> GetOrCreate<T>(string id) where T : CacheObject<T>
    {
        string path = GetPath<T>(id);
        T ret;
        if (!FileUtil.ReadFile(path, out ret))
        {
            MethodInfo builderMethod = typeof(T).GetMethod("Creator", BindingFlags.Static | BindingFlags.Public);
            var request = (Task<T>)builderMethod.Invoke(null, new object[] {id});
            ret = await request;
            FileUtil.WriteFile(ret, path);
        }
        return ret;
    }

    public static void Save<T>(T ret) where T : CacheObject<T>
    {
        string path = GetPath<T>(ret.id);
        FileUtil.WriteFile(ret, path);
    }
    
    public static string ReplaceInvalidChars(string filename) => string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
    private static string GetPath<T>(string id) => string.Format("{0}.{1}", typeof(T).FullName, ReplaceInvalidChars(id));
}