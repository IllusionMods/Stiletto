using Stiletto.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Stiletto.Configurations
{
    public class AnimationFlagsProvider
    {
        private AnimationSettingsCollection<AnimationFlags> _defaultSetting;
        private AnimationSettingsCollection<AnimationFlags> _dumpSetting;
        private List<AnimationSettingsCollection<AnimationFlags>> _settings;
        private ConcurrentDictionary<string, AnimationFlags> _cache = new ConcurrentDictionary<string, AnimationFlags>();

        private readonly string _rootDirectory;
        private readonly string _defaultFile;
        private readonly string _dumpFile;

        private static object locker = new object();

        private static AnimationFlags DefaultFlags = new AnimationFlags() {
                ACTIVE = true,
                TOE_ROLL = true,
                ANKLE_ROLL = true,
                HEIGHT = true
        };

        public AnimationFlagsProvider(string rootDirectory, string defaultFile, string dumpFile) 
        {
            _rootDirectory = rootDirectory;
            _defaultFile = defaultFile;
            _dumpFile = dumpFile;
            Directory.CreateDirectory(_rootDirectory);
            Reload();
        }

        public void Initialize()
        {
            _defaultSetting.Save(null, null, DefaultFlags);
        }

        public void Save(string path, string name, AnimationFlags flags) 
        {
            lock(locker) 
            {
                foreach (var config in _settings)
                {
                    var existing = config.Load(path, name);
                    if (existing != null)
                    {
                        config.Save(path, name, flags);
                        return;
                    }
                }
                _cache.Clear();
                _defaultSetting.Save(path, name, flags);
                _dumpSetting.Delete(path, name);
            }
        }

        public AnimationFlags Load(string path, string name) 
        {
            var cacheKey = $"{path}/{name}";

            if (_cache.ContainsKey(cacheKey))
            {
                return _cache[cacheKey];
            }

            foreach (var config in _settings)
            {
                var flags = config.Load(path, name);
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
                _dumpSetting.Save(path, name, flags);
                return flags;
            }
        }

        public void Reload()
        {
            lock (locker)
            {
                _settings = Directory.GetFiles(_rootDirectory, "*.txt", SearchOption.AllDirectories)
                    .OrderBy(x => x)
                    .Select(x => new AnimationSettingsCollection<AnimationFlags>(x))
                    .ToList();

                _defaultSetting = new AnimationSettingsCollection<AnimationFlags>(_defaultFile);
                _dumpSetting = new AnimationSettingsCollection<AnimationFlags>(_dumpFile);

                _settings.Add(_defaultSetting);
                _settings.Add(_dumpSetting);

                _cache.Clear();
            }
        }
    }
}
