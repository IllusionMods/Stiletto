using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Stiletto.Configurations
{
    public static class HeelFlagsProvider
    {
        private static Dictionary<string, HeelFlags> _dictionary;
        private static object locker = new object();

        public static void SaveFlags(string name, HeelFlags flags) 
        {
            lock(locker) 
            { 
                _dictionary[name] = flags;
                File.WriteAllLines(ConfigPaths.FLAG_PATH, _dictionary.Keys.OrderBy(x => x).Select(x => $"{x}={_dictionary[x]}").ToArray());
            }
        }

        public static HeelFlags GetFlags(string name) 
        {
            if (_dictionary == null) 
            {
                ReloadHeelFlags();
            }

            if (_dictionary.TryGetValue(name, out var flags))
            {
                return flags;
            }
            else {
                flags = new HeelFlags();
                SaveFlags(name, flags);
                return flags;
            }
        }

        public static void ReloadHeelFlags()
        {
            lock (locker)
            {
                _dictionary = LoadHeelFlags();
            }
        }

        private static Dictionary<string, HeelFlags> LoadHeelFlags()
        {
            var dictionary = new Dictionary<string, HeelFlags>();

            if (File.Exists(ConfigPaths.FLAG_PATH))
            {
                foreach (var l in File.ReadAllLines(ConfigPaths.FLAG_PATH).Where(x => !x.StartsWith(";")))
                {
                    var args = l.Split('=');
                    if (args.Length == 2)
                    {
                        var name = args[0].Trim();
                        var flags = args[1].Trim();
                        dictionary[name] = HeelFlags.Parse(flags);
                    }
                }
            }

            return dictionary;
        }
    }
}
