using Stiletto.Models;
using System;

namespace Stiletto
{
    public class HeelInfoEventArgs : EventArgs
    {
        public HeelInfoEventArgs(HeelInfo heelInfo)
        {
            AnimationPath = heelInfo.animationPath;
            AnimationName = heelInfo.animationName;
            AnimationFlags = heelInfo.flags;

            CustomPose = heelInfo.CustomPose;
            CustomHeel = new CustomHeel(heelInfo);
            HeelName = heelInfo.heelName;
            ChaControl = heelInfo.chaControl;
        }

        public string AnimationPath { get; }

        public string AnimationName { get; }

        public string AnimationKey => $"{AnimationPath}/{AnimationName}";

        public AnimationFlags AnimationFlags { get; }

        public CustomPose CustomPose { get; }

        public CustomHeel CustomHeel { get; }

        public string HeelName { get; }

        public ChaControl ChaControl { get; }
    }
}
