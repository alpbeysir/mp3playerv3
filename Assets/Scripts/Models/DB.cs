using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using MP3Player.Misc;
using LiteDB;
using UnityEngine;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MP3Player.Models
{
    public class DBObject<T> where T : DBObject<T>, new()
    {
        public string Id { get; set; }
        public DBObject() { }

        private static ConcurrentDictionary<string, object> instanceCache = new();

        public static T Get(string id)
        {
            if (id == null) return null;

            T dbObj;
            if (instanceCache.ContainsKey(id))
            {
                //Debug.Log(string.Format("Cache already has {0} with id {1}, returning cached instance", typeof(T).Name, id));
                return (T)instanceCache[id];
            }

            var col = DB.Instance.GetCollection<T>();
            dbObj = col.FindById(id);

            if (dbObj == null)
            {
                dbObj = new();
                dbObj.Id = id;
                //Debug.Log(string.Format("Database doesn't have {0} with id {1}, creating new instance", typeof(T).Name, id));
            }
            else
            {
                //Debug.Log(string.Format("First access to {0} with id {1}, queried database", typeof(T).Name, id));
            }

            instanceCache[id] = dbObj;
            return dbObj;
        }

        public static async Task<T> GetAsync(string id)
        {
            if (id == null) return null;

            T dbObj = null;
            if (instanceCache.ContainsKey(id))
            {
                //Debug.Log(string.Format("Cache already has {0} with id {1}, returning cached instance", typeof(T).Name, id));
                return (T)instanceCache[id];
            }

            await Task.Run(() => { 
                var col = DB.Instance.GetCollection<T>();
                dbObj = col.FindById(id);
            });

            if (dbObj == null)
            {
                dbObj = new();
                dbObj.Id = id;
                //Debug.Log(string.Format("Database doesn't have {0} with id {1}, creating new instance", typeof(T).Name, id));
            }
            else
            {
                //Debug.Log(string.Format("First access to {0} with id {1}, queried database", typeof(T).Name, id));
            }

            instanceCache[id] = dbObj;
            return dbObj;
        }

        public void Save()
        {
            DB.Instance.GetCollection<T>().Upsert((T)this);
        }

        public async Task SaveAsync()
        {
            await Task.Run(() => DB.Instance.GetCollection<T>().Upsert((T)this));
        }

        public void Delete()
        {
            instanceCache.Remove(Id, out _);
            DB.Instance.GetCollection<T>().Delete(Id);
        }
    }

    public static class DB
    {
        private static LiteDatabase db;
        public static LiteDatabase Instance
        {
            get
            {
                if (db == null)
                {
                    InitializeDB();
                }
                return db;
            }
        }

        public static void PreCacheInstance()
        {
            ThreadPool.QueueUserWorkItem((obj) => {
                lock (db)
                {
                    InitializeDB();
                }
            });
        }
        
        private static void InitializeDB()
        {
            Utils.CreateDirFromPath(Utils.DataPath + "db");
            db = new LiteDatabase(Utils.DataPath + "db");
        }

        public static void Dispose()
        {
            if (db == null) return;

            db.Dispose();
            db = null;
        }
    }
}