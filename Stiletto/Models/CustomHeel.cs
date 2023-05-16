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
            ShoeScaleX = heelInfo.ShoeScaleX;
            ShoeScaleY = heelInfo.ShoeScaleY;
            ShoeScaleZ = heelInfo.ShoeScaleZ;
            ShoeAngle = heelInfo.ShoeAngle;
            ShoeTranslateY = heelInfo.ShoeTranslateY;
            ShoeTranslateZ = heelInfo.ShoeTranslateZ;
            ShoeShearY = heelInfo.ShoeShearY;
            ShoeShearZ = heelInfo.ShoeShearZ;
        }

        [FileProperty("angleAnkle")]
        public float AnkleAngle { get; set; }

        [FileProperty("angleLeg")]
        public float LegAngle { get; set; }

        [FileProperty("height")]
        public float Height { get; set; }

        [FileProperty("shoeScaleX")]
        public float ShoeScaleX { get; set; }

        [FileProperty("shoeScaleY")]
        public float ShoeScaleY { get; set; }

        [FileProperty("shoeScaleZ")]
        public float ShoeScaleZ { get; set; }

        [FileProperty("shoeAngle")]
        public float ShoeAngle { get; set; }

        [FileProperty("shoeTranslateY")]
        public float ShoeTranslateY { get; set; }

        [FileProperty("shoeTranslateZ")]
        public float ShoeTranslateZ { get; set; }

        [FileProperty("shoeShearY")]
        public float ShoeShearY { get; set; }

        [FileProperty("shoeShearZ")]
        public float ShoeShearZ { get; set; }
    }
}
