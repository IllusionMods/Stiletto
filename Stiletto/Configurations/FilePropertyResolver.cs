using Stiletto.Attributes;
using StrayTech;
using System.Collections.Generic;
using System.Reflection;

namespace Stiletto.Configurations
{
    public class FilePropertyResolver<T>
    {
        private Dictionary<string, PropertyInfo> _properties = new Dictionary<string, PropertyInfo>();

        public FilePropertyResolver()
        {
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                var fileProperty = property.GetCustomAttribute<FilePropertyAttribute>();
                if (fileProperty != null)
                {
                    var name = string.IsNullOrEmpty(fileProperty.Name) ? property.Name : fileProperty.Name;
                    _properties[name] = property;
                }
            }
        }

        public PropertyInfo GetProperty(string name)
        {
            if (_properties.ContainsKey(name))
                return _properties[name];
            return null;
        }

        public IDictionary<string, PropertyInfo> PropertyMap => _properties;
    }
}
