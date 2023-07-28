using Stiletto.Models;

namespace Stiletto.Configurations
{
    public enum MatcherType
    {
        Key,
        Path,
        Regex
    }

    public class AnimationSettingsValue<T> where T: TextSerializer, new()
    {
        public int? LineIndex { get; set; }

        public MatcherType Type { get; set; }

        public string Matcher { get; set; }

        public T Value { get; set; }

        public string AnimationPath
        {
            get
            {
                if (Type == MatcherType.Key)
                    return Matcher.Split('/')[0];

                if (Type == MatcherType.Path)
                    return Matcher;

                return null;
            }
        }

        public string AnimationName
        {
            get
            {
                if (Type == MatcherType.Key)
                    return Matcher.Split('/')[1];

                return null;
            }
        }

        public AnimationSettingsValue(string path, string name, T value)
        {
            if (string.IsNullOrEmpty(path) && string.IsNullOrEmpty(name))
            {
                Type = MatcherType.Regex;
                Matcher = "";
            }
            else if (string.IsNullOrEmpty(name))
            {
                Type = MatcherType.Path;
                Matcher = path;
            }
            else
            {
                Type = MatcherType.Key;
                Matcher = $"{path}/{name}";
            }
            Value = value;
        }

        public AnimationSettingsValue(string operation, string value)
        {
            Value = new T();
            Value.Deserialize(value);

            var parts = operation.Split(':');

            if (parts.Length == 2)
            {
                Matcher = parts[1].Trim();

                var type = parts[0].Trim();

                if (type == "p")
                    Type = MatcherType.Path;

                if (type == "r")
                    Type = MatcherType.Regex;
            }
            else
            {
                Matcher = operation;
                Type = MatcherType.Key;
            }
        }

        public static AnimationSettingsValue<T> Parse(string line)
        {
            if (line.StartsWith(";"))
                return null;

            var args = line.Split('=');
            if (args.Length != 2)
                return null;

            return new AnimationSettingsValue<T>(args[0].Trim(), args[1].Trim());
        }

        public override string ToString()
        {
            if (Value == null)
                return "";

            var typePrefix = "";

            if (Type == MatcherType.Path)
                typePrefix = "p:";

            if (Type == MatcherType.Regex)
                typePrefix = "r:"; ;

            return $"{typePrefix}{Matcher}={Value.Serialize()}";
        }
    }
}
