using System.Threading;
using System.Threading.Tasks;

public class MediaInfo
{
    public Track metadata;
    public string mediaUri;
    
    public static async Task<MediaInfo> Creator(Track meta, CancellationToken token = default)
    {
        MediaInfo mediaInfo = new MediaInfo();
        mediaInfo.metadata = meta;
        mediaInfo.mediaUri = await meta.GetMediaUri(token);
        return mediaInfo;
    } 
}
