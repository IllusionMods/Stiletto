namespace Stiletto.Models
{
    public abstract class TextSerializer
    {
        public TextSerializer() { }

        public TextSerializer(string value) 
        {
            Deserialize(value);
        }

        public abstract void Deserialize(string value);

        public abstract string Serialize();
    }
}
