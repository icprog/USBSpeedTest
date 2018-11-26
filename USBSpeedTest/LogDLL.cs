using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace USBSpeedTest
{    class MyLog
    {
        public static RichTextBox richTextBox1;             //绑定主界面上的RichTextBox
        public static string path;                          //Log文件的目录
        public static int lines;                            //指定行数清空RichTextBox并创建新日志

        public static LogDLL Log = new LogDLL();

        public static void start()
        {
            Log.richTextBox1 = richTextBox1;
            Log.path = path;
            Log.lines = lines;
        }

        public static void Info(string info)
        {
            Log.Info(info);
        }

        public static void Error(string error)
        {
            Log.Error(error);
        }

        public static void Warning(string Warning)
        {
            Log.Error(Warning);
        }

        internal static void Info(object p)
        {
            throw new NotImplementedException();
        }
    }
    class LogDLL
    {
        public RichTextBox richTextBox1;
        public string path;
        public int lines;
        private string sDaytime;
        private int count = 0;
        private string RecordFilename;

        public delegate void LogAppendDelegate(Color color, string text);

        public void LogAppendMethod(Color color, string text)
        {
            if (!richTextBox1.ReadOnly)
                richTextBox1.ReadOnly = true;

            richTextBox1.SelectionColor = color;
            richTextBox1.AppendText(text);
            richTextBox1.AppendText("\n");


            string LogPath = path;
            if (!Directory.Exists(LogPath))
                Directory.CreateDirectory(LogPath);
            //如果软件记录到第二天，重新生成操作日志文件
            if (sDaytime != string.Format("{0}-{1:D2}-{2:D2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))
            {
                count = 0;
            }

            sDaytime = string.Format("{0}-{1:D2}-{2:D2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            RecordFilename = sDaytime + count.ToString() + "Log.txt";

            LogPath = LogPath + RecordFilename;


            File.AppendAllText(LogPath, text + "\r\n");
            FileInfo fi = new FileInfo(LogPath);
            if (fi.Length >= 1024 * 1024 * 60)
            {
                count++;
                sDaytime = string.Format("{0}-{1:D2}-{2:D2} Log", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                RecordFilename = sDaytime + count.ToString() + ".txt";
            }

            if (richTextBox1.Lines.Count() > lines)
            {
                richTextBox1.Clear();
            }
        }

        public void Error(string text)
        {
            LogAppendDelegate la = new LogAppendDelegate(LogAppendMethod);
            richTextBox1.Invoke(la, Color.Red, DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss] ") + "[ERROR]" + text);
        }
        public void Warning(string text)
        {
            LogAppendDelegate la = new LogAppendDelegate(LogAppendMethod);
            richTextBox1.Invoke(la, Color.Violet, DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss] ") + "[WARN]" + text);
        }
        public void Info(string text)
        {
            LogAppendDelegate la = new LogAppendDelegate(LogAppendMethod);
            richTextBox1.Invoke(la, Color.Black, DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss] ") + "[INFO]" + text);

        }

    }

}
