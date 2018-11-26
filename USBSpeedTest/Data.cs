using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Data;

namespace USBSpeedTest
{
    class Data
    {
        public static int SCid = 0;
        public static int OCid = 2;
        public static int DARid = 3;
        public static int OnlyId = 0;

        public static bool SaveSC1 = true;
        public static bool SaveSC2= true;
        public static bool SaveSC3 = true;
        public static bool SaveSC4 = true;

        public static List<byte> ADList01 = new List<byte>();//1D06
        public static List<byte> ADList02 = new List<byte>();//1D07
        public static DataTable dt_AD01 = new DataTable();
        public static DataTable dt_AD02 = new DataTable();
        public static double[] daRe_AD01 = new double[8] { 0,0,0,0,0,0,0,0};
        public static double[] daRe_AD02 = new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        public static bool AdFrmIsAlive = false;

        public static double[] value_a = new double[128];                         //DA修正参数a
        public static double[] value_b = new double[128];                         //DA修正参数b

        public static int[] value_R1 = new int[96];                         //R修正参数a
        public static int[] value_R2 = new int[96];                         //R修正参数b

        public static Byte[] LastOC = new byte[768];            //记录上一次OC的值，与此次的OC值比较，发生变化的才改变界面，减少界面消耗

     //   public static object synObj_dq0 = new object();
        public static object synObj_sc1 = new object();



        public static Queue<byte[]> DataQueue1 = new Queue<byte[]>();   //For FrameEPDU deal with 同步下行1
        public static Queue<byte[]> DataQueue2 = new Queue<byte[]>();   //For FrameEPDU deal with 同步下行2
        public static Queue<byte[]> DataQueue3 = new Queue<byte[]>();   //For FrameEPDU deal with 串口下行
        public static int Channel = 1;                                      //表明从哪里调用的FrameEPDU

        public static Dictionary<string, BinaryWriter> myDictionary1 = new Dictionary<string, BinaryWriter>();
        public static Dictionary<string, BinaryWriter> myDictionary2 = new Dictionary<string, BinaryWriter>();
        public static Dictionary<string, BinaryWriter> myDictionary3 = new Dictionary<string, BinaryWriter>();

        public static List<string> APIDList = new List<string>();
        public static List<string> APIDList2 = new List<string>();
        public static List<string> APIDList3 = new List<string>();

        public static Dictionary<string, Queue<byte[]>> Apid_EPDU_Dictionary = new Dictionary<string, Queue<byte[]>>();
        public static Dictionary<string, Queue<byte[]>> Apid_EPDU_Dictionary2 = new Dictionary<string, Queue<byte[]>>();
        public static Dictionary<string, Queue<byte[]>> Apid_EPDU_Dictionary3 = new Dictionary<string, Queue<byte[]>>();

        public static List<string> AlreadyOnApid = new List<string>();
        public static List<string> AlreadyOnApid2 = new List<string>();
        public static List<string> AlreadyOnApid3 = new List<string>();

        public static bool TBChan1Used = false;
        public static bool TBChan2Used = false;
        public static bool MOXAChanUsed = false;

        public static int SCRecvCounts1 = 0;
        public static int SCRecvCounts2 = 0;
        public static void init(int channel)
        {
            switch (channel) {
                case 1:
                    AlreadyOnApid.Clear();
                    APIDList.Clear();

                    foreach (var item in Apid_EPDU_Dictionary)
                    {
                        item.Value.Clear();
                    }
                    Apid_EPDU_Dictionary.Clear();
                    break;
                case 2:
                    AlreadyOnApid2.Clear();
                    APIDList2.Clear();

                    foreach (var item in Apid_EPDU_Dictionary2)
                    {
                        item.Value.Clear();
                    }
                    Apid_EPDU_Dictionary2.Clear();
                    break;
                case 3:
                    AlreadyOnApid3.Clear();
                    APIDList3.Clear();

                    foreach (var item in Apid_EPDU_Dictionary3)
                    {
                        item.Value.Clear();
                    }
                    Apid_EPDU_Dictionary3.Clear();
                    break;
            }

        }


        public static string DAconfigPath = Program.GetStartupPath() + @"配置文件\DAconfig.xml";
        public static string RconfigPath = Program.GetStartupPath() + @"配置文件\Rconfig.xml";
        public static string OCconfigPath = Program.GetStartupPath() + @"配置文件\OCconfig.xml";

        public static void SaveConfig(string Path,string key ,string value)
        {
            XDocument xDoc = XDocument.Load(Path);
            XmlReader reader = xDoc.CreateReader();

            bool Matched = false;//是否已在XML中

            foreach (var p in xDoc.Root.Elements("add"))
            {
                if (p.Attribute("key").Value == key)
                {
                    p.Attribute("value").Value = value;
                    Matched = true;
                }
            }
            if(Matched == false)
            {
                XElement element = new XElement("add", new XAttribute("key", key), new XAttribute("value", value));
                xDoc.Root.Add(element);
            }

            xDoc.Save(Path);
            //var query = from p in xDoc.Root.Elements("add")
            //            where p.Attribute("key").Value == "DAModifyA1"
            //            orderby p.Value
            //            select p.Value;

            //foreach (string s in query)
            //{
            //    Console.WriteLine(s);
            //}

        }

        public static string GetConfig(string Path,string key)
        {
            XDocument xDoc = XDocument.Load(Path);
            XmlReader reader = xDoc.CreateReader();
            string value = "Error";

            var query = from p in xDoc.Root.Elements("add")
                        where p.Attribute("key").Value == key
                        select p.Attribute("value").Value;

            foreach(string s in query)
            {
                value = s;
            }

            //foreach (var p in xDoc.Root.Elements("add"))
            //{
            //    if (p.Attribute("key").Value == key)
            //    {
            //        value = p.Attribute("value").Value;
            //    }
            //}
            return value;

        }

        public static string GetConfigStr(string Path, string key, string name)
        {
            XDocument xDoc = XDocument.Load(Path);
            XmlReader reader = xDoc.CreateReader();
            string value = "Error";
            var query = from p in xDoc.Root.Elements("add")
                        where p.Attribute("key").Value == key
                        select p.Attribute(name).Value;

            foreach (string s in query)
            {
                value = s;
            }

            return value;
        }

        public static void SaveConfigStr(string Path, string key, string name ,string value)
        {
            XDocument xDoc = XDocument.Load(Path);
            XmlReader reader = xDoc.CreateReader();

            bool Matched = false;//是否已在XML中

            foreach (var p in xDoc.Root.Elements("add"))
            {
                if (p.Attribute("key").Value == key)
                {
                    p.Attribute(name).Value = value;
                    Matched = true;
                }
            }
            if (Matched == false)
            {
                XElement element = new XElement("add", new XAttribute("key", key), new XAttribute("name", value));
                xDoc.Root.Add(element);
            }

            xDoc.Save(Path);
            //var query = from p in xDoc.Root.Elements("add")
            //            where p.Attribute("key").Value == "DAModifyA1"
            //            orderby p.Value
            //            select p.Value;

            //foreach (string s in query)
            //{
            //    Console.WriteLine(s);
            //}

        }



    }
}
