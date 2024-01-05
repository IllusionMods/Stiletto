using Stiletto.Attributes;

namespace Stiletto.Settings
{
    public class GeneralSettings
    {
        [FileProperty]
        public float CustomPose_BodyLengthPower { get; set; } = 0.075f;

        [FileProperty]
        public float CustomPose_BodyLengthMultiplier { get; set; } = 0.3f;

        [FileProperty]
        public float CustomPose_HeelHeightPower { get; set; } = 0.75f;

        [FileProperty]
        public float CustomPose_HeelHeightMultiplier { get; set; } = 32f;

        [FileProperty]
        public float KneeBend_HeelHeightMultiplier { get; set; } = 1f;

        [FileProperty]
        public string DisplaySettingsFilePath { get; set; } = "_display.txt";

        [FileProperty]
        public bool Enable_On_Start { get; set; } = true;
    }
}
