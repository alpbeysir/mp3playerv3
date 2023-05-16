using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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

        public static T RandomElement<T>(this IEnumerable<T> enumerable)
        {
            int index = UnityEngine.Random.Range(0, enumerable.Count());
            return enumerable.ElementAt(index);
        }

        public static int EditDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];


            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }

    }
}