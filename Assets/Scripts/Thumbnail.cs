using System.Threading.Tasks;

namespace Mp3Player
{

    [System.Serializable]
    public class Thumbnail : CacheObject<Thumbnail>
    {
        public byte[] data;

        public Thumbnail(string id) : base(id) { }

        [UnityEngine.Scripting.Preserve]
        public static async Task<Thumbnail> Creator(string id)
        {
            Thumbnail thumbnail = new Thumbnail(id);
            thumbnail.data = await Utils.DownloadFromURL(id);
            return thumbnail;
        }
    }
}