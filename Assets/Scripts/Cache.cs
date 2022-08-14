using System;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using UnityEngine;

[Serializable]
public abstract class CacheObject<T>
{
    public string id;

    public CacheObject(string id) => this.id = id;

    public CacheObject() { }

    public abstract Task<T> Creator(string id, CancellationToken token = default);
}

public static class Cache
{
    public static async Task<T> GetOrCreate<T>(string id, CancellationToken token = default) where T : CacheObject<T>, new()
    {
        string path = GetPath<T>(id);
        T ret;
        if (!Utils.FileUtil.ReadJson(path, out ret))
        {
            ret = new T();
            ret = await ret.Creator(id, token);
            Utils.FileUtil.WriteJson(ret, path);
        }
        return ret;
    }

    public static void Save<T>(T ret) where T : CacheObject<T>
    {
        string path = GetPath<T>(ret.id);
        Utils.FileUtil.WriteJson(ret, path);
    }

    private static string GetPath<T>(string id) where T : CacheObject<T>
    {
        return Utils.CachePath + ReplaceInvalidChars(string.Format("{0}.{1}", typeof(T).Name, id));
    }

    public static string ReplaceInvalidChars(string filename)
    {
        return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
    }
}