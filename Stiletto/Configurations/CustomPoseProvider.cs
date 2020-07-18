using Sirenix.OdinInspector.Demos;
using Stiletto.Models;
using System.IO;

namespace Stiletto.Configurations
{
    public class CustomPoseProvider
    {
        private const string THIGH_ANGLE = "angleThigh";
        private const string LEG_ANGLE = "angleLeg";
        private const string WAIST_ANGLE = "angleWaist";
        private const string ANKLE_ANGLE = "ankleAngle";

        private static ConcurrentDictionary<string, CustomPose> _cache = new ConcurrentDictionary<string, CustomPose>();

        public static void SaveCustomPose(string name, CustomPose config)
        {
            var lines = new string[] {
                $"{THIGH_ANGLE}={config.ThighAngle}",
                $"{LEG_ANGLE}={config.LegAngle}",
                $"{WAIST_ANGLE}={config.WaistAngle}",
                $"{ANKLE_ANGLE}={config.AnkleAngle}"
            };
            File.WriteAllLines(GetCustomPosePath(name), lines);
            _cache.Remove(name);
        }

        public static CustomPose LoadCustomPose(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new CustomPose();
            }

            if (_cache.ContainsKey(name))
            {
                return _cache[name];
            }

            var config = new CustomPose();
            var configFile = GetCustomPosePath(name);
            if (File.Exists(configFile))
            {
                foreach (var line in File.ReadAllLines(configFile))
                {
                    var parts = line.Split('=');
                    if (parts.Length > 1)
                    {
                        switch (parts[0])
                        {
                            case THIGH_ANGLE:
                                config.ThighAngle = float.Parse(parts[1]);
                                break;
                            case LEG_ANGLE:
                                config.LegAngle = float.Parse(parts[1]);
                                break;
                            case WAIST_ANGLE:
                                config.WaistAngle = float.Parse(parts[1]);
                                break;
                            case ANKLE_ANGLE:
                                config.AnkleAngle = float.Parse(parts[1]);
                                break;
                        }
                    }
                }
            }

            _cache[name] = config;

            return config;
        }

        public static void ClearCache()
        {
            _cache.Clear();
        }

        private static string GetCustomPosePath(string name)
        {
            return Path.Combine(ConfigPaths.POSE_PATH, $"{name}.txt");
        }
    }
}
