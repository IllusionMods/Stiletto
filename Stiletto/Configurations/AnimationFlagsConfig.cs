using Stiletto.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Stiletto.Configurations
{
    public class AnimationFlagsConfig
    {
        private readonly static object _locker = new object();

        public AnimationFlagsConfig(string filePath)
        {
            FilePath = filePath;
            KeyFlags = new ConcurrentDictionary<string, AnimationFlags>();
            PathFlags = new ConcurrentDictionary<string, AnimationFlags>();
            RegexFlags = new ConcurrentList<AnimationRegexFlags>();

            foreach (var flagsValue in GetAnimationFlagsValues(GetFileLines()))
            {
                switch (flagsValue.Type)
                {
                    case AnimationFlagsType.Key:
                        KeyFlags[flagsValue.Matcher] = flagsValue.Flags;
                        break;

                    case AnimationFlagsType.Path:
                        PathFlags[flagsValue.Matcher] = flagsValue.Flags;
                        break;

                    case AnimationFlagsType.Regex:
                        RegexFlags.Add(new AnimationRegexFlags(flagsValue.Matcher, flagsValue.Flags));
                        break;
                }
            }
        }

        public string FilePath { get; }

        public ConcurrentDictionary<string, AnimationFlags> KeyFlags { get; }

        public ConcurrentDictionary<string, AnimationFlags> PathFlags { get; }

        public ConcurrentList<AnimationRegexFlags> RegexFlags { get; }

        public AnimationFlags GetAnimationFlags(string path, string name)
        {
            var key = GetAnimationKey(path, name);

            if (KeyFlags.ContainsKey(key))
                return KeyFlags[key];

            if (PathFlags.ContainsKey(path))
                return PathFlags[path];

            var match = RegexFlags.FirstOrDefault(x => x.Matcher.IsMatch(key));

            if (match != null)
            {
                return match.Value;
            }

            return null;
        }

        public void SaveHeelFlags(string path, string name, AnimationFlags flags)
        {
            var update = new AnimationFlagsValue(path, name, flags);

            if (update.Type == AnimationFlagsType.Key)
                KeyFlags[update.Matcher] = update.Flags;

            if (update.Type == AnimationFlagsType.Path)
                PathFlags[update.Matcher] = update.Flags;

            SaveAnimationFlagsToFile(update);
        }

        public void DeleteHeelFlags(string path, string name)
        {
            var delete = new AnimationFlagsValue(path, name, null);

            if (delete.Type == AnimationFlagsType.Key)
                KeyFlags.Remove(delete.Matcher);

            if (delete.Type == AnimationFlagsType.Path)
                PathFlags.Remove(delete.Matcher);

            SaveAnimationFlagsToFile(delete);
        }

        private void SaveAnimationFlagsToFile(AnimationFlagsValue update)
        {
            var lines = GetFileLines();
            var values = GetAnimationFlagsValues(lines);
            var updated = false;

            foreach (var value in values)
            {
                if (value.Type == update.Type && value.Matcher == update.Matcher)
                {
                    lines[value.LineIndex.Value] = update.ToString();
                    updated = true;
                    break;
                }
            }
            lock (_locker)
            {
                if (updated)
                {
                    File.WriteAllLines(FilePath, lines);
                }
                else if (update.Flags != null)
                {
                    File.AppendAllText(FilePath, Environment.NewLine + update.ToString());
                }
            }
        }

        private List<AnimationFlagsValue> GetAnimationFlagsValues(string[] lines)
        {
            var values = new List<AnimationFlagsValue>();
            for (var i = 0; i < lines.Length; i++)
            {
                var value = AnimationFlagsValue.Parse(lines[i]);
                if (value != null) 
                {
                    value.LineIndex = i;
                    values.Add(value);
                }
            }

            return values;
        }

        private string[] GetFileLines()
        {
            lock(_locker) 
            { 
                if (!File.Exists(FilePath)) 
                    return new string[0];

                return File.ReadAllLines(FilePath);
            }
        }

        private string GetAnimationKey(string path, string name)
        {
            return $"{path}/{name}";
        }
    }
}
