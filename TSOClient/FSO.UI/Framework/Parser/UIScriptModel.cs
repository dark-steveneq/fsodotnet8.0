using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FSO.Client.UI.Framework.Parser
{
    public class UIGroup : UINode
    {
        public UINode SharedProperties { get; set; }
        public List<UINode> Children { get; set; }

        public UIGroup()
        {
            Children = [];
        }
    }

    public class UISharedProperties
    {
    }

    /// <summary>
    /// Class representing parsed UI elements
    /// </summary>
    public class UINode
    {
        public string Name { get; set; }
        public string ID { get; set; }

        /// <summary>
        /// Dictionary of attributes
        /// </summary>
        public Dictionary<string, string> Attributes { get; internal set; }

        public UINode()
        {
            Attributes = new Dictionary<string, string>();
        }

        /// <summary>
        /// Get Vector2 from attribute
        /// </summary>
        /// <param name="name">Attribute name</param>
        /// <returns>Parsed vector, zeroed if not found</returns>
        public Vector2 GetVector2(string name)
        {
            var att = Attributes[name];
            if (att != null)
            {
                /** Remove ( ) **/
                att = att.Substring(1, att.Length - 2);
                var parts = att.Split(new char[] { ',' });

                return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
            }
            return Vector2.Zero;
        }

        /// <summary>
        /// Get Color from attribute
        /// </summary>
        /// <param name="name">Attribute name</param>
        /// <returns>Parsed color, black if not found</returns>
        public Color GetColor(string name)
        {
            var att = Attributes[name];
            if (att != null)
            {
                return UIScript.ParseRGB(att);
            }
            return Color.Black;
        }

        public Point GetPoint(string name)
        {
            var att = Attributes[name];
            if (att != null)
            {
                /** Remove ( ) **/
                att = att.Substring(1, att.Length - 2);
                var parts = att.Split(new char[] { ',' });

                return new Point(int.Parse(parts[0]), int.Parse(parts[1]));
            }
            return Point.Zero;
        }

        public void AddAtts(Dictionary<string, string> attributes)
        {
            foreach (var att in attributes)
            {
                if (!Attributes.ContainsKey(att.Key))
                {
                    this[att.Key] = att.Value;
                }
            }
        }

        public string this[string name]
        {
            get
            {
                string result = null;
                Attributes.TryGetValue(name, out result);
                return result;
            }
            set
            {
                Attributes[name] = value;
            }
        }
    }
}