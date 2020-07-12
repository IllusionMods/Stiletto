using Stiletto.Configurations;

namespace Stiletto
{
    public enum HeelFLagsType 
    { 
        Active,
        AnkleRoll,
        ToeRoll,
        Height,
        KneeBend
    }

    public enum HeelInfoType
    {
        AnkleAngle,
        LegAngle,
        Height
    }

    public static class HeelInfoModifier
    {
        public static void UpdateHeelFlags(HeelInfo heelInfo, HeelFLagsType type, bool value)
        {
            if (heelInfo != null)
            {
                switch (type)
                {
                    case HeelFLagsType.Active:
                        heelInfo.SafeProc(x => x.flags.ACTIVE = value);
                        break;
                    case HeelFLagsType.AnkleRoll:
                        heelInfo.SafeProc(x => x.flags.ANKLE_ROLL = value);
                        break;
                    case HeelFLagsType.ToeRoll:
                        heelInfo.SafeProc(x => x.flags.TOE_ROLL = value);
                        break;
                    case HeelFLagsType.Height:
                        heelInfo.SafeProc(x => x.flags.HEIGHT = value);
                        break;
                    case HeelFLagsType.KneeBend:
                        heelInfo.SafeProc(x => x.flags.KNEE_BEND = value);
                        break;
                }
            }
        }

        public static void UpdateHeelInfo(HeelInfo heelInfo, HeelInfoType type, float value)
        {
            if (heelInfo != null)
            {
                switch (type)
                {
                    case HeelInfoType.AnkleAngle:
                        heelInfo.SafeProc(x => x.AnkleAngle = value);
                        break;
                    case HeelInfoType.LegAngle:
                        heelInfo.SafeProc(x => x.LegAngle = value);
                        break;
                    case HeelInfoType.Height:
                        heelInfo.SafeProc(x => x.Height = value);
                        break;
                }
            }
        }

        public static void UpdateHeelInfo(HeelInfo heelInfo, HeelConfig config)
        {
            if (heelInfo != null)
            {
                heelInfo.SafeProc(x => x.AnkleAngle = config.AnkleAngle);
                heelInfo.SafeProc(x => x.LegAngle = config.LegAngle);
                heelInfo.SafeProc(x => x.Height = config.Height);
            }
        }

        public static void SaveHeelInfo(HeelInfo heelInfo)
        {
            if (heelInfo != null)
            {
                HeelConfigProvider.SaveHeelFile(heelInfo.heelName, new HeelConfig
                {
                    AnkleAngle = heelInfo.AnkleAngle,
                    LegAngle = heelInfo.LegAngle,
                    Height = heelInfo.Height,
                });
            }
        }

        public static void SaveHeelFlags(HeelInfo heelInfo)
        {
            if (heelInfo != null)
            {
                HeelFlagsProvider.SaveFlags(heelInfo.animationPath, heelInfo.animationName, heelInfo.flags);
            }
        }
    }
}
