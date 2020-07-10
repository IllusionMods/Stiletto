using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;

namespace Stiletto.Configurations
{
    public static class HeelConfigProvider
    {
        private const string ANGLE_ANKLE = "angleAnkle";
        private const string ANGLE_LEG = "angleLeg";
        private const string HEIGHT = "height";

        public static void SaveHeelFile(string name, HeelConfig config)
        {
            var lines = new string[] {
                $"{ANGLE_ANKLE}={config.AngleAnkle}",
                $"{ANGLE_LEG}={config.AngleLeg}",
                $"{HEIGHT}={config.Height}"
            };
            File.WriteAllLines(GetHeelConfigPath(name), lines);
        }

        public static HeelConfig LoadHeelFile(string name) 
        {
            var config = new HeelConfig
            {
                AngleAnkle = 0,
                AngleLeg = 0,
                Height = 0
            };

            var configFile = GetHeelConfigPath(name);
            if (File.Exists(configFile))
            {
                foreach (var line in File.ReadAllLines(configFile)) 
                {
                    var parts = line.Split('=');
                    if (parts.Length > 1) 
                    {
                        switch (parts[0]) {
                            case ANGLE_ANKLE:
                                config.AngleAnkle = float.Parse(parts[1]);
                                break;
                            case ANGLE_LEG:
                                config.AngleLeg = float.Parse(parts[1]);
                                break;
                            case HEIGHT:
                                config.Height = float.Parse(parts[1]);
                                break;
                        }
                    }
                }
            }
            return config;
        }

        private static string GetHeelConfigPath(string name) 
        {
            return Path.Combine(ConfigPaths.HEEL_PATH, $"{name}.txt");
        }
    }
}
