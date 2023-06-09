using BepInEx;
using System.IO;

namespace Stiletto.Settings
{
    public static class FilePathSettings
    {
        public static readonly string CONFIG_PATH = Path.Combine(Paths.ConfigPath, "Stiletto");
        public static readonly string CUSTOM_HEEL_PATH = Path.Combine(CONFIG_PATH, "Heels");
        public static readonly string CUSTOM_POSE_PATH = Path.Combine(CONFIG_PATH, "Poses");
        public static readonly string ANIMATION_FLAGS_PATH = Path.Combine(CONFIG_PATH, "Flags");

        public static readonly string FLAG_DEFAULT_PATH = Path.Combine(CONFIG_PATH, "_flags.txt");
        public static readonly string FLAG_DUMP_PATH = Path.Combine(CONFIG_PATH, "_dumps.txt");
        public static readonly string GENERAL_SETTINGS_PATH = Path.Combine(CONFIG_PATH, "_settings.txt");
    }
}
