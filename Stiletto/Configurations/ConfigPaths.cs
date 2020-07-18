using BepInEx;
using System.IO;

namespace Stiletto.Configurations
{
    public static class ConfigPaths
    {
        public static readonly string CONFIG_PATH = Path.Combine(Paths.ConfigPath, "Stiletto");
        public static readonly string HEEL_PATH = Path.Combine(CONFIG_PATH, "Heels");
        public static readonly string FLAGS_PATH = Path.Combine(CONFIG_PATH, "Flags");
        public static readonly string POSE_PATH = Path.Combine(CONFIG_PATH, "Poses");

        public static readonly string FLAG_DEFAULT_PATH = Path.Combine(CONFIG_PATH, "_flags.txt");
        public static readonly string FLAG_DUMP_PATH = Path.Combine(CONFIG_PATH, "_dumps.txt");

        public static void Initalize() {
            var directories = new DirectoryInfo[] {
                new DirectoryInfo(CONFIG_PATH),
                new DirectoryInfo(HEEL_PATH),
                new DirectoryInfo(FLAGS_PATH),
                new DirectoryInfo(POSE_PATH)
            };

            foreach (var directory in directories)
            {
                if (!directory.Exists) directory.Create();
            }
        }
    }
}
