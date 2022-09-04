using MP3Player.Misc;
using Google.Apis.Util.Store;
using LiteDB;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MP3Player.Models
{
    public class SecureDataStore : IDataStore
    {
        //TODO find a better way
        private const string ENCRYPTION_KEY = "WnPadTztfJeZsgnaDMjSBnhf";
        private static readonly string DB_ID = typeof(SecureDataStore).Name;

        public string Id { get; set; }
        public Dictionary<string, string> Data { get; set; }

        private static SecureDataStore _instance;
        public static SecureDataStore Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = DBInstance.GetCollection<SecureDataStore>().FindById(DB_ID);
                    if (_instance == null)
                    {
                        _instance = new();
                        DBInstance.GetCollection<SecureDataStore>().Upsert(_instance);
                    }
                }
                return _instance;
            }
        }

        private static LiteDatabase db;
        private static LiteDatabase DBInstance

        {
            get
            {
                string filename = Utils.DataPath + "enc-db";
                string password = ENCRYPTION_KEY;
                Utils.CreateDirFromPath(filename);
                if (db == null) db = new LiteDatabase(string.Format("Filename={0};Password={1}", filename, password));
                return db;
            }
        }

        public Task ClearAsync()
        {
            Data.Clear();
            DBInstance.GetCollection<SecureDataStore>().Upsert(this);
            return Task.CompletedTask;
        }

        public Task DeleteAsync<T>(string key)
        {
            Data.Remove(key);
            DBInstance.GetCollection<SecureDataStore>().Upsert(this);
            return Task.CompletedTask;
        }

        public Task<T> GetAsync<T>(string key)
        {
            Data.TryGetValue(key, out string val);
            T tmp = default;
            if (val == null) return Task.FromResult(tmp);
            return Task.FromResult(JsonConvert.DeserializeObject<T>(val));
        }

        public Task StoreAsync<T>(string key, T value)
        {
            Data[key] = JsonConvert.SerializeObject(value);
            DBInstance.GetCollection<SecureDataStore>().Upsert(this);
            return Task.CompletedTask;
        }

        public SecureDataStore()
        {
            if (Data == null) Data = new();
            Id = DB_ID;
        }
    }
}