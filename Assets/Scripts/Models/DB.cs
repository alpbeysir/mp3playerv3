using System;
using System.Collections.Generic;
using LiteDB;
using UnityEngine;

public class DBObject<T> where T : DBObject<T>
{
    public string Id { get; set; }
    public DBObject() { }

    private static Dictionary<string, object> instanceCache = new();

    public static bool TryLoad<A>(string id, out A dbObj) where A : DBObject<A>, new()
    {
        if (instanceCache.ContainsKey(id))
        {
            Debug.Log(string.Format("Cache already has {0} with id {1}, returning cached instance", typeof(A).Name, id));
            dbObj = (A)instanceCache[id];
            return true;
        }

        var col = DB.Instance.GetCollection<A>();
        dbObj = col.FindById(id);
        if (dbObj == null)
        {
            dbObj = new();
            dbObj.Id = id;
            instanceCache[id] = dbObj;
            Debug.Log(string.Format("Database doesn't have {0} with id {1}, creating new instance", typeof(A).Name, id));
            return false;
        }
        else
        {
            instanceCache[id] = dbObj;
            Debug.Log(string.Format("First access to {0} with id {1}, queried database", typeof(A).Name, id));
            return true;
        }
    }

    public void Save()
    {
        DB.Instance.GetCollection<T>().Upsert((T)this);
    }

    public void Delete()
    {
        if (instanceCache.ContainsKey(Id)) instanceCache.Remove(Id);
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
            //Make sure this /db directory exists
            Utils.CreateDirFromPath(Utils.DataPath + "db");
            if (db == null) db = new LiteDatabase(Utils.DataPath + "db");
            return db;
        }
    }

    public static void Dispose()
    {
        if (db == null) return;

        db.Dispose();
        db = null;
    }
}

