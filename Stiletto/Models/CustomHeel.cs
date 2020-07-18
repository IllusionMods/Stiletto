using Stiletto.Attributes;

namespace Stiletto.Models
{
    public class CustomHeel
    {
        public CustomHeel() { }

        public CustomHeel(HeelInfo heelInfo)
        {
            AnkleAngle = heelInfo.AnkleAngle;
            LegAngle = heelInfo.LegAngle;
            Height = heelInfo.Height;
        }

        [FileProperty("angleAnkle")]
        public float AnkleAngle { get; set; }

        [FileProperty("angleLeg")]
        public float LegAngle { get; set; }

        [FileProperty("height")]
        public float Height { get; set; }
    }
}
