using System;
using System.Collections.Generic;
using YoutubeExplode;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class MetadataCache
{
    private static string metadataPath => Application.persistentDataPath + "/metadata/";
    public static async UniTask<Metadata> GetByID(string id)
    {
        string path = metadataPath + id;
        Metadata ret;
        if (!FileUtil.ReadFile(path, out ret))
        {
            ret = await Metadata.FromVideo(await Youtube.Instance.Videos.GetAsync(id));
            FileUtil.WriteFile(ret, path);
        }
        return ret;
    }
}