﻿using LiteDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MP3Player
{
    public class Playlist : DBObject<Playlist>
    {
        public string Name { get; set; }
        public List<string> Data { get; set; }
        public int CurrentIndex { get; set; }
        public int Count => Data.Count;

        public Track this[int index] => Track.Get(Data[index]);

        public void Add(Track track)
        {
            if (Data.Contains(track.Id)) return;
            Data.Add(track.Id);
            Save();
        }

        public bool Contains(Track track) => Data.Contains(track.Id);

        public void Goto(Track track) => CurrentIndex = Data.IndexOf(track.Id);

        [UnityEngine.Scripting.Preserve]
        public List<Track> GetAll()
        {
            var col = DB.Instance.GetCollection<Track>();
            return Data.ConvertAll(s => col.FindById(s));
        }

        public Track GetCurrent()
        {
            if (CurrentIndex >= 0 && CurrentIndex < Data.Count) return Track.Get(Data[CurrentIndex]);
            else return null;
        }
        public void Next()
        {
            CurrentIndex++;
            if (CurrentIndex >= Data.Count) CurrentIndex = Data.Count;
            Save();
        }

        public void Previous()
        {
            CurrentIndex--;
            if (CurrentIndex < 0) CurrentIndex = 0;
            Save();
        }

        public void Remove(Track track)
        {
            if (Data.IndexOf(track.Id) < CurrentIndex) CurrentIndex--;
            Data.Remove(track.Id);
            Save();
        }

        public void ResetPosition()
        {
            CurrentIndex = 0;
            Save();
        }

        public string GetIconUri()
        {
            if (Count > 0)
            {
                return this[0].HighResThumbnailUrl;
            }
            return null;
        }
        public Playlist() : base()
        {
            if (Id == "") Id = ObjectId.NewObjectId().ToString();
            if (Data == null) Data = new();
        }
    }
}