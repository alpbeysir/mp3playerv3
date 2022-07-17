using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class FileUtil
{
    public static bool ReadFile<T>(string path, out T result)
    {
        BinaryFormatter bf = new BinaryFormatter();

        try
        {
            using FileStream fs = File.Open(path, FileMode.Open);
            result = (T)bf.Deserialize(fs);
            //Debug.Log("Read file: " + Path.GetFileName(path));
            return true;
        }
        catch
        {
            //Debug.Log("Couldn't read file: " + Path.GetFileName(path));
            result = default;
            return false;
        }
    }
   
    public static void WriteFile<T>(T obj, string path)
    {
        BinaryFormatter bf = new BinaryFormatter();

        if (!Directory.Exists(Path.GetDirectoryName(path))) Directory.CreateDirectory(Path.GetDirectoryName(path));

        using FileStream fs = File.Create(path);
        bf.Serialize(fs, obj);
        //Debug.Log("Wrote file: " + Path.GetFileName(path));
    }
}
