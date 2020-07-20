using Stiletto.Configurations;
using Stiletto.Models;
using Stiletto.Settings;
using System;
using System.IO;

namespace Stiletto
{
    public static class StilettoContext
    {
        public static DynamicProvider<CustomHeel> _customHeelProvider = new DynamicProvider<CustomHeel>(FilePathSettings.CUSTOM_HEEL_PATH);
        public static DynamicFileProvider<GeneralSettings> _generalSettingsProvider = new DynamicFileProvider<GeneralSettings>(FilePathSettings.GENERAL_SETTINGS_PATH);
        public static DynamicFileProvider<DisplaySettings> _displaySettingsProvider = new DynamicFileProvider<DisplaySettings>(GetDisplaySettingsFilePath());

        public static CustomPoseProvider _customPoseProvider = new CustomPoseProvider(FilePathSettings.CUSTOM_POSE_PATH);
        public static AnimationFlagsProvider _animationFlagsProvider = new AnimationFlagsProvider(FilePathSettings.ANIMATION_FLAGS_PATH, FilePathSettings.FLAG_DEFAULT_PATH, FilePathSettings.FLAG_DUMP_PATH);

        public static void RegisterHeelInfo(HeelInfo heelInfo)
        {
            HeelInfos.Add(heelInfo);
        }

        public static void UnregisterHeelInfo(HeelInfo heelInfo)
        {
            HeelInfos.Remove(heelInfo);
        }

        public static CustomPoseProvider CustomPoseProvider => _customPoseProvider;

        public static DynamicProvider<CustomHeel> CustomHeelProvider => _customHeelProvider;

        public static AnimationFlagsProvider AnimationFlagsProvider => _animationFlagsProvider;

        public static int Count => HeelInfos.Count;

        public static int LastIndex => HeelInfos.Count - 1;

        public static ConcurrentList<HeelInfo> HeelInfos { get; } = new ConcurrentList<HeelInfo>();

        public static void Initalize()
        {
            var directories = new DirectoryInfo[]
            {
                new DirectoryInfo(FilePathSettings.CONFIG_PATH),
                new DirectoryInfo(FilePathSettings.CUSTOM_HEEL_PATH),
                new DirectoryInfo(FilePathSettings.ANIMATION_FLAGS_PATH),
                new DirectoryInfo(FilePathSettings.CUSTOM_POSE_PATH)
            };

            foreach (var directory in directories)
            {
                if (!directory.Exists) directory.Create();
            }

            _generalSettingsProvider.Initalize();
            _displaySettingsProvider.Initalize();

            ReloadConfigurations();
        }

        public static void ReloadConfigurations()
        {
            _generalSettingsProvider.Reload();
            _displaySettingsProvider.Reload(GetDisplaySettingsFilePath());
            _animationFlagsProvider.Reload();
            _customPoseProvider.Reload();
            _customHeelProvider.Reload();

            foreach (var heelInfo in HeelInfos)
            {
                heelInfo.Reload();
            }
        }

        private static string GetDisplaySettingsFilePath()
        {
            return Path.Combine(FilePathSettings.CONFIG_PATH, _generalSettingsProvider.Value.DisplaySettingsFilePath);
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
