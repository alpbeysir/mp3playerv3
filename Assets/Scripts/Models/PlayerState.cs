using MP3Player.Playback;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

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

        public async Task Move(TrackChangeDirection dir)
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
                        //TODO handle end of playlist (switch to recommendations) for now just restart playlist
                        Debug.Log("Reached playlist end");
                        pl.ResetPosition();
                        CurrentTrackId = pl.GetCurrent().Id;
                    }
                    pl.Next();
                    CurrentTrackId = track.Id;
                }
            }

            _ = SaveAsync();
        }

    }
}