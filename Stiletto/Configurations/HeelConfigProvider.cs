using System.IO;

namespace Stiletto.Configurations
{
    public static class HeelConfigProvider
    {
        private const string ANKLE_ANGLE = "angleAnkle";
        private const string LEG_ANGLE = "angleLeg";
        private const string HEIGHT = "height";

        public static void SaveHeelFile(string name, HeelConfig config)
        {
            var lines = new string[] {
                $"{ANKLE_ANGLE}={config.AnkleAngle}",
                $"{LEG_ANGLE}={config.LegAngle}",
                $"{HEIGHT}={config.Height}"
            };
            File.WriteAllLines(GetHeelConfigPath(name), lines);
        }

        public static HeelConfig LoadHeelFile(string name) 
        {
            var config = new HeelConfig
            {
                AnkleAngle = 0,
                LegAngle = 0,
                Height = 0
            };

            if (string.IsNullOrEmpty(name)) 
                return config;

            var configFile = GetHeelConfigPath(name);
            if (File.Exists(configFile))
            {
                foreach (var line in File.ReadAllLines(configFile)) 
                {
                    var parts = line.Split('=');
                    if (parts.Length > 1) 
                    {
                        switch (parts[0]) {
                            case ANKLE_ANGLE:
                                config.AnkleAngle = float.Parse(parts[1]);
                                break;
                            case LEG_ANGLE:
                                config.LegAngle = float.Parse(parts[1]);
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
