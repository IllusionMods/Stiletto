using Stiletto.Models;
using System.IO;

namespace Stiletto.Configurations
{
    public class CustomPoseProvider
    {
        private ConcurrentDictionary<string, AnimationSettingsCollection<CustomPose>> _settings;

        private readonly string _rootDirectory;

        private static object locker = new object();

        public CustomPoseProvider(string rootDirectory)
        {
            _settings = new ConcurrentDictionary<string, AnimationSettingsCollection<CustomPose>>();
            _rootDirectory = rootDirectory;
            Directory.CreateDirectory(_rootDirectory);
            Reload();
        }

        public CustomPose Load(string path, string name)
        {
            return GetSettingsCollection(path)?.Load(path, name) ?? new CustomPose();
        }

        public void Save(string path, string name, CustomPose pose)
        {
            lock (locker)
            {
                GetSettingsCollection(path)?.Save(path, name, pose);
            }
        }

        public void Reload()
        {
            lock (locker)
            {
                _settings.Clear();
                var files = Directory.GetFiles(_rootDirectory, "*.txt", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    _settings[Path.GetFileNameWithoutExtension(file)] = new AnimationSettingsCollection<CustomPose>(file);
                }
            }
        }

        private AnimationSettingsCollection<CustomPose> GetSettingsCollection(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (!_settings.ContainsKey(path))
            {
                _settings[path] = new AnimationSettingsCollection<CustomPose>(Path.Combine(_rootDirectory, $"{path}.txt"));
            }

            return _settings[path];
        }
    }
}
