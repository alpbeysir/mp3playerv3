﻿using YoutubeExplode;

public static class Youtube
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