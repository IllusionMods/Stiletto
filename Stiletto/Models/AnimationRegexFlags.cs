using System.Text.RegularExpressions;

namespace Stiletto.Models
{
    public class AnimationRegexFlags
    {
        public AnimationRegexFlags(string matcher, AnimationFlags value)
        {
            Matcher = new Regex(matcher);
            Value = value;
        }

        public Regex Matcher { get; }

        public AnimationFlags Value { get; }
    }
}
