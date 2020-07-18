using Stiletto.Models;

namespace Stiletto.Configurations
{
    public enum AnimationFlagsType
    {
        Key,
        Path,
        Regex
    }

    public class AnimationFlagsValue
    {
        public AnimationFlagsValue(string path, string name, AnimationFlags value)
        {
            if (string.IsNullOrEmpty(name))
            {
                Type = AnimationFlagsType.Path;
                Matcher = path;
            }
            else 
            {
                Type = AnimationFlagsType.Key;
                Matcher = $"{path}/{name}";
            }
            Flags = value;
        }

        public AnimationFlagsValue(string operation, string value)
        {
            Flags = AnimationFlags.Parse(value);

            var parts = operation.Split(':');

            if (parts.Length == 2)
            {
                Matcher = parts[1].Trim();

                var type = parts[0].Trim();

                if (type == "p")
                    Type = AnimationFlagsType.Path;

                if (type == "r")
                    Type = AnimationFlagsType.Regex;
            }
            else
            {
                Matcher = operation;
                Type = AnimationFlagsType.Key;
            }
        }

        public static AnimationFlagsValue Parse(string line)
        {
            if (line.StartsWith(";"))
                return null;

            var args = line.Split('=');
            if (args.Length != 2)
                return null;

            return new AnimationFlagsValue(args[0].Trim(), args[1].Trim());
        }

        public int? LineIndex { get; set; }

        public AnimationFlagsType Type { get; set; }

        public string Matcher { get; set; }

        public AnimationFlags Flags { get; set; }

        public string AnimationPath
        {
            get
            {
                if (Type == AnimationFlagsType.Key)
                    return Matcher.Split('/')[0];

                if (Type == AnimationFlagsType.Path)
                    return Matcher;

                return null;
            }
        }

        public string AnimationName 
        {
            get
            {
                if (Type == AnimationFlagsType.Key)
                    return Matcher.Split('/')[1];

                return null;
            }
        }

        public override string ToString()
        {
            if (Flags == null)
                return "";

            var typePrefix = "";

            if (Type == AnimationFlagsType.Path)
                typePrefix = "p:";

            if (Type == AnimationFlagsType.Regex)
                typePrefix = "r:";;

            return $"{typePrefix}{Matcher}={Flags}";
        }
    }
}
