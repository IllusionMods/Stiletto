using System;

namespace Stiletto.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FilePropertyAttribute: Attribute
    {
        public FilePropertyAttribute() {}

        public FilePropertyAttribute(string name) 
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
