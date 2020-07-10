using System.Linq;

namespace Stiletto.Configurations
{
    public struct HeelFlags
    {
        public bool ACTIVE { get; set; }
        public bool HEIGHT { get; set; }
        public bool TOE_ROLL { get; set; }
        public bool ANKLE_ROLL { get; set; }
        public bool KNEE_BEND { get; set; }

        public override string ToString()
        {
            return string.Join(",", new[] { ACTIVE, HEIGHT, TOE_ROLL, ANKLE_ROLL, KNEE_BEND }.Select(x => x ? "1" : "0").ToArray());
        }

        public static HeelFlags Parse(string s)
        {
            var args = s.Split(',').Select(x => x.Trim() == "1").ToArray();
            var hf = new HeelFlags();
            if (args.Length != 5) return hf;
            hf.ACTIVE = args[0];
            hf.HEIGHT = args[1];
            hf.TOE_ROLL = args[2];
            hf.ANKLE_ROLL = args[3];
            hf.KNEE_BEND = args[4];
            return hf;
        }
    }
}