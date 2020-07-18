using Stiletto.Configurations;
using Stiletto.Models;
using System;
using System.IO;

namespace Stiletto
{
    public static class StilettoContext
    {
        public static DynamicProvider<CustomPose> _customPoseProvider = new DynamicProvider<CustomPose>(RootSettings.CUSTOM_POSE_PATH);
        public static DynamicProvider<CustomHeel> _customHeelProvider = new DynamicProvider<CustomHeel>(RootSettings.CUSTOM_HEEL_PATH);
        public static AnimationFlagsProvider _animationFlagsProvider = new AnimationFlagsProvider(RootSettings.ANIMATION_FLAGS_PATH, RootSettings.FLAG_DEFAULT_PATH, RootSettings.FLAG_DUMP_PATH);

        public static void RegisterHeelInfo(HeelInfo heelInfo)
        {
            HeelInfos.Add(heelInfo);
        }

        public static void UnregisterHeelInfo(HeelInfo heelInfo)
        {
            HeelInfos.Remove(heelInfo);
        }

        public static DynamicProvider<CustomPose> CustomPoseProvider => _customPoseProvider;

        public static DynamicProvider<CustomHeel> CustomHeelProvider => _customHeelProvider;

        public static AnimationFlagsProvider AnimationFlagsProvider => _animationFlagsProvider;

        public static int Count => HeelInfos.Count;

        public static int LastIndex => HeelInfos.Count - 1;

        public static ConcurrentList<HeelInfo> HeelInfos { get; } = new ConcurrentList<HeelInfo>();

        public static void Initalize()
        {
            var directories = new DirectoryInfo[]
            {
                new DirectoryInfo(RootSettings.CONFIG_PATH),
                new DirectoryInfo(RootSettings.CUSTOM_HEEL_PATH),
                new DirectoryInfo(RootSettings.ANIMATION_FLAGS_PATH),
                new DirectoryInfo(RootSettings.CUSTOM_POSE_PATH)
            };

            foreach (var directory in directories)
            {
                if (!directory.Exists) directory.Create();
            }

            ReloadConfigurations();
        }

        public static void ReloadConfigurations()
        {
            _animationFlagsProvider.Reload();
            _customPoseProvider.Reload();
            _customHeelProvider.Reload();

            foreach (var heelInfo in HeelInfos)
            {
                heelInfo.Reload();
            }
        }

        public static void NotifyHeelInfoUpdate(HeelInfo heelInfo)
        {
            if (heelInfo != null) 
            {
                OnHeelInfoUpdate(null, new HeelInfoEventArgs(heelInfo));
            }
        }

        public static event EventHandler<HeelInfoEventArgs> OnHeelInfoUpdate;
    }
}
