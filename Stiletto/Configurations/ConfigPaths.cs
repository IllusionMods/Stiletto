using BepInEx;
using System.IO;

namespace Stiletto.Configurations
{
    public static class ConfigPaths
    {
        public static readonly string CONFIG_PATH = Path.Combine(Paths.ConfigPath, "Stiletto");
        public static readonly string HEEL_PATH = Path.Combine(CONFIG_PATH, "Heels");
        public static readonly string FLAG_PATH = Path.Combine(CONFIG_PATH, "_flags.txt");
    }
}
