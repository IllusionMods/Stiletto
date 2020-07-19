using System.Linq;

namespace Stiletto.Models
{
    public class AnimationFlags : TextSerializer
    {
        public AnimationFlags() { }

        public AnimationFlags(string value) : base(value) { }

        public bool ACTIVE { get; set; }
        public bool HEIGHT { get; set; }
        public bool TOE_ROLL { get; set; }
        public bool ANKLE_ROLL { get; set; }
        public bool KNEE_BEND { get; set; }
        public bool CUSTOM_POSE { get; set; }

        public override string Serialize()
        {
            return string.Join(",", new[] {
                ACTIVE, HEIGHT, TOE_ROLL, ANKLE_ROLL, KNEE_BEND, CUSTOM_POSE
            }.Select(x => x ? "1" : "0").ToArray());
        }

        public override void Deserialize(string value)
        {
            var args = value.Split(',').Select(x => x.Trim() == "1").ToArray();
            if (args.Length <= 0) return;

            ACTIVE = args.ElementAtOrDefault(0);
            HEIGHT = args.ElementAtOrDefault(1);
            TOE_ROLL = args.ElementAtOrDefault(2);
            ANKLE_ROLL = args.ElementAtOrDefault(3);
            KNEE_BEND = args.ElementAtOrDefault(4);
            CUSTOM_POSE = args.ElementAtOrDefault(5);
        }
    }
}