using System.Collections.Generic;
#nullable enable
namespace YoutubeExplode.Bridge
{
    internal interface IPlaylistExtractor
    {
        string? TryGetPlaylistTitle();

        string? TryGetPlaylistAuthor();

        string? TryGetPlaylistChannelId();

        string? TryGetPlaylistDescription();

        IReadOnlyList<ThumbnailExtractor> GetPlaylistThumbnails();
    }
}