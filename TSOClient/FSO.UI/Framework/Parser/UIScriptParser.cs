using System;
using System.Collections.Generic;
using System.IO;

namespace FSO.Client.UI.Framework.Parser
{
    public class UIScriptParser
    {
        public List<UINode> program = [];
        
        public bool Parse(StreamReader reader)
        {
            string contents = reader.ReadToEnd().ReplaceLineEndings();
            parseBlock(contents, null);
            return true;
        }

        private int parseBlock(string block, UIGroup parent)
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
                        Dictionary<string, string> arguments = getArguments(data[1..]);
                        switch (data[0])
                        {
                        case "Begin":
                            var group = new UIGroup();
                            i += parseBlock(block[i..], group);
                            if (parent == null)
                                program.Add(group);
                            else
                                parent.Children.Add(group);
                            break;
                        case "End":
                            return i;
                        case "SetSharedProperties":
                            parent.AddAtts(arguments);
                            break;
                        default:
                            var node = new UINode
                            {
                                Name = data[0],
                                //ID = Guid.NewGuid().ToString()
                                //ID = arguments.ContainsKey("id") ? arguments["id"] : ""
                                ID = ""
                            };
                            //if (arguments.ContainsKey("id"))
                            //    node.ID = arguments["id"];
                            if (arguments.ContainsKey("Nameless"))
                                node.ID = arguments["Nameless"];
                            node.AddAtts(parent.Attributes);
                            node.AddAtts(arguments);
                            if (parent == null)
                                program.Add(node);
                            else
                                parent.Children.Add(node);
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
                else if (output[^1].Length > 0 && (data[i] == ' ' || data[i] == '\t') || data[i] == '\n')
                    output = [.. output, ""];
            }
            return output;
        }

        private Dictionary<string, string> getArguments(string[] data)
        {
            Dictionary<string, string> args = [];
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
                    args[key] = value;
                else if (key != "" && value != "")
                    args.Add(key, value);
            }
            return args;
        }
    }
}