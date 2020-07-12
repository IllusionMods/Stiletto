using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Stiletto.Configurations
{
    public class RegexHeelFLags
    {
        public Regex Test { get; set; }

        public HeelFlags Flags { get; set; }
    }

    public class HeelFlagsConfig
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public HeelFlagsConfig(string filePath)
        {
            FilePath = filePath;

            KeyFlags = new Dictionary<string, HeelFlags>();
            PathFlags = new Dictionary<string, HeelFlags>();
            RegexFlags = new List<RegexHeelFLags>();

            LoadDictionaries();
        }

        public HeelFlags? GetHeelFlags(string path, string name)
        {
            try
            {
                _lock.EnterReadLock();

                var key = GetFlagKey(path, name);

                if (KeyFlags.ContainsKey(key))
                    return KeyFlags[key];

                if (PathFlags.ContainsKey(path))
                    return PathFlags[path];

                var match = RegexFlags.FirstOrDefault(x => x.Test.IsMatch(key));

                if (match != null)
                {
                    return match.Flags;
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return null;
        }

        public void SaveHeelFlags(string path, string name, HeelFlags flags)
        {
            try
            {
                _lock.EnterWriteLock();

                var isPathFlags = string.IsNullOrEmpty(name);

                var key = isPathFlags ? $"p:{path}" : GetFlagKey(path, name);

                if (isPathFlags)
                {
                    PathFlags[path] = flags;
                }
                else
                {
                    KeyFlags[key] = flags;
                }

                UpdateFile(key, flags.ToString());
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void DeleteHeelFlags(string path, string name)
        {
            var key = GetFlagKey(path, name);
            try
            {
                _lock.EnterWriteLock();

                if (KeyFlags.ContainsKey(key))
                {
                    KeyFlags.Remove(key);

                }

                UpdateFile(key, null);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public Dictionary<string, HeelFlags> KeyFlags { get; }

        public Dictionary<string, HeelFlags> PathFlags { get; }

        public List<RegexHeelFLags> RegexFlags { get; }

        public string FilePath { get; }

        private void LoadDictionaries()
        {
            if (!File.Exists(FilePath)) return;
            try
            {
                _lock.EnterWriteLock();

                var lines = File.ReadAllLines(FilePath);

                foreach (var line in lines.Where(x => !x.StartsWith(";")))
                {
                    var args = line.Split('=');
                    if (args.Length == 2)
                    {
                        var operation = args[0].Trim();
                        var parts = operation.Split(':');

                        var flags = HeelFlags.Parse(args[1].Trim());

                        if (parts.Length == 2)
                        {
                            var type = parts[0].Trim();
                            var value = parts[1].Trim();

                            if (type == "p")
                            {
                                PathFlags[value] = flags;
                            }

                            if (type == "r")
                            {
                                RegexFlags.Add(new RegexHeelFLags
                                {
                                    Test = new Regex(value),
                                    Flags = flags
                                });
                            }
                        }
                        else
                        {
                            KeyFlags[operation] = flags;
                        }
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void UpdateFile(string key, string flags)
        {
            var textLine = !string.IsNullOrEmpty(flags) ? $"{key}={flags}" : "";

            if (File.Exists(FilePath))
            {
                var lines = File.ReadAllLines(FilePath).ToList();
                var updated = false;

                for (var i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    var args = line.Split('=');
                    if (!line.StartsWith(";") && args.Length == 2)
                    {
                        var operation = args[0].Trim();
                        if (operation == key)
                        {
                            lines[i] = textLine;
                            updated = true;
                            break;
                        }
                    }
                }

                if (!updated && !string.IsNullOrEmpty(textLine))
                {
                    lines.Add(textLine);
                }

                File.WriteAllLines(FilePath, lines.ToArray());
            }
            else
            {
                File.WriteAllLines(FilePath, new string[] { textLine });
            }
        }

        private string GetFlagKey(string path, string name)
        {
            return $"{path}/{name}";
        }
    }
}
