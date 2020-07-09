using System.Collections.Generic;
using System.Xml.Serialization;

namespace Stiletto
{
    [XmlRoot("Stiletto")]
    public class XMLContainer
    {
        public XMLContainer()
        {
        }

        public XMLContainer(int id, float angleAnkle, float angleLeg, float height)
        {
            ShoeConfig = new List<ShoeConfig> { new ShoeConfig { Id = id, AngleAnkle = angleAnkle, AngleLeg = angleLeg, Height = height } };
        }

        [XmlElement]
        public List<ShoeConfig> ShoeConfig;
    }

    public class ShoeConfig
    {
        [XmlAttribute]
        public int Id;

        [XmlAttribute]
        public float AngleAnkle;

        [XmlAttribute]
        public float AngleLeg;

        [XmlAttribute]
        public float Height;
    }
}
