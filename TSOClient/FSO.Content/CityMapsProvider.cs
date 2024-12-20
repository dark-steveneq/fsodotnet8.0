using FSO.Common;
using FSO.Common.Content;
using FSO.Content.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FSO.Content
{
    public class CityMapsProvider : IContentProvider<CityMap>
    {
        private Dictionary<int, string> DirCache;
        private Content Content;
        private static int PoolID = CacheControler.NewPool("CityMapProvider");
        
        public CityMapsProvider(Content content)
        {
            this.Content = content;
            CacheControler.UsePool(PoolID);
        }

        ~CityMapsProvider() => CacheControler.FreePool(PoolID);

        public void Init()
        {
            DirCache = new Dictionary<int, string>();

            var dir = Content.GetPath("cities");
            foreach (var map in Directory.GetDirectories(dir))
            {
                var id = int.Parse(Path.GetFileName(map).Replace("city_", ""));
                DirCache.Add(id, map);
            }

            dir = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), FSOEnvironment.ContentDir), "Cities/");
            foreach (var map in Directory.GetDirectories(dir))
            {
                var id = int.Parse(Path.GetFileName(map).Replace("city_", ""));
                DirCache.Add(id, map);
            }
        }

        public CityMap Get(string id)
        {
            return Get(ulong.Parse(id));
        }

        public CityMap Get(ulong id)
        {
            CityMap result = CacheControler.Get<CityMap>(PoolID, id);
            if (result == null)
                result = new CityMap(DirCache[(int)id]);
                CacheControler.Cache(PoolID, id, result);
            return result;
        }

        public CityMap Get(uint type, uint fileID)
        {
            throw new NotImplementedException();
        }

        public List<IContentReference<CityMap>> List()
        {
            throw new NotImplementedException();
        }

        public CityMap Get(ContentID id)
        {
            throw new NotImplementedException();
        }
    }
}
