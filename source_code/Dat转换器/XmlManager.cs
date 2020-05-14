// XmlManager.cs
// This class reads a XML file and can convet it to a data file.

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.IO;

namespace Dat转换器
{
    class XmlManager
    {
        private XElement xml = null;

        private BinaryWriter dat = null;

        private XElement config = null;

        private void LoadConfig(string path)
        {
            string filename = Path.GetFileName(path);
            XDocument x = XDocument.Load("Config.xml");

            foreach (XElement e in x.Root.Elements())
            {
                if (e.Name.ToString().Equals(xml.Name.ToString(),StringComparison.OrdinalIgnoreCase))
                {
                    config = e;
                    return;
                }
            }
        }

        public XmlManager(string path)
        {
            xml = XElement.Load(path);
            LoadConfig(path);
        }

        private void WriteOne(string type,string data)
        {
            type = type.ToUpper();
            switch (type)
            {
                case "INT":
                    if (data == "None") dat.Write(-1);
                    else dat.Write(Convert.ToInt32(data));
                    break;
                case "UINT":
                    dat.Write(Convert.ToUInt32(data));
                    break;
                case "LONG":
                    dat.Write(Convert.ToInt64(data));
                    break;
                case "ULONG":
                    dat.Write(Convert.ToUInt64(data));
                    break;
                case "SHORT":
                    dat.Write(Convert.ToInt16(data));
                    break;
                case "USHORT":
                    dat.Write(Convert.ToUInt16(data));
                    break;
                case "BYTE":
                    dat.Write(Convert.ToByte(data));
                    break;
                case "BOOL":
                    dat.Write(Convert.ToBoolean(data));
                    break;
                case "FLOAT":
                    dat.Write(Convert.ToSingle(data));
                    break;
                case "DOUBLE":
                    dat.Write(Convert.ToDouble(data));
                    break;
                case "STRING":
                    dat.Write(Convert.ToString(data));
                    break;
                case "LANGUE":
                    string[] ss = data.Split('/');
                    dat.Write(Convert.ToUInt64(ss[0]));
                    dat.Write(ss[1]);
                    break;
                case "ID":
                    if (data == "None") dat.Write(-1);
                    else
                    {
                        string[] sss = data.Split('/');
                        uint a = (Convert.ToUInt32(sss[0]) << 24)| (Convert.ToUInt32(sss[1]));
                        dat.Write(a);
                    }
                    break;
                default:
                    throw new Exception(type+"是不识别的数据类型。");
            }
        }

        public void WriteAnElement(XElement config, XElement dest, int attrCounts = -1)
        {
            List<XAttribute> configAttrs = config.Attributes().ToList();
            List<XElement> configSubEles = config.Elements().ToList();
            List<XAttribute> destAttrs = dest.Attributes().ToList();
            List<XElement> destSubEles = dest.Elements().ToList();

            int starNum = 0;
            int eleNum = 0;
            if (attrCounts == -1) attrCounts = configAttrs.Count;
            for (int i = 0, i2 = 0; i < attrCounts ; i++)
            {
                string type = configAttrs[i].Value;
                if (type.IndexOf(',') == -1)
                {
                    WriteOne(type, destAttrs[i2].Value);
                    i2++;
                }
                else
                {
                    bool flag = false;
                    string[] ss = type.Split(',');
                    int n;
                    try
                    {
                        n = Convert.ToInt32(ss[0]);
                    }
                    catch
                    {
                        WriteOne(ss[0], destAttrs[i2].Value);
                        n = Convert.ToInt32(destAttrs[i2].Value);
                        i2++;
                    }
                    if (ss[1] == "*")
                    {
                        XElement newConfig = configSubEles[starNum];
                        int temp = -1;
                        if (ss.Length == 3)
                        {
                            try
                            {
                                temp = Convert.ToInt32(ss[2]);
                                flag = false;
                            }
                            catch
                            {
                                flag = true;
                                temp = Convert.ToInt32(destAttrs[i2].Value);
                                i2++;
                            }
                        }
                        for (int j = 0; j < n; j++)
                        {
                            XElement newDest = destSubEles[eleNum];
                            if (flag) WriteOne(ss[2], temp.ToString());
                            WriteAnElement(newConfig, newDest, temp);		//Recurse
                            eleNum++;
                        }
                        starNum++;
                    }
                    else
                    {
                        int k = Convert.ToInt32(ss[1]);        //Loop k times
                        for (int j = 0; j < n; j++)
                        {
                            for (int l = 0; l < k; l++)
                            {
                                i++;
                                WriteOne(configAttrs[i].Value, destAttrs[i2].Value);
                                i2++;
                            }
                            i -= k;
                        }
                        i += k;
                    }
                }
            }
        }

        public string Write(string destFile)
        {
            string filename = Path.Combine(destFile, xml.Name.ToString());
            FileStream f = File.Create(filename);
            dat = new BinaryWriter(f);
            foreach (XElement ele in xml.Elements())
            {
                WriteAnElement(config, ele);
            }
            dat.Close();
            f.Close();
            return filename;
        }
    }
}
