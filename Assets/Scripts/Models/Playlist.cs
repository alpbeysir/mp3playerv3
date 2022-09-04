using MP3Player.Models;
using LiteDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using MP3Player.Youtube;

namespace MP3Player.Models
{
    public enum PlaylistSource
    {
        Local, Youtube, Spotify
    }
    public class Playlist : DBObject<Playlist>
    {
        public string Name { get; set; }
        public List<string> Data { get; set; }
        public int CurrentIndex { get; set; }
        public PlaylistSource Source { get; set; }

        //Only populated if this is a Youtube Playlist, used to check for changes
        public string RootETag { get; set; }
        public string ContentDetailsETag { get; set; }

        public int Count => Data.Count;

        public Track this[int index] => Track.Get(Data[index]);

        public void Add(Track track)
        {
            if (Data.Contains(track.Id)) return;
            Data.Add(track.Id);
            Save();
        }

        public void Add(IEnumerable<Track> list)
        {
            var validIds = list.Select(t => t.Id).Where(id => !Data.Contains(id));
            Data.AddRange(validIds);
            Save();
        }

        public bool Contains(Track track) => Data.Contains(track.Id);

        public void Goto(Track track) => CurrentIndex = Data.IndexOf(track.Id);

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
            //TODO add collage of 4 images
            return null;
        }
        public Playlist() : base()
        {
            if (Id == null || Id == "") Id = ObjectId.NewObjectId().ToString();
            if (Data == null) Data = new();
        }

        public static async Task<Playlist> FromYoutubePlaylist(Google.Apis.YouTube.v3.Data.Playlist youtubePlaylist)
        {
            var service = await RealYoutube.GetUserServiceAsync();

            var localCandidate = Get(youtubePlaylist.Id);
            if (localCandidate != null)
            {
                //Check ETag
                if (localCandidate.RootETag == youtubePlaylist.ETag && localCandidate.ContentDetailsETag == youtubePlaylist.ContentDetails.ETag)
                {
                    Debug.Log($"YouTube playlist {youtubePlaylist.Snippet.Title} hasn't changed, returning local");
                    return localCandidate;
                }
                else
                {
                    Debug.Log($"YouTube playlist {youtubePlaylist.Snippet.Title} changed, syncing");
                }
            }

            try
            {
                if (service == null) return null;

                Playlist localPlaylist = Get(youtubePlaylist.Id);
                localPlaylist.Name = youtubePlaylist.Snippet.Title;
                localPlaylist.Id = youtubePlaylist.Id;
                localPlaylist.RootETag = youtubePlaylist.ETag;
                localPlaylist.Source = PlaylistSource.Youtube;
                localPlaylist.Data.Clear();

                List<Track> tracks = new();

                string nextToken = "";
                while (nextToken != null)
                {
                    var itemsReq = service.PlaylistItems.List("snippet, contentDetails");
                    itemsReq.PlaylistId = youtubePlaylist.Id;
                    itemsReq.PageToken = nextToken;
                    itemsReq.MaxResults = 50;
                    var itemsResp = await itemsReq.ExecuteAsync();

                    var videosReq = service.Videos.List("snippet, contentDetails");
                    videosReq.Id = itemsResp.Items.Select((p) => p.ContentDetails.VideoId).ToList();
                    videosReq.MaxResults = 50;
                    var videosResp = await videosReq.ExecuteAsync();

                    foreach (var item in videosResp.Items)
                    {
                        Track track = new(item);
                        tracks.Add(track);
                        track.Save();
                        localPlaylist.Add(track);
                    }

                    nextToken = itemsResp.NextPageToken;
                }

                localPlaylist.Add(tracks);
                localPlaylist.Save();
                Debug.Log($"Sync of playlist {localPlaylist.Name} complete");

                return localPlaylist;
            }
            catch (Exception e)
            {
                Debug.LogError("An error occurred in FromYoutubePlaylist!");
                Debug.LogException(e);
                return null;
            }
        }

    }
}