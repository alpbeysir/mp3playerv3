using System;
using System.Collections.Generic;
using LiteDB;
using UnityEngine;

public class DBObject<T> where T : DBObject<T>
{
    public string Id { get; set; }
    public DBObject() { }

    //private static Dictionary<string, object> instanceCache = new();

    public static bool TryLoad<A>(string id, out A dbObj) where A : DBObject<A>, new()
    {
        //if (instanceCache.ContainsKey(id))
        //{
        //    dbObj = (A)instanceCache[id];
        //    return true;
        //}

        var col = DB.Instance.GetCollection<A>();
        dbObj = col.FindById(id);
        if (dbObj == null)
        {
            dbObj = new();
            dbObj.Id = id;
            //instanceCache[id] = dbObj;
            return false;
        }
        else
        {
            //instanceCache[id] = dbObj;
            return true;
        }
    }

    public void Save()
    {
        var col = DB.Instance.GetCollection<T>();
        col.Upsert((T)this);
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
        db.Dispose();
    }
}

