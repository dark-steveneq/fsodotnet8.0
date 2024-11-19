using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FSO.Client.UI.Framework.Parser
{
    public class UIScript
    {
        private static GraphicsDevice gd;
        private static UIContainer parent;

        public UIScript(GraphicsDevice grd, UIContainer par)
        {
            gd = grd;
            parent = par;
        }

        public UIScript(GraphicsDevice grd, UIContainer par, string path)
        {
            gd = grd;
            parent = par;
            Parse(path);
        }

        public void Parse(string path)
        {
            if (!File.Exists(path))
                throw new Exception("File doesn't exist");

            StreamReader file = File.OpenText(path);
            string contents = file.ReadToEnd().ReplaceLineEndings();
            file.Close();
            parseBlock(contents, []);
        }

        public static Color ParseRGB(string rgb)
        {
            if (rgb.StartsWith("("))
            {
                rgb = rgb.Substring(1);
            }
            if (rgb.EndsWith(")"))
            {
                rgb = rgb.Substring(0, rgb.Length - 1);
            }

            var split = rgb.Split(new char[] { ',' });
            return new Color(
                byte.Parse(split[0]),
                byte.Parse(split[1]),
                byte.Parse(split[2])
            );
            
        }

        private int parseBlock(string block, Dictionary<string, Argument> parent)
        {
            int i = 0;
            for (i = 0; block.Length > i; i++)
            {
                if (block[i] == '<')
                {
                    string start = block[(i + 1)..];
                    int ii = start.IndexOf(">");
                    if (ii != -1)
                    {
                        i += ii;
                        start = start[..ii];
                        string[] data = split(start);
                        Dictionary<string, Argument> arguments = getArguments(data[1..], parent);
                        switch (data[0])
                        {
                        case "Begin":
                            i += parseBlock(block[i..], arguments);
                            break;
                        case "End":
                            return i;
                        default:
                            break;
                        }
                    }
                }
            }
            return i;
        }
        
        private string[] split(string data)
        {
            string[] output = [""];
            bool isString = false;
            for (int i = 0; data.Length > i; i++)
            {
                if (data[i] == '=')
                {
                    if (output[^1].Length > 0)
                        output = [.. output, ""];
                    output[^1] = "=";
                    output = [.. output, ""];
                }
                else if (data[i] == '"')
                    isString = !isString;
                else if (isString || (data[i] != '\n' && data[i] != '\t' && data[i] != ' '))
                    output[^1] += data[i];
                else if (output[^1].Length > 0 && data[i] == ' ' || data[i] == '\n')
                    output = [.. output, ""];
            }
            return output;
        }

        private Dictionary<string, Argument> getArguments(string[] data, Dictionary<string, Argument> parent)
        {
            Dictionary<string, Argument> args = parent.ToDictionary(entry => entry.Key, entry => entry.Value);
            for (int i = 0; data.Length > i; i++)
            {
                string key   = "";
                string value = "";

                if (data[i].Length > 0 && data[i].Contains("=") && data[i] != "=")
                {
                    int ii = data[i].IndexOf('=');
                    key = data[i][..ii];
                    value = data[i][(ii + 1)..];
                }
                else if (data[i].Length > 0 && data.Length - i >= 2 && data[i+1] == "=")
                {
                    key   = data[i];
                    value = data[i + 2];
                    i += 2;
                }
                else if (data[i].Length > 0)
                {
                    key   = "Nameless";
                    value = data[i];
                }
                for (int ii = 0; key.Length > ii; i++)
                    if (key[ii] != ' ')
                    {
                        key = key[ii..];
                        break;
                    }
                if (key != "" && value != "" && args.ContainsKey(key))
                    args[key] = new(value);
                else if (key != "" && value != "")
                    args.Add(key, new(value));
            }
            return args;
        }
    }

    public class Argument
    {
        private string  _valStr = "";
        private int     _valInt = 0;
        private Vector2 _valPos = new();
        private Color   _valCol = Color.Black;

        public string  String => _valStr;
        public int     Int    => _valInt;
        public Vector2 Pos    => _valPos;
        public Color   Color  => _valCol;

        public Argument(string value)
        {
            _valStr = value;
            int.TryParse(value.Replace("\n", "").Replace("\t", ""), out _valInt);
            _valCol = UIScript.ParseRGB(value);
            // TODO: Pos and color
        }
    }
}
