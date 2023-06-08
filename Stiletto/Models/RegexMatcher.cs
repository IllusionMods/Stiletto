using System.Text.RegularExpressions;

namespace Stiletto.Models
{
    public class RegexMatcher<T>
    {
        public RegexMatcher(string matcher, T value)
        {
            Matcher = new Regex(matcher);
            Value = value;
        }

        public Regex Matcher { get; }

        public T Value { get; }
    }
}
