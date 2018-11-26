using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;
using System.Windows.Forms;
using System.Configuration;
using Microsoft.Win32;
using System.Diagnostics;
using System.Timers;

namespace USBSpeedTest
{
    class SaveFile
    {
        public static ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_Async = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_1 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_2 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_3 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_4 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_5 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_6 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_7 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_8 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_9 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_10 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_11 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_12 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_13 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_14 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_15 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_16 = new ReaderWriterLockSlim();
        public static List<ReaderWriterLockSlim> myLock = new List<ReaderWriterLockSlim>();

        public static ReaderWriterLockSlim Lock_asyn_1 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_asyn_2 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_asyn_3 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_asyn_4 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_asyn_5 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_asyn_6 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_asyn_7 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_asyn_8 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_asyn_9 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_asyn_10 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_asyn_11 = new ReaderWriterLockSlim();
        public static ReaderWriterLockSlim Lock_asyn_12 = new ReaderWriterLockSlim();


        public static List<ReaderWriterLockSlim> myLockforAsync = new List<ReaderWriterLockSlim>();
        //在网络收发线程中给DataQueue赋值

        public static Queue<byte[]> DataQueue_0 = new Queue<byte[]>();
        public static Queue<byte[]> DataQueue_SC1 = new Queue<byte[]>();
        public static Queue<byte[]> DataQueue_SC2 = new Queue<byte[]>();
        public static Queue<byte[]> DataQueue_SC3 = new Queue<byte[]>();
        public static Queue<byte[]> DataQueue_SC4 = new Queue<byte[]>();
        public static Queue<byte[]> DataQueue_SC5 = new Queue<byte[]>();
        public static Queue<byte[]> DataQueue_SC6 = new Queue<byte[]>();
        public static Queue<byte[]> DataQueue_SC7 = new Queue<byte[]>();
        public static Queue<byte[]> DataQueue_SC8 = new Queue<byte[]>();
        public static Queue<byte[]> DataQueue_SC9 = new Queue<byte[]>();
        public static Queue<byte[]> DataQueue_SC10 = new Queue<byte[]>();
        public static Queue<byte[]> DataQueue_SC11 = new Queue<byte[]>();
        public static Queue<byte[]> DataQueue_SC12 = new Queue<byte[]>();
        public static Queue<byte[]> DataQueue_SC13 = new Queue<byte[]>();
        public static Queue<byte[]> DataQueue_SC14 = new Queue<byte[]>();
        public static Queue<byte[]> DataQueue_SC15 = new Queue<byte[]>();
        public static Queue<byte[]> DataQueue_SC16 = new Queue<byte[]>();

        public static Queue<byte[]> DataQueue_Async = new Queue<byte[]>();

        public static List<Queue<byte[]>> DataQueueList = new List<Queue<byte[]>>();

        public static bool SaveOn = true;

        FileStream file0, file_async;
        FileStream file_SC1, file_SC2, file_SC3, file_SC4,
            file_SC5, file_SC6, file_SC7, file_SC8,
            file_SC9, file_SC10, file_SC11, file_SC12,
            file_SC13, file_SC14, file_SC15, file_SC16;
        List<FileStream> myFileList_dat = new List<FileStream>();

        FileStream file_asyn1, file_asyn2, file_asyn3, file_asyn4, file_asyn5, file_asyn6,
            file_asyn7, file_asyn8, file_asyn9, file_asyn10, file_asyn11, file_asyn12;
        List<FileStream> myFileList_txt = new List<FileStream>();

        public static Queue<string> DataQueue_asyn_1 = new Queue<string>();
        public static Queue<string> DataQueue_asyn_2 = new Queue<string>();
        public static Queue<string> DataQueue_asyn_3 = new Queue<string>();
        public static Queue<string> DataQueue_asyn_4 = new Queue<string>();
        public static Queue<string> DataQueue_asyn_5 = new Queue<string>();
        public static Queue<string> DataQueue_asyn_6 = new Queue<string>();
        public static Queue<string> DataQueue_asyn_7 = new Queue<string>();
        public static Queue<string> DataQueue_asyn_8 = new Queue<string>();
        public static Queue<string> DataQueue_asyn_9 = new Queue<string>();
        public static Queue<string> DataQueue_asyn_10 = new Queue<string>();
        public static Queue<string> DataQueue_asyn_GNC1 = new Queue<string>();
        public static Queue<string> DataQueue_asyn_GNC2 = new Queue<string>();
        public static List<Queue<string>> DataQueue_asynList = new List<Queue<string>>();
        //在主页面初始化时，调用FileInit()和FileSaveStart(),分别初始化文件夹和开始存储

        /// <summary>
        /// 存储线程初始化
        /// </summary>
        public void FileInit()
        {
            Trace.WriteLine("Start FileInit!\n");

            string MaBen_path = Program.GetStartupPath() + @"码本\";
            if (!Directory.Exists(MaBen_path))
                Directory.CreateDirectory(MaBen_path);

            string Save_path = Program.GetStartupPath() + @"SaveData\";
       //    FileCreateDat(Program.GetStartupPath() + @"源码\", out file0);

            FileCreateDat(Program.GetStartupPath() + @"单元测试仪数据\源码\", out file_SC1);

            FileCreateDat(Program.GetStartupPath() + @"单元测试仪数据\FF08\", out file_SC2);
            FileCreateDat(Program.GetStartupPath() + @"单元测试仪数据\FF01\", out file_SC3);
            FileCreateDat(Program.GetStartupPath() + @"单元测试仪数据\FF02\", out file_SC4);
            FileCreateDat(Program.GetStartupPath() + @"单元测试仪数据\FF03\", out file_SC5);
            FileCreateDat(Program.GetStartupPath() + @"单元测试仪数据\FF04\", out file_SC6);

            FileCreateDat(Program.GetStartupPath() + @"单元测试仪数据\FF08\1D00(指令回令)\", out file_SC7);
            FileCreateDat(Program.GetStartupPath() + @"单元测试仪数据\FF08\1D01(被动遥测RS422)\", out file_SC8);
            FileCreateDat(Program.GetStartupPath() + @"单元测试仪数据\FF08\1D02(KA模拟器返回)\", out file_SC9);
            FileCreateDat(Program.GetStartupPath() + @"单元测试仪数据\FF08\1D03(KD模拟器返回)\", out file_SC10);
            FileCreateDat(Program.GetStartupPath() + @"单元测试仪数据\FF08\1D04(12所模拟器)\", out file_SC11);
            FileCreateDat(Program.GetStartupPath() + @"单元测试仪数据\FF08\1D05(电源遥测)\", out file_SC12);
            FileCreateDat(Program.GetStartupPath() + @"单元测试仪数据\FF08\1D06(AD通道0-7)\", out file_SC13);
            FileCreateDat(Program.GetStartupPath() + @"单元测试仪数据\FF08\1D07(AD通道8-15)\", out file_SC14);

            //FileCreateDat(Program.GetStartupPath() + @"数传机箱数据\同步422数据\通道2\", out file_SC11);
            //FileCreateDat(Program.GetStartupPath() + @"数传机箱数据\FF08\", out file_SC12);
            //FileCreateDat(Program.GetStartupPath() + @"数传数据\通道13\", out file_SC13);
            //FileCreateDat(Program.GetStartupPath() + @"数传数据\通道14\", out file_SC14);
            //FileCreateDat(Program.GetStartupPath() + @"数传数据\通道15\", out file_SC15);
            //FileCreateDat(Program.GetStartupPath() + @"数传数据\通道16\", out file_SC16);

            //FileCreateDat(Program.GetStartupPath() + @"异步422\", out file_async);

            DataQueueList.Add(DataQueue_SC1);
            DataQueueList.Add(DataQueue_SC2);
            DataQueueList.Add(DataQueue_SC3);
            DataQueueList.Add(DataQueue_SC4);
            DataQueueList.Add(DataQueue_SC5);
            DataQueueList.Add(DataQueue_SC6);
            DataQueueList.Add(DataQueue_SC7);
            DataQueueList.Add(DataQueue_SC8);
            DataQueueList.Add(DataQueue_SC9);
            DataQueueList.Add(DataQueue_SC10);
            DataQueueList.Add(DataQueue_SC11);
            DataQueueList.Add(DataQueue_SC12);
            DataQueueList.Add(DataQueue_SC13);
            DataQueueList.Add(DataQueue_SC14);
            DataQueueList.Add(DataQueue_SC15);
            DataQueueList.Add(DataQueue_SC16);


            //FileCreateTxt(Program.GetStartupPath() + @"数传机箱数据\异步422接收\通道1\", out file_asyn1);
            //FileCreateTxt(Program.GetStartupPath() + @"数传机箱数据\异步422接收\通道2\", out file_asyn2);
            //FileCreateTxt(Program.GetStartupPath() + @"数传机箱数据\异步422接收\通道3\", out file_asyn3);
            //FileCreateTxt(Program.GetStartupPath() + @"数传机箱数据\异步422接收\通道4\", out file_asyn4);
            //FileCreateTxt(Program.GetStartupPath() + @"数传机箱数据\异步422接收\通道5\", out file_asyn5);
            //FileCreateTxt(Program.GetStartupPath() + @"数传机箱数据\异步422接收\通道6\", out file_asyn6);
            //FileCreateTxt(Program.GetStartupPath() + @"数传机箱数据\异步422接收\通道7\", out file_asyn7);
            //FileCreateTxt(Program.GetStartupPath() + @"数传机箱数据\异步422接收\通道8\", out file_asyn8);
            //FileCreateTxt(Program.GetStartupPath() + @"数传机箱数据\异步422接收\通道9\", out file_asyn9);
            //FileCreateTxt(Program.GetStartupPath() + @"数传机箱数据\异步422接收\通道10\", out file_asyn10);
            //FileCreateTxt(Program.GetStartupPath() + @"数传机箱数据\异步422接收\通道GNC1\", out file_asyn11);
            //FileCreateTxt(Program.GetStartupPath() + @"数传机箱数据\异步422接收\通道GNC2\", out file_asyn12);

            DataQueue_asynList.Add(DataQueue_asyn_1);
            DataQueue_asynList.Add(DataQueue_asyn_2);
            DataQueue_asynList.Add(DataQueue_asyn_3);
            DataQueue_asynList.Add(DataQueue_asyn_4);
            DataQueue_asynList.Add(DataQueue_asyn_5);
            DataQueue_asynList.Add(DataQueue_asyn_6);
            DataQueue_asynList.Add(DataQueue_asyn_7);
            DataQueue_asynList.Add(DataQueue_asyn_8);
            DataQueue_asynList.Add(DataQueue_asyn_9);
            DataQueue_asynList.Add(DataQueue_asyn_10);
            DataQueue_asynList.Add(DataQueue_asyn_GNC1);
            DataQueue_asynList.Add(DataQueue_asyn_GNC2);

            myLock.Add(Lock_1);
            myLock.Add(Lock_2);
            myLock.Add(Lock_3);
            myLock.Add(Lock_4);
            myLock.Add(Lock_5);
            myLock.Add(Lock_6);
            myLock.Add(Lock_7);
            myLock.Add(Lock_8);
            myLock.Add(Lock_9);
            myLock.Add(Lock_10);
            myLock.Add(Lock_11);
            myLock.Add(Lock_12);
            myLock.Add(Lock_13);
            myLock.Add(Lock_14);
            myLock.Add(Lock_15);
            myLock.Add(Lock_16);

            //          WriteFileThread = new Thread(WriteToFile);
            //           WriteFileThread.Start(file1);
        }

        public void FileCreateDat(string Path, out FileStream file)
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            DirectoryInfo folder = new DirectoryInfo(Path);
            try
            {
                foreach (FileInfo tempfile in folder.GetFiles("*.*"))
                {
                    string name = tempfile.Name;
                    if (tempfile.Length == 0)
                    {
                        Trace.WriteLine("删除文件" + tempfile.FullName);
                        File.Delete(tempfile.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                MyLog.Error(ex.Message);
            }


            string timestr = string.Format("{0}-{1:D2}-{2:D2} {3:D2}：{4:D2}：{5:D2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            string filename = Path + timestr + ".dat";
            file = new FileStream(filename, FileMode.Create);
            myFileList_dat.Add(file);
        }

        public void FileCreateTxt(string Path, out FileStream file)
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            string timestr = string.Format("{0}-{1:D2}-{2:D2} {3:D2}：{4:D2}：{5:D2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            string filename = Path + timestr + ".txt";
            file = new FileStream(filename, FileMode.Create);
            myFileList_txt.Add(file);
        }

        public void FileSaveStart()
        {
            //       MyLog.Info("开启存盘线程！");
            Trace.WriteLine("开启存盘线程");
            SaveOn = true;

           // new Thread(() => { WriteToFileSC(0, file0, ref DataQueue_0, ref Lock); }).Start();
            new Thread(() => { WriteToFileSC(0, file_SC1, ref DataQueue_SC1, ref Lock_1); }).Start();
            new Thread(() => { WriteToFileSC(1, file_SC2, ref DataQueue_SC2, ref Lock_2); }).Start();
            new Thread(() => { WriteToFileSC(3, file_SC3, ref DataQueue_SC3, ref Lock_3); }).Start();
            new Thread(() => { WriteToFileSC(4, file_SC4, ref DataQueue_SC4, ref Lock_4); }).Start();
            new Thread(() => { WriteToFileSC(5, file_SC5, ref DataQueue_SC5, ref Lock_5); }).Start();
            new Thread(() => { WriteToFileSC(6, file_SC6, ref DataQueue_SC6, ref Lock_6); }).Start();
            new Thread(() => { WriteToFileSC(7, file_SC7, ref DataQueue_SC7, ref Lock_7); }).Start();
            new Thread(() => { WriteToFileSC(8, file_SC8, ref DataQueue_SC8, ref Lock_8); }).Start();
            new Thread(() => { WriteToFileSC(9, file_SC9, ref DataQueue_SC9, ref Lock_9); }).Start();
            new Thread(() => { WriteToFileSC(10, file_SC10, ref DataQueue_SC10, ref Lock_10); }).Start();
            new Thread(() => { WriteToFileSC(11, file_SC11, ref DataQueue_SC11, ref Lock_11); }).Start();
            new Thread(() => { WriteToFileSC(12, file_SC12, ref DataQueue_SC12, ref Lock_12); }).Start();
            new Thread(() => { WriteToFileSC(13, file_SC13, ref DataQueue_SC13, ref Lock_13); }).Start();
            new Thread(() => { WriteToFileSC(14, file_SC14, ref DataQueue_SC14, ref Lock_14); }).Start();
            //new Thread(() => { WriteToFileSC(15, file_SC15, ref DataQueue_SC15, ref Lock_15); }).Start();
            //new Thread(() => { WriteToFileSC(16, file_SC16, ref DataQueue_SC16, ref Lock_16); }).Start();

            //new Thread(() => { WriteToFileSC(17, file_async, ref DataQueue_Async, ref Lock_Async); }).Start();

            //new Thread(() => { WriteToFileAsynC(1, file_asyn1, ref DataQueue_asyn_1, ref Lock_asyn_1); }).Start();
            //new Thread(() => { WriteToFileAsynC(2, file_asyn2, ref DataQueue_asyn_2, ref Lock_asyn_2); }).Start();
            //new Thread(() => { WriteToFileAsynC(3, file_asyn3, ref DataQueue_asyn_3, ref Lock_asyn_3); }).Start();
            //new Thread(() => { WriteToFileAsynC(4, file_asyn4, ref DataQueue_asyn_4, ref Lock_asyn_4); }).Start();
            //new Thread(() => { WriteToFileAsynC(5, file_asyn5, ref DataQueue_asyn_5, ref Lock_asyn_5); }).Start();
            //new Thread(() => { WriteToFileAsynC(6, file_asyn6, ref DataQueue_asyn_6, ref Lock_asyn_6); }).Start();
            //new Thread(() => { WriteToFileAsynC(7, file_asyn7, ref DataQueue_asyn_7, ref Lock_asyn_7); }).Start();
            //new Thread(() => { WriteToFileAsynC(8, file_asyn8, ref DataQueue_asyn_8, ref Lock_asyn_8); }).Start();
            //new Thread(() => { WriteToFileAsynC(9, file_asyn9, ref DataQueue_asyn_9, ref Lock_asyn_9); }).Start();
            //new Thread(() => { WriteToFileAsynC(10, file_asyn10, ref DataQueue_asyn_10, ref Lock_asyn_10); }).Start();
            //new Thread(() => { WriteToFileAsynC(11, file_asyn11, ref DataQueue_asyn_GNC1, ref Lock_asyn_11); }).Start();
            //new Thread(() => { WriteToFileAsynC(12, file_asyn12, ref DataQueue_asyn_GNC2, ref Lock_asyn_12); }).Start();
        }

        public void FileClose()
        {
            SaveOn = false;
            Trace.WriteLine("Start FileClose!\n");
            Thread.Sleep(500);
            //           WriteFileThread.Abort();
            foreach (var item in myFileList_dat)
            {
                item.Close();
            }
            foreach (var item in myFileList_txt)
            {
                item.Close();
            }
        }


        private void WriteToFileSC(int key, object file, ref Queue<byte[]> myQueue, ref ReaderWriterLockSlim myLock)
        {
            Trace.WriteLine("Start WriteToFileSC Thread:" + key.ToString());
            FileStream myfile = (FileStream)file;
            BinaryWriter bw = new BinaryWriter(myfile);
       //     FileInfo fileInfo;

            while (SaveOn)
            {
                if (myQueue.Count() > 0)
                {
                    try
                    {
                        myLock.EnterReadLock();
                        bw.Write(myQueue.Dequeue());
                        bw.Flush();
                        myLock.ExitReadLock();

                        #region 分割文件，防止文件过大
                        long FileSizeMB = myfile.Length / (1024 * 1024 * 1024);
                        if (FileSizeMB > 1)
                        {
                            myFileList_dat[key].Flush();

                            string Path2 = myFileList_dat[key].Name;
                            int count = Path2.LastIndexOf("\\");
                            Path2 = Path2.Substring(0, count + 1);

                            myFileList_dat[key].Close();

                            FileStream newFile;
                            string timestr = string.Format("{0}-{1:D2}-{2:D2} {3:D2}：{4:D2}：{5:D2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                            string filename = Path2 + timestr + ".dat";
                            newFile = new FileStream(filename, FileMode.Create);

                            myFileList_dat.Remove(myFileList_dat[key]);
                            myFileList_dat.Insert(key, newFile);

                            break;
                            //break跳出循环会执行新线程
                        }
                        #endregion
                    }
                    catch (Exception e)
                    {
                        bw.Close();
                        Trace.WriteLine(myQueue.Count());
                        MyLog.Error(e.Message);
                        break;
                    }
                }
                else
                {
                    Thread.Sleep(200);
                    //                  Trace.WriteLine("Queue0 is empty!!");
                }
            }
            bw.Close();
            Trace.WriteLine("Leaving WriteToFileSC:" + key.ToString());

            if (SaveOn)
            {
                WriteToFileSC(key, myFileList_dat[key], ref myQueue, ref myLock);
            }
        }


        private void WriteToFileAsynC(int key, object file,ref Queue<string> myQueue,ref ReaderWriterLockSlim myLock)
        {
            Trace.WriteLine("Start WriteToFileAsync Thread:" + key.ToString());
            FileStream myfile = (FileStream)file;
            StreamWriter bw = new StreamWriter(myfile);
      //      FileInfo fileInfo;
            while (SaveOn)
            {
                if (myQueue.Count() > 0)
                {
                    try
                    {
                        myLock.EnterReadLock();
                        bw.Write(myQueue.Dequeue());
                        bw.Flush();
                        myLock.ExitReadLock();

                        #region 分割文件，防止文件过大
                        long FileSizeMB = myfile.Length / (1024 * 1024);
                        if (FileSizeMB > 10)
                        {
                            myFileList_txt[key].Flush();

                            string Path2 = myFileList_txt[key].Name;
                            int count = Path2.LastIndexOf("\\");
                            Path2 = Path2.Substring(0, count + 1);

                            myFileList_txt[key].Close();

                            FileStream newFile;
                            string timestr = string.Format("{0}-{1:D2}-{2:D2} {3:D2}：{4:D2}：{5:D2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                            string filename = Path2 + timestr + ".txt";
                            newFile = new FileStream(filename, FileMode.Create);

                            myFileList_txt.Remove(myFileList_txt[key]);
                            myFileList_txt.Insert(key, newFile);

                            break;
                            //break跳出循环会执行新线程
                        }
                        #endregion
                    }
                    catch (Exception e)
                    {
                        bw.Close();
                        Trace.WriteLine(myQueue.Count());
                        Trace.WriteLine(e.Message);
                        break;
                    }
                }
                else
                {
                    Thread.Sleep(200);
                    //                  Trace.WriteLine("Queue0 is empty!!");
                }
            }
            bw.Close();
            Trace.WriteLine("Leaving WriteToFileAsync:" + key.ToString());

            if (SaveOn) WriteToFileAsynC(key, myFileList_txt[key],ref myQueue,ref myLock);

        }
    }
}
