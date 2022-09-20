using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.IO;

namespace MP3Player.Misc
{
    public static class Utils
    {
#if !UNITY_EDITOR
    private static string _persistentDataPath;
    public static string RootPath
    {
        get
        {
            if (_persistentDataPath == null) _persistentDataPath = Application.persistentDataPath + "/";
            return _persistentDataPath;
        }
    }
#else

        public static string RootPath => Application.dataPath + "/../PersistentData/";
#endif

        public static string DataPath => RootPath + "db/";
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

        public static float Map(float x, float in_min, float in_max, float out_min, float out_max)
        {
            x = Mathf.Clamp(x, in_min, in_max);
            float value = (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;

            bool bigger = out_max > out_min;
            return Mathf.Clamp(value, bigger ? out_min : out_max, bigger ? out_max : out_min);
        }

        public static bool ContainsArray(string str, string pred)
        {
            foreach (char c in str) if (pred.Contains(c)) return true;
            return false;
        }
    }
}