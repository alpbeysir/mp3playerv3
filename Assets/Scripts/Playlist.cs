using System;
using System.Collections.Generic;

[Serializable]
public class Playlist
{
    public readonly string Guid = /*Utils.GetUniqueKey()*/"main";
    public string Name { get; set; }
    
    public List<string> data = new();
    public int currentIndex;
    public Dictionary<string, int> isIn = new();

    public int Length => data.Count;

    public void Add(string id)
    {
        data.Add(id);
        isIn.Add(id, data.Count - 1);
        Save(this);
    }

    public bool Contains(string id) => isIn.ContainsKey(id);

    public List<string> GetAll() => data;
    public string GetCurrent() => data[currentIndex];

    public void Goto(string id)
    {
        currentIndex = isIn[id];
        Save(this);
    }
    public void Goto(int index)
    {
        currentIndex = index;
        Save(this);
    }

    public void Next()
    {
        currentIndex++;
        if (currentIndex >= data.Count) currentIndex = 0;
        Save(this);
    }

    public void Previous()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = data.Count - 1;
        Save(this);
    }

    public void Remove(string id)
    {
        if (data.IndexOf(id) < currentIndex) currentIndex--;
        data.Remove(id);
        isIn.Remove(id);
        Save(this);
    }

    public void ResetPosition()
    {
        currentIndex = 0;
        Save(this);
    }

    public void SetPosition(string id)
    {
        currentIndex = data.IndexOf(id);
        Save(this);
    }

    public static bool Load(string guid, out Playlist playlist) => Utils.FileUtil.ReadJson(Utils.PlaylistPath + guid, out playlist);
    public static void Save(Playlist playlist) => Utils.FileUtil.WriteJson(playlist, Utils.PlaylistPath + playlist.Guid);
}