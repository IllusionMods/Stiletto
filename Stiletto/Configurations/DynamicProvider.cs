using Stiletto.Models;
using System.IO;

namespace Stiletto.Configurations
{
    public class DynamicProvider<T> where T : class, new()
    {
        private ConcurrentDictionary<string, DynamicFileProvider<T>> _fileProviders = new ConcurrentDictionary<string, DynamicFileProvider<T>>();
        private readonly FilePropertyResolver<T> _filePropertyResolver = new FilePropertyResolver<T>();
        private string _rootDirectory;

        public DynamicProvider(string rootDirectory)
        {
            _rootDirectory = rootDirectory;
        }

        public virtual void Save(string name, T item)
        {
            GetFileProvider(name)?.Save(item);
        }

        public virtual T Load(string name)
        {
            return GetFileProvider(name)?.Value ?? new T();
        }

        public virtual void Reload()
        {
            _fileProviders.Clear();
        }

        private DynamicFileProvider<T> GetFileProvider(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            if (!_fileProviders.ContainsKey(name))
                _fileProviders[name] = new DynamicFileProvider<T>(GetFilePath(name), _filePropertyResolver);
            return _fileProviders[name];
        }

        protected virtual string GetFilePath(string name)
        {
            return Path.Combine(_rootDirectory, $"{name}.txt");
        }
    }
}
