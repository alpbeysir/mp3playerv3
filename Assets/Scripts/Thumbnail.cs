using System.Threading.Tasks;

[System.Serializable]
public class Thumbnail : CacheObject<Thumbnail>
{   
    public byte[] data;

    public Thumbnail(string id) : base(id) { }


    public static async Task<Thumbnail> Creator(string id)
    {
        Thumbnail thumbnail = new Thumbnail(id);
        thumbnail.data = await Utils.DownloadFromURL(id);
        return thumbnail;
    }
}
