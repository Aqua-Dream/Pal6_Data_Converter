// Program.cs
// Entry point of this program.

using System;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;

namespace Dat转换器
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Dat转换器                设计者：风靡义磊";
            Environment.CurrentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            if (args.Length == 0)
            {
                Console.WriteLine("请拖动dat或xml格式文件到本程序！");
            }
            else
            {
                foreach (string path in args)
                {
                    try
                    {
                        Console.WriteLine("正在处理文件：{0}", path);
                        string destFile=path;
                        string filename = Path.GetFileName(path);
                        bool IsXml=ReadConfig(ref destFile);
                        if(IsXml)
                        {
                            XmlManager xml = new XmlManager(path);
                            destFile = xml.Write(destFile);
                        }
                        else
                        {
                            if (filename.IndexOf(".dat") != -1) filename = filename.Remove(filename.IndexOf(".dat"), 4);
                            filename += ".xml";
                            destFile = Path.Combine(destFile, filename);
                            DatManager dat = new DatManager(path);
                            XElement xml = dat.Read();
                            xml.Save(destFile);
                        }
                        Console.WriteLine("已保存到：{0}", destFile);
                        Console.WriteLine();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("发生错误！{0}", e.Message);
                        Console.WriteLine();
                    }
                }
            }
            Console.WriteLine("按任意键退出..");
            Console.ReadKey();
        }

        static bool ReadConfig(ref string destFile)
        {
            XDocument x = XDocument.Load("Config.xml");
            string sourcePath = Path.GetDirectoryName(destFile);
            string filename = Path.GetFileName(destFile);
            bool IsXml = filename.IndexOf(".xml", StringComparison.OrdinalIgnoreCase) != -1;
            
            foreach (XElement e in x.Root.Elements())
            {
                if (e.Name.ToString().Equals("Config", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (XAttribute attr in e.Attributes())
                    {
                        if(IsXml)
                        {
                            if(attr.Name.ToString().IndexOf("dat",StringComparison.OrdinalIgnoreCase)!=-1)
                            {
                                destFile= attr.Value.Replace("源文件目录", sourcePath);
                                break;
                            }
                        }
                        else
                        {
                            if (attr.Name.ToString().IndexOf("xml", StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                destFile = attr.Value.Replace("源文件目录", sourcePath);
                                break;
                            }
                        }
                    }
                    break;
                }
            }

            if (!Directory.Exists(destFile)) Directory.CreateDirectory(destFile);
            return IsXml;
        }
    }
}