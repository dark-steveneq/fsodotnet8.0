using System;

namespace FSO.Client.UI.Framework.Parser
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UIAttribute : Attribute
    {
        public string Name { get; set; }
        public Type Parser { get; set; }
        public UIAttributeType DataType = UIAttributeType.Unknown;

        public UIAttribute(string name)
        {
            this.Name = name;
        }

        public UIAttribute(string name, Type parser)
        {
            this.Name = name;
            this.Parser = parser;
        }
    }

    public enum UIAttributeType
    {
        Point,
        Texture,
        Vector2,
        Unknown,
        StringTable,
        String,
        Integer,
        Boolean,
        Color
    }

    public interface UIAttributeParser
    {
        void ParseAttribute(UINode node);
    }

}