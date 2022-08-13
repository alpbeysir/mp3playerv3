using System;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using UnityEngine;
using System.Threading;

[Serializable]
public abstract class CacheObject<T>
{
    public readonly string id;

    public CacheObject(string id) => this.id = id;

    public CacheObject() { }

    public abstract Task<T> Creator(string id, CancellationToken token = default);
}

public static class Cache
{
    public static async Task<T> GetOrCreate<T>(string id, CancellationToken token = default) where T : CacheObject<T>, new()
    {
        string path = GetPath<T>(id);
        T ret = new T();
        if (!Utils.FileUtil.ReadJson(path, out ret))
        {
            var n = await ret.Creator(id, token);
            ret = n;
            //MethodInfo builderMethod = typeof(T).GetMethod("Creator", BindingFlags.Static | BindingFlags.Public);
            //var request = (Task<T>)builderMethod.Invoke(null, new object[] { id });
            //ret = await request;
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