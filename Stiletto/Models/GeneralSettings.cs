using Stiletto.Attributes;

namespace Stiletto.Models
{
    public class GeneralSettings
    {
        [FileProperty]
        public float CustomPose_BodyLengthPower { get; set; } = 3f;

        [FileProperty]
        public float CustomPose_BodyLengthMultiplier { get; set; } = 3f;

        [FileProperty]
        public float CustomPose_HeelHeightPower { get; set; } = 0.75f;

        [FileProperty]
        public float CustomPose_HeelHeightMultiplier { get; set; } = 32f;
    }
}
