﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using GOLD;

namespace FSO.Client.UI.Framework.Parser
{
    public class UIGroup : UINode
    {
        public UINode SharedProperties { get; set; }
        public List<UINode> Children { get; set; }


		public static UIGroup FromReduction(Reduction r, Dictionary<Token, object> dataMap)
        {
            UIGroup result = new UIGroup();
			// <Object> ::= BeginLiteral <Content> EndLiteral
            var content = (List<UINode>)dataMap[r[1]];
            var sharedProps = content.FirstOrDefault(x => x.Name == "SetSharedProperties");

            result.SharedProperties = sharedProps;
            result.Children = content.Where(x => x != sharedProps).ToList();

            return result;
        }
    }

    public class UISharedProperties
    {
        public static UISharedProperties FromReduction(Reduction r)
        {
            UISharedProperties result = new UISharedProperties();
            return result;
        }
    }

    public class UINode
    {
        public string Name { get; set; }
        public string ID { get; set; }

        public Dictionary<string, string> Attributes { get; internal set; }

        public UINode()
        {
            Attributes = new Dictionary<string, string>();
        }

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

        public Color GetColor(string name)
        {
            var att = Attributes[name];
            if (att != null)
            {
                return UIScript.ParseRGB(att);
            }
            return default(Color);
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
