using System.Threading;
using System.Threading.Tasks;

public class MediaInfo
{
    public Metadata metadata;
    public string mediaUri;
    
    public static async Task<MediaInfo> Creator(Metadata meta, CancellationToken token)
    {
        MediaInfo mediaInfo = new MediaInfo();
        mediaInfo.metadata = meta;
        mediaInfo.mediaUri = await Utils.GetMediaUri(mediaInfo.metadata.id, token);
        return mediaInfo;
    } 
}
