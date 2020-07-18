using System.Linq;

namespace Stiletto.Models
{
    public class AnimationFlags
    {
        public bool ACTIVE { get; set; }
        public bool HEIGHT { get; set; }
        public bool TOE_ROLL { get; set; }
        public bool ANKLE_ROLL { get; set; }
        public bool KNEE_BEND { get; set; }
        public bool CUSTOM_POSE { get; set; }

        public override string ToString()
        {
            return string.Join(",", new[] { 
                ACTIVE, HEIGHT, TOE_ROLL, ANKLE_ROLL, KNEE_BEND, CUSTOM_POSE 
            }.Select(x => x ? "1" : "0").ToArray());
        }

        public static AnimationFlags Parse(string s)
        {
            var args = s.Split(',').Select(x => x.Trim() == "1").ToArray();
            var hf = new AnimationFlags();

            if (args.Length <= 0) return hf;

            hf.ACTIVE = args.ElementAtOrDefault(0);
            hf.HEIGHT = args.ElementAtOrDefault(1);
            hf.TOE_ROLL = args.ElementAtOrDefault(2);
            hf.ANKLE_ROLL = args.ElementAtOrDefault(3);
            hf.KNEE_BEND = args.ElementAtOrDefault(4);
            hf.CUSTOM_POSE = args.ElementAtOrDefault(5);

            return hf;
        }
    }
}