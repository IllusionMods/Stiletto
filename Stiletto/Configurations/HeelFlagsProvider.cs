using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Stiletto.Configurations
{
    public static class HeelFlagsProvider
    {
        private static HeelFlagsConfig _defaultConfig;
        private static HeelFlagsConfig _dumpConfig;
        private static List<HeelFlagsConfig> _configs;
        private static Dictionary<string, HeelFlags> _cache;

        private static object locker = new object();

        public static void SaveFlags(string path, string name, HeelFlags flags) 
        {
            lock(locker) 
            {
                foreach (var config in _configs)
                {
                    var existing = config.GetHeelFlags(path, name);
                    if (existing != null)
                    {
                        config.SaveHeelFlags(path, name, flags);
                        return;
                    }
                }

                var cacheKey = GetCacheKey(path, name);
                if (_cache.ContainsKey(cacheKey))
                {
                    _cache.Remove(cacheKey);
                }

                _defaultConfig.SaveHeelFlags(path, name, flags);
                _dumpConfig.DeleteHeelFlags(path, name);
            }
        }

        public static HeelFlags GetFlags(string path, string name) 
        {
            var cacheKey = GetCacheKey(path, name);

            if (_cache.ContainsKey(cacheKey))
            {
                return _cache[cacheKey];
            }

            foreach (var config in _configs)
            {
                var flags = config.GetHeelFlags(path, name);
                if (flags != null)
                {
                    _cache[cacheKey] = flags.Value;
                    return flags.Value;
                }
            }

            lock (locker)
            {
                var flags = new HeelFlags();
                _cache[cacheKey] = flags;
                _dumpConfig.SaveHeelFlags(path, name, flags);
                return flags;
            }
        }

        public static void ReloadHeelFlags()
        {
            lock (locker)
            {
                var flagsFiles = Directory.GetFiles(ConfigPaths.FLAGS_PATH, "*.txt", SearchOption.AllDirectories);
                _configs = flagsFiles.OrderBy(x => x).Select(x => new HeelFlagsConfig(x)).ToList();

                _defaultConfig = new HeelFlagsConfig(ConfigPaths.FLAG_DEFAULT_PATH);
                _dumpConfig = new HeelFlagsConfig(ConfigPaths.FLAG_DUMP_PATH);

                _configs.Add(_defaultConfig);
                _configs.Add(_dumpConfig);

                _cache = new Dictionary<string, HeelFlags>();
            }
        }

        public static string GetCacheKey(string path, string name) 
        {
            return $"{path}/{name}";
        }
    }
}
