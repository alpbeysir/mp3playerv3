﻿using YoutubeExplode;

namespace MP3Player.Youtube
{
    public static class FakeYoutube
    {
        private static YoutubeClient _instance;
        public static YoutubeClient Instance
        {
            get
            {
                if (_instance == null) _instance = new YoutubeClient();
                return _instance;
            }
        }
    }
}