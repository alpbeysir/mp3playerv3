using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mp3Player
{
    [System.Serializable]
    public class Thumbnail : CacheObject<Thumbnail>
    {
        public byte[] Data { get; set; }

        public Thumbnail() { }
 
        public Thumbnail(string id) : base(id) { }

        public override async Task<Thumbnail> Creator(string id, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Thumbnail thumbnail = new Thumbnail(id);
            thumbnail.id = id;
            thumbnail.Data = await Utils.DownloadFromURL(id);
            return thumbnail;
        }
    }
}