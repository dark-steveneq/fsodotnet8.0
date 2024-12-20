using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FSO.Content
{
    /// <summary>
    /// Centralized mempool-like cache system
    /// </summary>
    public class CacheControler
    {
        private static List<Pool> _pools = [new Pool("All")];

        public static List<Pool> Pools { get { return _pools; } }

        /// <summary>
        /// Create a new cache pool
        /// </summary>
        /// <param name="name">Pool's name</param>
        /// <returns>Created pool's ID</returns>
        public static int NewPool(string name)
        {
            _pools.Add(new Pool(name));
            return _pools.Count - 1;
        }

        /// <summary>
        /// Remove pool
        /// </summary>
        /// <param name="poolid">Pool to remove</param>
        internal static void RemovePool(int poolid)
        {
            if (poolid <= 0 && poolid > _pools.Count)
                return;
            _pools.RemoveAt(poolid);
        }

        /// <summary>
        /// Add a use to pool's internal usage counter
        /// </summary>
        /// <param name="poolid">Pool to affect</param>
        public static void UsePool(int poolid)
        {
            if (poolid <= 0 && poolid > _pools.Count)
                return;
            _pools[poolid].ReserveUse();
        }

        /// <summary>
        /// Remove a use to pool's internal usage counter
        /// </summary>
        /// <param name="poolid">Pool to affect</param>
        public static void FreePool(int poolid)
        {
            if (poolid <= 0 && poolid > _pools.Count)
                return;
            _pools[poolid].FreeUsage();
        }

        /// <summary>
        /// Cache object to pool
        /// </summary>
        /// <param name="poolid">Pool to cache to</param>
        /// <param name="id">ID to use</param>
        /// <param name="obj">Object to cache</param>
        /// <returns>Did cache?</returns>
        public static bool Cache(int poolid, string id, object obj) => Cache(poolid, id, "", obj);


        /// <summary>
        /// Cache object to pool
        /// </summary>
        /// <param name="poolid">Pool to cache to</param>
        /// <param name="id">ID to use</param>
        /// <param name="obj">Object to cache</param>
        /// <returns>Did cache?</returns>
        public static bool Cache(int poolid, ulong id, object obj) => Cache(poolid, id, "", obj);

        /// <summary>
        /// Cache object to pool
        /// </summary>
        /// <param name="poolid">Pool to cache to</param>
        /// <param name="id">ID to use</param>
        /// <param name="filename">Filename used to lookup the ID, may be null or empty</param>
        /// <param name="obj">Object to cache</param>
        /// <returns>Did cache?</returns>
        public static bool Cache(int poolid, ulong id, string filename, object obj) => Cache(poolid, id.ToString(), filename, obj);

        /// <summary>
        /// Cache object to pool
        /// </summary>
        /// <param name="poolid">Pool to cache to</param>
        /// <param name="id">ID to use</param>
        /// <param name="filename">Filename used to lookup the ID, may be null or empty</param>
        /// <param name="obj">Object to cache</param>
        /// <returns>Did cache?</returns>
        public static bool Cache(int poolid, string id, string filename, object obj)
        {
            if (poolid <= 0 && poolid > _pools.Count)
                return false;
            return _pools[poolid].Cache(id, filename, obj);
        }

        /// <summary>
        /// Get cached object
        /// </summary>
        /// <typeparam name="T">Type to cast to</typeparam>
        /// <param name="poolid">Pool to fetch from</param>
        /// <param name="id">ID to fetch</param>
        /// <returns>Object instance, null if not found</returns>
        public static T Get<T>(int poolid, ulong id) => Get<T>(poolid, id.ToString());

        /// <summary>
        /// Get cached object by filename
        /// </summary>
        /// <typeparam name="T">Type to cast to</typeparam>
        /// <param name="id">Object to fetch</param>
        /// <returns>Object instance, null if not found</returns>
        public static T Get<T>(int poolid, string id)
        {
            if (poolid <= 0 && poolid > _pools.Count)
                return default(T);

            return _pools[poolid].Get<T>(id);
        }

        /// <summary>
        /// Remove specified object from cache
        /// </summary>
        /// <param name="poolid">Pool to remove from</param>
        /// <param name="id">ID to remove</param>
        /// <returns>Did remove?</returns>
        public static bool Remove(int poolid, string id)
        {
            if (poolid <= 0 && poolid > _pools.Count)
                return false;

            return _pools[poolid].Remove(id);
        }

        /// <summary>
        /// Remove specified object from cache
        /// </summary>
        /// <param name="poolid">Pool to remove from</param>
        /// <param name="id">ID to remove</param>
        /// <returns>Did remove?</returns>
        public static bool Remove(int poolid, ulong id) => Remove(poolid, id.ToString());

        /// <summary>
        /// Check if object is cached
        /// </summary>
        /// <param name="poolid">Pool to check in</param>
        /// <param name="id">ID to check</param>
        /// <returns>Is cached?</returns>
        public static bool Cached(int poolid, string id)
        {
            if (poolid <= 0 && poolid > _pools.Count)
                return false;

            return _pools[poolid].Cached(id);
        }

        /// <summary>
        /// Check if object is cached
        /// </summary>
        /// <param name="poolid">Pool to check in</param>
        /// <param name="id">ID to check</param>
        /// <returns>Is cached?</returns>
        public static bool Cached(int poolid, ulong id) => Cached(poolid, id.ToString());

        /// <summary>
        /// Get type of object
        /// </summary>
        /// <param name="poolid">Pool to get from</param>
        /// <param name="id">Object to get type of</param>  
        /// <returns>Type</returns>
        public static System.Type GetType(int poolid, string id)
        {
            if (poolid <= 0 && poolid > _pools.Count)
                return null;
            return _pools[poolid].GetType(id);
        }

        /// <summary>
        /// Get type of object
        /// </summary>
        /// <param name="poolid">Pool to get from</param>
        /// <param name="id">Object to get type of</param>  
        /// <returns>Type</returns>
        public static System.Type GetType(int poolid, ulong id) => GetType(poolid, id.ToString());
    }

    /// <summary>
    /// Class for storing cache pool information
    /// </summary>
    public class Pool
    {
        private string _name;
        private int _usages = 0;
        private ConcurrentDictionary<string, object> _objects = [];
        private ConcurrentDictionary<string, string> _lookup = [];

        public string Name { get { return _name; } }
        public int Usage { get { return _usages; } }

        /// <summary>
        /// Initialize pool
        /// </summary>
        /// <param name="name">Pool's name shown in profiler</param>
        public Pool(string name)
        {
            this._name = name;
        }

        /// <summary>
        /// Lookup ID if possible and return
        /// </summary>
        /// <param name="id">ID argument to patch</param>
        /// <returns>Patched ID</returns>
        internal string PatchID(string id)
        {
            if (!_objects.ContainsKey(id))
                lock (_lookup)
                    if (_lookup.ContainsKey(id))
                        id = _lookup[id];
            return id;
        }

        /// <summary>
        /// Reserve usage, when usages are 0 cache gets cleared up
        /// </summary>
        internal void ReserveUse()
        {
            _usages++;
        }

        /// <summary>
        /// Free usage, when usages are 0 cache get cleared up
        /// </summary>
        internal void FreeUsage()
        {
            _usages--;
            if (_usages == 0)
            {
                _objects.Clear();
                _lookup.Clear();
            }
        }

        /// <summary>
        /// Cache object to pool
        /// </summary>
        /// <param name="poolid">Pool to cache to</param>
        /// <param name="id">ID to use</param>
        /// <param name="filename">Filename used to lookup the ID, may be null or empty</param>
        /// <param name="obj">Object to cache</param>
        /// <returns>Did cache?</returns>
        internal bool Cache(string id, string filename, object obj)
        {
            if (filename != null && filename != "")
                lock (_lookup)
                    _lookup.TryAdd(filename, id);
            lock (_objects)
                return _objects.TryAdd(id.ToString(), obj);
        }

        /// <summary>
        /// Get object from cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        internal T Get<T>(string id)
        {
            id = PatchID(id);
            lock (_objects)
            {
                if (_objects.ContainsKey(id))
                    return (T)_objects.GetValueOrDefault(id);
            }
            return default(T);
        }

        /// <summary>
        /// Remove object from cache
        /// </summary>
        /// <param name="id">Object to remove</param>
        /// <returns>Did remove?</returns>
        internal bool Remove(string id)
        {
            if (id == null && id == "")
                return false;

            id = PatchID(id);
            lock (_objects)
                _objects[id] = null;
            lock (_lookup)
                foreach (var pair in _lookup)
                    if (pair.Value == id)
                        _lookup.TryRemove(pair);

            return true;
        }

        /// <summary>
        /// Lookup filename from ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string LookupName(string id)
        {
            if (id == null && id == "")
                return "";

            lock (_lookup)
                foreach (var pair in _lookup)
                    if (pair.Value == id)
                        return pair.Key;

            return "";
        }

        /// <summary>
        /// Check if object is cached
        /// </summary>
        /// <param name="id">ID to check</param>
        /// <returns>Is cached?</returns>
        internal bool Cached(string id)
        {
            if (id == null && id == "")
                return false;
            id = PatchID(id);
            return _objects.ContainsKey(id);
        }

        /// <summary>
        /// Get type of object
        /// </summary>
        /// <param name="id">Object to get type of</param>  
        /// <returns>Type</returns>
        internal System.Type GetType(string id)
        {
            if (id == null && id == "")
                return null;
            id = PatchID(id);
            return _objects.GetValueOrDefault(id).GetType();
        }

        /// <summary>
        /// Return a dictionary of IDs and filenames
        /// </summary>
        /// <returns>Dictionary with IDs as keys and filenames as values</returns>
        public Dictionary<string, string> GetContents()
        {
            Dictionary<string, string> contents = [];
            lock (_lookup)
                foreach(var pair in _lookup)
                    if (!contents.ContainsKey(pair.Value))
                        contents.Add(pair.Value, pair.Key);

            lock (_objects)
                foreach (var pair in _objects)
                    if (!contents.ContainsKey(pair.Key))
                        contents.Add(pair.Key, "");

            return contents;
        }
    }
}
