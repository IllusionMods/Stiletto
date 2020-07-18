using Stiletto.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Stiletto.Configurations
{
    public class AnimationFlagsProvider
    {
        private AnimationFlagsConfig _defaultConfig;
        private AnimationFlagsConfig _dumpConfig;
        private List<AnimationFlagsConfig> _configs;
        private ConcurrentDictionary<string, AnimationFlags> _cache = new ConcurrentDictionary<string, AnimationFlags>();
        private readonly string _rootDir;
        private readonly string _defaultFile;
        private readonly string _dumpFile;

        private static object locker = new object();

        public AnimationFlagsProvider(string rootDir, string defaultFile, string dumpFile) 
        {
            _rootDir = rootDir;
            _defaultFile = defaultFile;
            _dumpFile = dumpFile;
            Reload();
        }

        public void Save(string path, string name, AnimationFlags flags) 
        {
            lock(locker) 
            {
                foreach (var config in _configs)
                {
                    var existing = config.GetAnimationFlags(path, name);
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

        public AnimationFlags Load(string path, string name) 
        {
            var cacheKey = GetCacheKey(path, name);

            if (_cache.ContainsKey(cacheKey))
            {
                return _cache[cacheKey];
            }

            foreach (var config in _configs)
            {
                var flags = config.GetAnimationFlags(path, name);
                if (flags != null)
                {
                    _cache[cacheKey] = flags;
                    return flags;
                }
            }

            lock (locker)
            {
                var flags = new AnimationFlags();
                _cache[cacheKey] = flags;
                _dumpConfig.SaveHeelFlags(path, name, flags);
                return flags;
            }
        }

        public void Reload()
        {
            lock (locker)
            {
                _configs = Directory.GetFiles(_rootDir, "*.txt", SearchOption.AllDirectories)
                    .OrderBy(x => x)
                    .Select(x => new AnimationFlagsConfig(x))
                    .ToList();

                _defaultConfig = new AnimationFlagsConfig(_defaultFile);
                _dumpConfig = new AnimationFlagsConfig(_dumpFile);

                _configs.Add(_defaultConfig);
                _configs.Add(_dumpConfig);

                _cache.Clear();
            }
        }

        private string GetCacheKey(string path, string name) 
        {
            return $"{path}/{name}";
        }
    }
}
