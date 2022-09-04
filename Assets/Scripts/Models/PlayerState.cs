using System.Collections.Generic;

namespace MP3Player.Models
{
    public class PlayerState : DBObject<PlayerState>
    {
        public string Current { get; set; }
        public string Playlist { get; set; }
        public List<string> History { get; set; }
        public List<string> PlayQueue { get; set; }

        public PlayerState() : base()
        {
            Id = "state";
            History = new();
            PlayQueue = new();
        }
    }
}