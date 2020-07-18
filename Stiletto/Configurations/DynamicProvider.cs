using Stiletto.Attributes;
using Stiletto.Models;
using StrayTech;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Stiletto.Configurations
{
    public class DynamicProvider<T> where T : new()
    {
        private ConcurrentDictionary<string, T> _cache = new ConcurrentDictionary<string, T>();
        private Dictionary<string, PropertyInfo> _properties = new Dictionary<string, PropertyInfo>();
        private string _rootDirectory;

        private static object _lock = new object();

        public DynamicProvider(string rootDirectory)
        {
            _rootDirectory = rootDirectory;

            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                var propertyName = property.GetCustomAttribute<FilePropertyAttribute>();
                if (propertyName != null)
                {
                    _properties[propertyName.Name] = property;
                }
            }
        }

        public virtual void Save(string name, T item)
        {
            var lines = _properties
                .Select(property => $"{property.Key}={property.Value.GetValue(item, null)}")
                .ToArray();

            File.WriteAllLines(GetFilePath(name), lines);
            _cache.Remove(name);
        }

        public virtual T Load(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new T();
            }

            if (_cache.ContainsKey(name))
            {
                return _cache[name];
            }

            var filePath = GetFilePath(name);

            lock (_lock)
            {
                var item = new T();

                if (File.Exists(filePath))
                {
                    item = FormatItem(File.ReadAllLines(filePath));
                }
                _cache[name] = item;
                return item;
            }
        }

        public virtual void Reload()
        {
            _cache.Clear();
        }

        protected virtual T FormatItem(string[] lines)
        {
            var item = new T();

            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length > 1)
                {
                    var propertyName = parts[0].Trim();
                    if (_properties.ContainsKey(propertyName))
                    {
                        var property = _properties[propertyName];
                        try
                        {
                            var value = TryConvertValue(parts[1].Trim(), property.PropertyType);
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

        protected virtual string GetFilePath(string name)
        {
            return Path.Combine(_rootDirectory, $"{name}.txt");
        }

        private object TryConvertValue(string stringValue, Type type)
        {
            if (type == typeof(string))
            {
                return stringValue;
            }

            var nullableType = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (nullableType)
            {
                if (string.IsNullOrEmpty(stringValue))
                {
                    return GetDefault(type);
                }
                type = new NullableConverter(type).UnderlyingType;
            }

            Type[] argTypes = { typeof(string), type.MakeByRefType() };
            var tryParseMethodInfo = type.GetMethod("TryParse", argTypes);
            if (tryParseMethodInfo == null)
            {
                return GetDefault(type);
            }

            object[] args = { stringValue, GetDefault(type) };
            tryParseMethodInfo.Invoke(null, args);
            
            return args[1];
        }

        private object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
