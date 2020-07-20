using System;
using System.IO;
using System.Linq;

namespace Stiletto.Configurations
{
    public class DynamicFileProvider<T> where T : class, new()
    {
        private readonly string _filePath;
        private readonly FilePropertyResolver<T> _filePropertyResolver;

        private object _lock = new object();

        public DynamicFileProvider(string filePath) : this(filePath, new FilePropertyResolver<T>()) { }

        public DynamicFileProvider(string filePath, FilePropertyResolver<T> filePropertyResolver)
        {
            _filePath = filePath;
            _filePropertyResolver = filePropertyResolver;

            Value = Read();
        }

        public void Save(T item)
        {
            var lines = _filePropertyResolver.PropertyMap
                .Select(property => $"{property.Key}={property.Value.GetValue(item, null)}")
                .ToArray();

            lock (_lock)
            {
                Value = item;
                File.WriteAllLines(_filePath, lines);
            }
        }

        public void Reload()
        {
            Value = Read();
        }

        public T Value { get; private set; }

        private T Read()
        {
            var item = new T();

            lock (_lock)
            {
                if (!File.Exists(_filePath))
                {
                    return item;
                }

                var lines = File.ReadAllLines(_filePath);

                foreach (var line in lines)
                {
                    var parts = line.Split('=');
                    if (parts.Length > 1)
                    {
                        var propertyName = parts[0].Trim();
                        var property = _filePropertyResolver.GetProperty(propertyName);

                        if (property != null)
                        {
                            try
                            {
                                var value = StringHelper.Convert(parts[1].Trim(), property.PropertyType);
                                property.SetValue(item, value, null);
                            }
                            catch
                            {
                                Console.WriteLine($"{property.Name}, {property.PropertyType.Name}");
                            }
                        }
                    }
                }

                return item;
            }
        }
    }
}
