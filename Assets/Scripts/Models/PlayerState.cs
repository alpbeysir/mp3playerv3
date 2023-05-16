using MP3Player.Playback;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MP3Player.Misc;
using System.Diagnostics;
using MP3Player.Youtube;
using System.Threading;
using YoutubeExplode.Search;

namespace MP3Player.Models
{
    public class PlayerState : DBObject<PlayerState>
    {
        public string CurrentTrackId { get; set; }
        public string CurrentPlaylistId { get; set; }
        public List<string> History { get; set; }
        public List<string> PlayQueue { get; set; }

        private const int MAX_HISTORY_LEN = 250;

        public PlayerState() : base()
        {
            Id = "state";
            History = new();
            PlayQueue = new();
        }

        public void SetPlaylist(string playlistId)
        {
            CurrentPlaylistId = playlistId;
            PlayQueue.Clear();
            _ = SaveAsync();
        }

        public async Task Move(TrackChangeDirection dir, CancellationToken token)
        {
            if (dir == TrackChangeDirection.Previous)
            {
                PlayQueue.Insert(0, CurrentTrackId);
                CurrentTrackId = History.Last();
                History.RemoveAt(History.Count - 1);
            }
            else if (dir == TrackChangeDirection.Next)
            {
                if (CurrentTrackId != null)
                {
                    History.Add(CurrentTrackId);
                    if (History.Count > MAX_HISTORY_LEN) History.RemoveRange(0, History.Count - MAX_HISTORY_LEN);
                }

                if (PlayQueue.Count > 0)
                {
                    var trackId = PlayQueue[0];
                    PlayQueue.RemoveAt(0);
                    CurrentTrackId = (await Track.GetAsync(trackId)).Id;
                }
                else
                {
                    var pl = await Playlist.GetAsync(CurrentPlaylistId);
                    var track = pl.GetCurrent();
                    if (track == null)
                    {
                        var playlists = DB.Instance.GetCollection<Playlist>().FindAll();
                        SetPlaylist(playlists.RandomElement().Id);
                        var newPlaylist = Playlist.Get(CurrentPlaylistId);
                        newPlaylist.GotoRandom();
                        CurrentTrackId = newPlaylist.GetCurrent().Id;
                        newPlaylist.Next();
                        //var relatedSearch = new RealYoutube.SearchEnumerator(string.Empty, token, PlayerController.Current.Id); 
                        //VideoSearchResult relatedResult = null;
                        //try
                        //{
                        //    int randomMoveAmount = UnityEngine.Random.Range(1, 20);
                            
                        //    while (randomMoveAmount > 0)
                        //    {
                        //        await relatedSearch.MoveNextAsync();
                        //        relatedResult = relatedSearch.Current as VideoSearchResult;
                        //        randomMoveAmount--;
                        //    }
                        //}
                        //finally
                        //{
                        //    if (relatedResult != null)
                        //    { 
                        //        var relatedTrack = new Track(relatedResult);
                        //        relatedTrack.Save();
                        //        PlayQueue.Insert(0, relatedTrack.Id);
                        //        await Move(TrackChangeDirection.Next, token);
                        //    }
                        //    else
                        //    {
                        //        var tracks = DB.Instance.GetCollection<Playlist>().FindAll();
                        //        SetPlaylist(tracks.RandomElement().Id);
                        //        var newPlaylist = await Playlist.GetAsync(CurrentPlaylistId);
                        //        newPlaylist.GotoRandom();
                        //        CurrentTrackId = pl.GetCurrent().Id;
                        //        newPlaylist.Next();
                        //    }
                        //}            
                    }
                    else
                    {
                        pl.Next();
                        CurrentTrackId = track.Id;
                    }
                }
            }

            _ = SaveAsync();
        }

    }
}