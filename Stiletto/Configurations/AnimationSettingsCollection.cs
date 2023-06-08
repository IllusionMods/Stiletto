using Stiletto.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Stiletto.Configurations
{
    public class AnimationSettingsCollection<T> where T: TextSerializer, new()
    {
        private readonly static object _locker = new object();

        public AnimationSettingsCollection(string filePath)
        {
            FilePath = filePath;
            KeyFlags = new ConcurrentDictionary<string, T>();
            PathFlags = new ConcurrentDictionary<string, T>();
            RegexFlags = new ConcurrentList<RegexMatcher<T>>();

            foreach (var value in GetValues(GetFileLines()))
            {
                switch (value.Type)
                {
                    case MatcherType.Key:
                        KeyFlags[value.Matcher] = value.Value;
                        break;

                    case MatcherType.Path:
                        PathFlags[value.Matcher] = value.Value;
                        break;

                    case MatcherType.Regex:
                        RegexFlags.Add(new RegexMatcher<T>(value.Matcher, value.Value));
                        break;
                }
            }
        }

        public string FilePath { get; }

        public ConcurrentDictionary<string, T> KeyFlags { get; }

        public ConcurrentDictionary<string, T> PathFlags { get; }

        public ConcurrentList<RegexMatcher<T>> RegexFlags { get; }

        public T Load(string path, string name)
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

        public void Save(string path, string name, T settings)
        {
            var update = new AnimationSettingsValue<T>(path, name, settings);

            if (update.Type == MatcherType.Key)
                KeyFlags[update.Matcher] = update.Value;

            if (update.Type == MatcherType.Path)
                PathFlags[update.Matcher] = update.Value;

            SaveToFile(update);
        }

        public void Delete(string path, string name)
        {
            var delete = new AnimationSettingsValue<T>(path, name, null);

            if (delete.Type == MatcherType.Key)
                KeyFlags.Remove(delete.Matcher);

            if (delete.Type == MatcherType.Path)
                PathFlags.Remove(delete.Matcher);

            SaveToFile(delete);
        }

        private void SaveToFile(AnimationSettingsValue<T> update)
        {
            var lines = GetFileLines();
            var values = GetValues(lines);
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
                else if (update.Value != null)
                {
                    File.AppendAllText(FilePath, Environment.NewLine + update.ToString());
                }
            }
        }

        private List<AnimationSettingsValue<T>> GetValues(string[] lines)
        {
            var values = new List<AnimationSettingsValue<T>>();
            for (var i = 0; i < lines.Length; i++)
            {
                var value = AnimationSettingsValue<T>.Parse(lines[i]);
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
            lock (_locker)
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
