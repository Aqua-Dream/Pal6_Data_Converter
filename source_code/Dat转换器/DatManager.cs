// DataManager.cs
// This class reads a data file and can convet it to a XML element.

using System;
using System.Collections.Generic;
using System.Linq;

using System.IO;
using System.Xml.Linq;

namespace Dat转换器
{
    public class DatManager
    {
        private BinaryReader dat = null;

        private XElement Config = null;

        private XElement root = null;

        public DatManager(string path)
        {
            LoadConfig(path);
            FileStream f = File.OpenRead(path);
            dat = new BinaryReader(f);
        }

        private void LoadConfig(string path)
        {
            string filename = Path.GetFileName(path);
            root = new XElement(filename);
            XDocument xml = XDocument.Load( "Config.xml");
            foreach(XElement e in xml.Root.Elements())
            {
                if (filename.Equals(e.Name.ToString(), StringComparison.OrdinalIgnoreCase)) 
                {
                    Config = e;
                    return;
                }
            }
            throw new Exception("找不到此文件的相关配置。");
        }

        private string ReadOne(string type)
        {
            type=type.ToUpper();
            switch(type)
            {
                case "INT":
                    int n = dat.ReadInt32();
                    if (n == -1) return "None";
                    else return n.ToString();
                case "UINT":
                    return dat.ReadUInt32().ToString();
                case "LONG":
                    return dat.ReadInt64().ToString();
                case "ULONG":
                    return dat.ReadUInt64().ToString();
                case "SHORT":
                    return dat.ReadInt16().ToString();
                case "USHORT":
                    return dat.ReadUInt16().ToString();
                case "BYTE":
                    return dat.ReadByte().ToString();
                case "BOOL":
                    return dat.ReadBoolean().ToString();
                case "FLOAT":
                    return dat.ReadSingle().ToString();
                case "DOUBLE":
                    return dat.ReadDouble().ToString();
                case "STRING":
                    return dat.ReadString().ToString();
                case "LANGUE":
                    string s = dat.ReadUInt64().ToString();
                    return s + "/" + dat.ReadString();
                case "ID":
                    uint a = dat.ReadUInt32();
                    if (a == 0xFFFFFFFF) return "None";
                    return (a >> 24).ToString() +"/"+ (a & 0xFFFFFFu).ToString();
                default:
                    throw new Exception("不识别的数据类型。");
            }
        }

        public void ReadAnElement(XElement config, XElement dest, int attrCounts=-1)
        {
            List<XAttribute> attrs = config.Attributes().ToList();
            List<XElement> subEles = config.Elements().ToList();
   
            int starNum = 0;
            if (attrCounts == -1) attrCounts = attrs.Count;
            for (int i = 0; i < attrCounts; i++)
            {
                XAttribute attr = attrs[i];
                string value = attr.Value;
                if (value.IndexOf(',') == -1)
                {
                    dest.Add(new XAttribute(attr.Name, ReadOne(value)));
                }
                else
                {
                    string[] ss = value.Split(',');
                    int n;
                    try
                    {
                        n = Convert.ToInt32(ss[0]);
                    }
                    catch
                    {
                        string s = ReadOne(ss[0]);
                        dest.Add(new XAttribute(attr.Name, s));
                        n = Convert.ToInt32(s);
                    }
                    if (ss[1] == "*")
                    {
                        for (int j = 0; j < n; j++)
                        {
                            XElement newDest = new XElement(subEles[starNum].Name);
                            XElement newConfig = new XElement(subEles[starNum].Name);
                            int k = subEles[starNum].Attributes().Count();
                            if (ss.Length == 3)
                            {
                                try
                                {
                                    k = Convert.ToInt32(ss[2]);
                                }
                                catch
                                {
                                    string s = ReadOne(ss[2]);
                                    dest.Add(new XAttribute(attr.Name, s));
                                    k = Convert.ToInt32(s);
                                }

                            }
                            ReadAnElement(subEles[starNum], newDest, k);	//Recurse
                            dest.Add(newDest);
                        }
                        starNum++;
                    }
                    else
                    {
                        int k = Convert.ToInt32(ss[1]);
                        for (int j = 0; j < n; j++)
                        {
                            for (int l = 0; l < k; l++)
                            {
                                i++;
                                dest.Add(new XAttribute(attrs[i].Name.ToString() + (j + 1).ToString(), ReadOne(attrs[i].Value)));
                            }
                            i -= k;
                        }
                        i += k;
                    }
                }
            }
            
        }

        public XElement Read()
        {
            int num = 0;
            while (dat.BaseStream.Position < dat.BaseStream.Length)
            {
                num++;
                XElement row = new XElement("数据"+num.ToString());
                ReadAnElement(Config, row);
                root.Add(row);
            }
            return root;
        }
    }
}
