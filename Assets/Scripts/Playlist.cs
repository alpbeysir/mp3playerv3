using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class Playlist
{
    public readonly string Guid = Utils.GetUniqueKey();
    public string Name { get; set; }

    private List<string> data = new();
    private int currentIndex;
    private Dictionary<string, int> isIn = new();

    public int Length => data.Count;

    public void Add(string id)
    {
        data.Add(id);
        isIn.Add(id, data.Count - 1);
    }

    public bool Contains(string id) => isIn.ContainsKey(id);

    public List<string> GetAll() => data;
    public string GetCurrent() => data[currentIndex];

    public void Goto(string id)
    {
        currentIndex = isIn[id];
    }
    public void Goto(int index)
    {
        currentIndex = index;
    }

    public void Next()
    {
        currentIndex++;
        if (currentIndex >= data.Count) currentIndex = 0;
    }

    public void Previous()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = data.Count - 1;
    }

    public void Remove(string id)
    {
        if (data.IndexOf(id) < currentIndex) currentIndex--;
        data.Remove(id);
        isIn.Remove(id);
    }

    public void ResetPosition()
    {
        currentIndex = 0;
    }

    public void SetPosition(string id)
    {
        currentIndex = data.IndexOf(id);
    }

    public static bool Load(string guid, out Playlist playlist) => Utils.FileUtil.Read(Utils.PlaylistPath + guid, out playlist);
    public static void Save(Playlist playlist) => Utils.FileUtil.Write(playlist, Utils.PlaylistPath + playlist.Guid);
}