using CyUSB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace USBSpeedTest
{
    public partial class MainForm : Form
    {
        public RegisterForm myRegisterForm;
        public ADForm myADForm;
        SaveFile FileThread = null;
        public byte[] TempStoreBuf = new byte[8192];
        public int TempStoreBufTag = 0;

        public DateTime startDT;
        public DateTime endDT;
        public int RecvdMB = 0;

        public static Queue<byte> DataQueue_1D0E = new Queue<byte>();   //处理FF08异步数传通道的数据
        public static ReaderWriterLockSlim Lock_1D0E = new ReaderWriterLockSlim();

        public static Queue<byte> DataQueue_1D0F = new Queue<byte>();   //处理FF08异步数传通道的数据
        public static ReaderWriterLockSlim Lock_1D0F = new ReaderWriterLockSlim();

        int ThisCount = 0;
        int LastCount = 0;

        public MainForm()
        {
            InitializeComponent();

            //启动日志
            MyLog.richTextBox1 = richTextBox1;
            MyLog.path = Program.GetStartupPath() + @"LogData\";
            MyLog.lines = 50;
            MyLog.start();


            // Create the list of USB devices attached to the CyUSB3.sys driver.
            USB.usbDevices = new USBDeviceList(CyConst.DEVICES_CYUSB);

            //Assign event handlers for device attachment and device removal.
            USB.usbDevices.DeviceAttached += new EventHandler(UsbDevices_DeviceAttached);
            USB.usbDevices.DeviceRemoved += new EventHandler(UsbDevices_DeviceRemoved);

            USB.Init();
        }

        void UsbDevices_DeviceAttached(object sender, EventArgs e)
        {
            SetDevice(false);
        }

        /*Summary
        This is the event handler for device removal. This method resets the device count and searches for the device with VID-PID 04b4-1003
        */
        void UsbDevices_DeviceRemoved(object sender, EventArgs e)
        {
            USBEventArgs evt = (USBEventArgs)e;
            USBDevice RemovedDevice = evt.Device;

            string RemovedDeviceName = evt.FriendlyName;
            MyLog.Error(RemovedDeviceName + "板卡断开");

            int key = int.Parse(evt.ProductID.ToString("x4").Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            USB.MyDeviceList[key] = null;

        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            SetDevice(false);
            myRegisterForm = new RegisterForm();


            Data.dt_AD01.Columns.Add("序号", typeof(Int32));
            Data.dt_AD01.Columns.Add("名称", typeof(String));
            Data.dt_AD01.Columns.Add("测量值", typeof(double));
            for (int i = 0; i < 8; i++)
            {
                DataRow dr = Data.dt_AD01.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = "通道" + (i + 1).ToString();
                dr["测量值"] = 0;
                Data.dt_AD01.Rows.Add(dr);
            }


            Data.dt_AD02.Columns.Add("序号", typeof(Int32));
            Data.dt_AD02.Columns.Add("名称", typeof(String));
            Data.dt_AD02.Columns.Add("测量值", typeof(double));
            for (int i = 8; i < 16; i++)
            {
                DataRow dr = Data.dt_AD02.NewRow();
                dr["序号"] = i + 1;
                dr["名称"] = "通道" + (i + 1).ToString();
                dr["测量值"] = 0;
                Data.dt_AD02.Rows.Add(dr);
            }

            myADForm = new ADForm();
        }

        /*Summary
Search the device with VID-PID 04b4-00F1 and if found, select the end point
*/
        private void SetDevice(bool bPreserveSelectedDevice)
        {
            int nDeviceList = USB.usbDevices.Count;
            for (int nCount = 0; nCount < nDeviceList; nCount++)
            {
                USBDevice fxDevice = USB.usbDevices[nCount];
                String strmsg;
                strmsg = "(0x" + fxDevice.VendorID.ToString("X4") + " - 0x" + fxDevice.ProductID.ToString("X4") + ") " + fxDevice.FriendlyName;

                int key = int.Parse(fxDevice.ProductID.ToString("x4").Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                if (USB.MyDeviceList[key] == null)
                {
                    USB.MyDeviceList[key] = (CyUSBDevice)fxDevice;

                    MyLog.Info(USB.MyDeviceList[key].FriendlyName + ConfigurationManager.AppSettings[USB.MyDeviceList[key].FriendlyName] + "连接");


                    USB.SendCMD(0, 0x81, 0x7f);
                    Thread.Sleep(100);
                    USB.SendCMD(0, 0x81, 0x00);
                    Thread.Sleep(100);
                    USB.SendCMD(0, 0x82, 0x7f);
                    Thread.Sleep(100);
                    USB.SendCMD(0, 0x82, 0x00);
                    Thread.Sleep(100);
                    USB.SendCMD(0, 0x83, 0x7f);
                    Thread.Sleep(100);
                    USB.SendCMD(0, 0x83, 0x00);
                    Thread.Sleep(100);
                    USB.SendCMD(0, 0x84, 0x7f);
                    Thread.Sleep(100);
                    USB.SendCMD(0, 0x84, 0x00);
                    Thread.Sleep(100);
                    USB.SendCMD(0, 0x85, 0x7f);
                    Thread.Sleep(100);
                    USB.SendCMD(0, 0x85, 0x00);
                    Thread.Sleep(100);

                }
            }

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        bool RecvTag = false;
        private void button1_Click(object sender, EventArgs e)
        {
            if (this.button1.Text == "Start")
            {
                this.textBox1.Text = null;
                this.textBox2.Text = null;
                this.textBox3.Text = null;

                this.button1.Text = "Stop";

                for (int i = 0; i < 9; i++)
                {
                    if (USB.MyDeviceList[i] != null)
                    {

                        CyControlEndPoint CtrlEndPt = null;
                        CtrlEndPt = USB.MyDeviceList[i].ControlEndPt;

                        if (CtrlEndPt != null)
                        {
                            USB.SendCMD(i, 0x80, 0x01);
                            USB.SendCMD(i, 0x80, 0x00);

                            USB.MyDeviceList[i].Reset();

                            Register.Byte80H = (byte)(Register.Byte80H | 0x04);
                            USB.SendCMD(i, 0x80, Register.Byte80H);

                            this.btn_80_2.Text = "1";

                        }
                    }
                }

                FileThread = new SaveFile();
                FileThread.FileInit();
                FileThread.FileSaveStart();

                MyLog.Info("开始读取");
                RecvTag = true;

                ThisCount = 0;
                LastCount = 0;

                new Thread(() => { RecvAllUSB(); }).Start();
                new Thread(() => { DealWithADFun(); }).Start();

            }
            else
            {
                this.button1.Text = "Start";
                ThisCount = 0;
                LastCount = 0;
                RecvTag = false;
                Thread.Sleep(500);
                if (FileThread != null)
                    FileThread.FileClose();
            }
        }

        int Recv4KCounts = 0;
        private void RecvAllUSB()
        {
            CyUSBDevice MyDevice01 = USB.MyDeviceList[Data.SCid];

            startDT = DateTime.Now;
            DateTime midDT = startDT;
            RecvdMB = 0;
            TempStoreBufTag = 0;
            while (RecvTag)
            {
                if (MyDevice01.BulkInEndPt != null)
                {
                    byte[] buf = new byte[4096];
                    int buflen = 4096;

                    MyDevice01.BulkInEndPt.XferData(ref buf, ref buflen);

                    if (buflen > 0)
                    {
                        Trace.WriteLine("收到数据包长度为：" + buflen.ToString());
                        Array.Copy(buf, 0, TempStoreBuf, TempStoreBufTag, buflen);
                        TempStoreBufTag += buflen;

                        byte[] Svbuf = new byte[buflen];
                        Array.Copy(buf, Svbuf, buflen);

                        SaveFile.Lock_1.EnterWriteLock();
                        SaveFile.DataQueue_SC1.Enqueue(Svbuf);
                        SaveFile.Lock_1.ExitWriteLock();

                        while (TempStoreBufTag >= 4096)
                        {
                            if (TempStoreBuf[0] == 0xff && (0x0 <= TempStoreBuf[1]) && (TempStoreBuf[1] < 0x11))
                            {
                                DealWithLongFrame(ref TempStoreBuf, ref TempStoreBufTag);
                            }
                            else
                            {
                                MyLog.Error("数传422机箱 收到异常帧！");
                                Trace.WriteLine("收到异常帧" + TempStoreBufTag.ToString());
                                Array.Clear(TempStoreBuf, 0, TempStoreBufTag);
                                TempStoreBufTag = 0;
                            }
                        }
                    }
                    else if (buflen == 0)
                    {
                        //Trace.WriteLine("数传422机箱 收到0包-----0000000000");
                    }
                    else
                    {
                        Trace.WriteLine("数传422机箱 收到buflen <0");
                    }

                    endDT = DateTime.Now;
                    double tempTime = endDT.Subtract(midDT).TotalSeconds;
                    if (tempTime > 2)
                    {
                        midDT = endDT;
                        double tempMB = Recv4KCounts / 256;
                        Recv4KCounts = 0;
                        this.textBox4.BeginInvoke(new Action(() =>
                        {
                            double speed = tempMB / tempTime;
                            textBox4.Text = speed.ToString();
                            this.progressBar1.Value = (int)speed;
                        }));
                    }
                }
            }
            endDT = DateTime.Now;

            this.textBox1.BeginInvoke(
                new Action(() =>
                {
                    double costTime = endDT.Subtract(startDT).TotalSeconds;
                    double RecvdM = RecvdMB / 1024;
                    textBox1.Text = costTime.ToString();
                    textBox2.Text = RecvdM.ToString();
                    textBox3.Text = (RecvdM / costTime).ToString();
                }));

        }

        void DealWithLongFrame(ref byte[] TempBuf, ref int TempTag)
        {
            ThisCount = TempStoreBuf[2] * 256 + TempStoreBuf[3];
            if (LastCount != 0 && ThisCount != 0 && (ThisCount - LastCount != 1))
            {
                MyLog.Error("出现漏帧情况！！");
                Trace.WriteLine("出现漏帧情况:" + LastCount.ToString("x4") + "--" + ThisCount.ToString("x4"));
            }
            LastCount = ThisCount;

            byte[] buf_LongFrame = new byte[4096];
            Array.Copy(TempStoreBuf, 0, buf_LongFrame, 0, 4096);

            Array.Copy(TempStoreBuf, 4096, TempStoreBuf, 0, TempStoreBufTag - 4096);
            TempStoreBufTag -= 4096;

            RecvdMB += 4;
            Recv4KCounts += 1;
            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x01)
            {
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                SaveFile.Lock_3.EnterWriteLock();
                SaveFile.DataQueue_SC3.Enqueue(bufsav);
                SaveFile.Lock_3.ExitWriteLock();
            }
            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x02)
            {
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                SaveFile.Lock_4.EnterWriteLock();
                SaveFile.DataQueue_SC4.Enqueue(bufsav);
                SaveFile.Lock_4.ExitWriteLock();
            }
            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x03)
            {
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                SaveFile.Lock_5.EnterWriteLock();
                SaveFile.DataQueue_SC5.Enqueue(bufsav);
                SaveFile.Lock_5.ExitWriteLock();
            }
            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x04)
            {
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                SaveFile.Lock_6.EnterWriteLock();
                SaveFile.DataQueue_SC6.Enqueue(bufsav);
                SaveFile.Lock_6.ExitWriteLock();
            }

            if (buf_LongFrame[0] == 0xff && buf_LongFrame[1] == 0x08)
            {
                //FF08为短帧通道
                byte[] bufsav = new byte[4092];
                Array.Copy(buf_LongFrame, 4, bufsav, 0, 4092);
                SaveFile.Lock_2.EnterWriteLock();
                SaveFile.DataQueue_SC2.Enqueue(bufsav);
                SaveFile.Lock_2.ExitWriteLock();

                for (int i = 0; i < 6; i++)
                {
                    if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x00)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_7.EnterWriteLock();
                        SaveFile.DataQueue_SC7.Enqueue(buf1D0x);
                        SaveFile.Lock_7.ExitWriteLock();
                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x01)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_8.EnterWriteLock();
                        SaveFile.DataQueue_SC8.Enqueue(buf1D0x);
                        SaveFile.Lock_8.ExitWriteLock();
                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x02)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_9.EnterWriteLock();
                        SaveFile.DataQueue_SC9.Enqueue(buf1D0x);
                        SaveFile.Lock_9.ExitWriteLock();
                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x03)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_10.EnterWriteLock();
                        SaveFile.DataQueue_SC10.Enqueue(buf1D0x);
                        SaveFile.Lock_10.ExitWriteLock();
                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x04)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_11.EnterWriteLock();
                        SaveFile.DataQueue_SC11.Enqueue(buf1D0x);
                        SaveFile.Lock_11.ExitWriteLock();
                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x05)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_12.EnterWriteLock();
                        SaveFile.DataQueue_SC12.Enqueue(buf1D0x);
                        SaveFile.Lock_12.ExitWriteLock();
                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x06)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_13.EnterWriteLock();
                        SaveFile.DataQueue_SC13.Enqueue(buf1D0x);
                        SaveFile.Lock_13.ExitWriteLock();

                        lock (Data.ADList01)
                        {
                            for (int j = 0; j < num; j++)
                                Data.ADList01.Add(buf1D0x[j]);
                        }
                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x07)
                    {
                        int num = bufsav[i * 682 + 2] * 256 + bufsav[i * 682 + 3];//有效位
                        byte[] buf1D0x = new byte[num];
                        Array.Copy(bufsav, i * 682 + 4, buf1D0x, 0, num);
                        SaveFile.Lock_14.EnterWriteLock();
                        SaveFile.DataQueue_SC14.Enqueue(buf1D0x);
                        SaveFile.Lock_14.ExitWriteLock();
                        lock (Data.ADList02)
                        {
                            for (int j = 0; j < num; j++)
                                Data.ADList02.Add(buf1D0x[j]);
                        }

                    }
                    else if (bufsav[i * 682 + 0] == 0x1D && bufsav[i * 682 + 1] == 0x0f)
                    {
                        //空闲帧
                    }
                    else
                    {
                        Trace.WriteLine("FF08通道出错!");
                    }
                }
            }
        }

        private void DealWithADFun()
        {
            while (RecvTag)
            {
                bool Tag1 = false;
                bool Tag2 = false;

                lock (Data.ADList01)
                {

                    if (Data.ADList01.Count > 16)
                    {
                        Tag1 = true;

                        byte[] buf = new byte[16];
                        for (int t = 0; t < 16; t++)
                        {
                            buf[t] = Data.ADList01[t];
                        }
                        for (int k = 0; k < 8; k++)
                        {
                            int temp = (buf[2 * k] & 0x7f) * 256 + buf[2 * k + 1];

                            if ((buf[2 * k] & 0x80) == 0x80)
                            {
                                temp = 0x8000 - temp;
                            }
                            double value = temp;
                            value = 10 * (value / 32767);
                            if ((buf[2 * k] & 0x80) == 0x80)
                                Data.daRe_AD01[k] = -value;
                            else
                                Data.daRe_AD01[k] = value;
                        }
                        Data.ADList01.RemoveRange(0, 16);

                    }
                    else
                    {
                        Tag1 = false;        
                    }

                }


                lock (Data.ADList02)
                {

                    if (Data.ADList02.Count > 16)
                    {
                        Tag2 = true;
                        byte[] buf = new byte[16];
                        for (int t = 0; t < 16; t++)
                        {
                            buf[t] = Data.ADList02[t];
                        }
                        for (int k = 0; k < 8; k++)
                        {
                            int temp = (buf[2 * k] & 0x7f) * 256 + buf[2 * k + 1];

                            if ((buf[2 * k] & 0x80) == 0x80)
                            {
                                temp = 0x8000 - temp;
                            }

                            double value = temp;
                            value = 10 * (value / 32767);
                            if ((buf[2 * k] & 0x80) == 0x80)
                                Data.daRe_AD02[k] = -value;
                            else
                                Data.daRe_AD02[k] = value;
                        }
                        Data.ADList02.RemoveRange(0, 16);
                    }
                    else
                    {
                        Tag2 = false;
                    }
                }

                if (Tag1 == false && Tag2 == false)
                {
                    Thread.Sleep(500);
                }


            }
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            RecvTag = false;

            if (USB.usbDevices != null)
            {
                USB.usbDevices.DeviceRemoved -= UsbDevices_DeviceRemoved;
                USB.usbDevices.DeviceAttached -= UsbDevices_DeviceAttached;
                USB.usbDevices.Dispose();
            }


            Thread.Sleep(200);
            if (FileThread != null)
                FileThread.FileClose();

            this.Dispose();



        }

        private void button2_Click(object sender, EventArgs e)
        {
            Register.Byte81H = (byte)(Register.Byte81H | 0x01);
            USB.SendCMD(0, 0x81, Register.Byte81H);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            USB.SendCMD(0, 0x81, 0x00);

            Register.Byte80H = (byte)(Register.Byte80H | 0x01);
            USB.SendCMD(0, 0x80, Register.Byte80H);

            Register.Byte80H = (byte)(Register.Byte80H & 0xfe);
            USB.SendCMD(0, 0x80, Register.Byte80H);


        }

        private void button4_Click(object sender, EventArgs e)
        {
            Register.Byte81H = (byte)(Register.Byte81H | 0x08);
            USB.SendCMD(0, 0x81, Register.Byte81H);

        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                byte addr = Convert.ToByte(textBox5.Text, 16);

                byte value = Convert.ToByte(textBox6.Text, 16);

                if (addr < 0x80 || addr > 0xff)
                {
                    MessageBox.Show("请输入正确的addr!!");
                }
                else if (value < 0x00 || value > 0x7f)
                {
                    MessageBox.Show("请输入正确的value!!");
                }
                else
                {
                    USB.SendCMD(0, addr, value);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("请输入正确的地址和值：" + ex.Message);
            }


        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private static byte[] StrToHexByte(string hexString)
        {

            hexString = hexString.Replace(" ", "").Replace("\r", "").Replace("\n", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";

            byte[] returnBytes = new byte[hexString.Length / 2];

            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;

        }

        private void button7_Click(object sender, EventArgs e)
        {
            String Str_Content = textBox7.Text.Replace(" ", "");
            int lenth = (Str_Content.Length) / 2;
            if (lenth >= 0)
            {
                int AddToFour = lenth % 4;
                if (AddToFour != 0)
                {
                    for (int i = 0; i < (4 - AddToFour); i++) Str_Content += "00";
                }

                byte[] temp = StrToHexByte(textBox8.Text + lenth.ToString("x4") + Str_Content + textBox9.Text);

                USB.SendData(0, temp);
            }
            else
            {
                MyLog.Error("请至少输入4个Byte的数据");
            }
        }

        private void btn_80_0_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string str = btn.Name;

            string TextName = btn.Text;
            string[] tempList = TextName.Split(':');

            if (tempList[0] == "0")
            {

                btn.Text = "1:" + tempList[1];

                byte addr = Convert.ToByte(str.Substring(4, 2), 16);
                int bitpos = Convert.ToByte(str.Substring(7, 1), 10);
                switch (addr)
                {
                    case 0x80:
                        Register.Byte80H = (byte)(Register.Byte80H | (byte)(0x01 << bitpos));
                        USB.SendCMD(0, 0x80, Register.Byte80H);
                        break;
                    case 0x81:
                        Register.Byte81H = (byte)(Register.Byte81H | (byte)(0x01 << bitpos));
                        USB.SendCMD(0, 0x81, Register.Byte81H);
                        break;
                    case 0x82:
                        Register.Byte82H = (byte)(Register.Byte82H | (byte)(0x01 << bitpos));
                        USB.SendCMD(0, 0x82, Register.Byte82H);
                        break;
                    case 0x83:
                        Register.Byte83H = (byte)(Register.Byte83H | (byte)(0x01 << bitpos));
                        USB.SendCMD(0, 0x83, Register.Byte83H);
                        break;
                    case 0x84:
                        Register.Byte84H = (byte)(Register.Byte84H | (byte)(0x01 << bitpos));
                        USB.SendCMD(0, 0x84, Register.Byte84H);
                        break;
                    case 0x85:
                        Register.Byte85H = (byte)(Register.Byte85H | (byte)(0x01 << bitpos));
                        USB.SendCMD(0, 0x85, Register.Byte85H);
                        break;
                    default:
                        Register.Byte80H = (byte)(Register.Byte80H | (byte)(0x01 << bitpos));
                        USB.SendCMD(0, 0x80, Register.Byte80H);
                        break;
                }

            }
            //if (btn.Text == "0")
            //{
            //    btn.Text = "1";

            //    byte addr = Convert.ToByte(str.Substring(4, 2), 16);
            //    int bitpos = Convert.ToByte(str.Substring(7, 1), 10);
            //    switch (addr)
            //    {
            //        case 0x80:
            //            Register.Byte80H = (byte)(Register.Byte80H | (byte)(0x01 << bitpos));
            //            USB.SendCMD(0, 0x80, Register.Byte80H);
            //            break;
            //        case 0x81:
            //            Register.Byte81H = (byte)(Register.Byte81H | (byte)(0x01 << bitpos));
            //            USB.SendCMD(0, 0x81, Register.Byte81H);
            //            break;
            //        case 0x82:
            //            Register.Byte82H = (byte)(Register.Byte82H | (byte)(0x01 << bitpos));
            //            USB.SendCMD(0, 0x82, Register.Byte82H);
            //            break;
            //        case 0x83:
            //            Register.Byte83H = (byte)(Register.Byte83H | (byte)(0x01 << bitpos));
            //            USB.SendCMD(0, 0x83, Register.Byte83H);
            //            break;
            //        case 0x84:
            //            Register.Byte84H = (byte)(Register.Byte84H | (byte)(0x01 << bitpos));
            //            USB.SendCMD(0, 0x84, Register.Byte84H);
            //            break;
            //        case 0x85:
            //            Register.Byte85H = (byte)(Register.Byte85H | (byte)(0x01 << bitpos));
            //            USB.SendCMD(0, 0x85, Register.Byte85H);
            //            break;
            //        default:
            //            Register.Byte80H = (byte)(Register.Byte80H | (byte)(0x01 << bitpos));
            //            USB.SendCMD(0, 0x80, Register.Byte80H);
            //            break;
            //    }

            //}
            else
            {
                //                btn.Text = "0";

                btn.Text = "0:" + tempList[1];

                byte addr = Convert.ToByte(str.Substring(4, 2), 16);
                int bitpos = Convert.ToByte(str.Substring(7, 1), 10);
                switch (addr)
                {
                    case 0x80:
                        Register.Byte80H = (byte)(Register.Byte80H & (byte)(0x7f - (byte)(0x01 << bitpos)));
                        USB.SendCMD(0, 0x80, Register.Byte80H);
                        break;
                    case 0x81:
                        Register.Byte81H = (byte)(Register.Byte81H & (byte)(0x7f - (byte)(0x01 << bitpos)));
                        USB.SendCMD(0, 0x81, Register.Byte81H);
                        break;
                    case 0x82:
                        Register.Byte82H = (byte)(Register.Byte82H & (byte)(0x7f - (byte)(0x01 << bitpos)));
                        USB.SendCMD(0, 0x82, Register.Byte82H);
                        break;
                    case 0x83:
                        Register.Byte83H = (byte)(Register.Byte83H & (byte)(0x7f - (byte)(0x01 << bitpos)));
                        USB.SendCMD(0, 0x83, Register.Byte83H);
                        break;
                    case 0x84:
                        Register.Byte84H = (byte)(Register.Byte84H & (byte)(0x7f - (byte)(0x01 << bitpos)));
                        USB.SendCMD(0, 0x84, Register.Byte84H);
                        break;
                    case 0x85:
                        Register.Byte85H = (byte)(Register.Byte85H & (byte)(0x7f - (byte)(0x01 << bitpos)));
                        USB.SendCMD(0, 0x85, Register.Byte85H);
                        break;
                    default:
                        Register.Byte80H = (byte)(Register.Byte80H & (byte)(0x7f - (byte)(0x01 << bitpos)));
                        USB.SendCMD(0, 0x80, Register.Byte80H);
                        break;
                }

            }

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            this.richTextBox1.Clear();
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            double str_len = textBox7.Text.Length;
            double byte_len = str_len / 2;
            textBox10.Text = byte_len.ToString();
        }

        private void 查看寄存器表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myRegisterForm = new RegisterForm();

            myRegisterForm.Show();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            String Str_Content = textBox7.Text.Replace(" ", "");
            int lenth = (Str_Content.Length) / 2;
            if (lenth >= 0)
            {
                int AddToFour = lenth % 4;
                if (AddToFour != 0)
                {
                    for (int i = 0; i < (4 - AddToFour); i++) Str_Content += "00";
                }

                for (int j = 0; j < 8; j++)
                {
                    byte[] temp = StrToHexByte((0x1D00 + j).ToString("x4") + lenth.ToString("x4") + Str_Content + textBox9.Text);
                    temp[4] = (byte)(0x1 + j);
                    USB.SendData(0, temp);
                }
            }
            else
            {
                MyLog.Error("请至少输入4个Byte的数据");
            }
        }

        public SerialPort ComPortRecv;
        int DisLen = 2000000;
        private void btn_SerialOpen_2_Click(object sender, EventArgs e)
        {
            try
            {
                ComPortRecv = new SerialPort();
                ComPortRecv.BaudRate = Convert.ToInt32(comboBox_SerialBaudrate_2.Text);
                ComPortRecv.PortName = comboBox_SerialPortNum_2.Text;
                ComPortRecv.DataBits = Convert.ToInt32(comboBox_SerialDatabit_2.Text);

                switch (comboBox_SerialStopbit_2.Text)
                {
                    case "1":
                        ComPortRecv.StopBits = StopBits.One;
                        break;
                    case "1.5":
                        ComPortRecv.StopBits = StopBits.OnePointFive;
                        break;
                    case "2":
                        ComPortRecv.StopBits = StopBits.Two;
                        break;
                    default:
                        MessageBox.Show("Error:停止位参数设置不正确", "Error");
                        break;
                }

                switch (comboBox_SerialParity_2.Text)
                {
                    case "无校验":
                        ComPortRecv.Parity = Parity.None;
                        break;
                    case "偶校验":
                        ComPortRecv.Parity = Parity.Even;
                        break;
                    case "奇校验":
                        ComPortRecv.Parity = Parity.Odd;
                        break;
                    default:
                        MessageBox.Show("Error:校验位参数设置不正确", "Error");
                        break;
                }

                ComPortRecv.ReadTimeout = 1000;
                //ComPortRecv.ReceivedBytesThreshold = 1;
                ComPortRecv.Open();
                MyLog.Info("接收串口打开成功");

                //事件注册
                ComPortRecv.DataReceived += ComPortRecv_DataReceived;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

        }

        private void ComPortRecv_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //throw new NotImplementedException();
            Thread.Sleep(100);
            try
            {
                byte[] byteRead = new byte[ComPortRecv.BytesToRead];
                ComPortRecv.Read(byteRead, 0, byteRead.Length);
                //5a 54 01 02 03 04 05 06 07 08 00 5a fe 09 0a 0b 00 ff ff 00 0e 00 aa 5a fe
                int N = 53 + 11;
                byte[] SendToMac = new byte[N];
                SendToMac[0] = 0x5a;
                SendToMac[1] = 0x54;
                for (int i = 2; i < 53 + 2; i++) SendToMac[i] = 0x00;
                SendToMac[2] = 0x81;

                SendToMac[N - 9] = 0x00;
                SendToMac[N - 8] = 0xff;
                SendToMac[N - 7] = 0xff;
                SendToMac[N - 6] = 0x00;
                SendToMac[N - 5] = 0x53;
                SendToMac[N - 4] = 0x00;
                SendToMac[N - 3] = 0xaa;
                SendToMac[N - 2] = 0x5a;
                SendToMac[N - 1] = 0xfe;

                if (byteRead.Length > 0)
                {
                    if (byteRead[0] == 0xAB)
                    {
                        if (ExecDec) DisLen -= DecKM;
                        for (int j = 0; j < 30; j++)
                        {
                            this.textBox11.BeginInvoke(new Action(() => { this.textBox11.AppendText((DisLen / 10).ToString() + "m， "); }));

                            //MyLog.Info((DisLen / 10).ToString() + "m");
                            if (DisLen > 0)
                            {
                                SendToMac[11] = (byte)(DisLen & 0xff);
                                SendToMac[12] = (byte)((DisLen >> 8) & 0xff);
                                SendToMac[13] = (byte)((DisLen >> 16) & 0xff);
                                SendToMac[14] = (byte)((DisLen >> 24) & 0xff);
                            }
                            else
                            {
                                DisLen = 0;
                                SendToMac[11] = 0x00;
                                SendToMac[12] = 0x00;
                                SendToMac[13] = 0x00;
                                SendToMac[14] = 0x00;
                            }

                            ComPortRecv.Write(SendToMac, 0, SendToMac.Count());
                            // Thread.Sleep(1);
                        }
                    }
                    else
                    {
                        String temp = "";
                        for (int i = 0; i < byteRead.Length; i++) temp += byteRead[i].ToString("x2");
                        MyLog.Error("串口收到非0xAB数据！-----" + temp);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message + "--ComPortRecv_DataReceived");
            }
        }

        private void comboBox_SerialPortNum_2_DropDown(object sender, EventArgs e)
        {
            string[] str = SerialPort.GetPortNames();
            if (str == null)
            {
                MyLog.Info("尝试选择串口,但是本机没有串口！");
            }
            comboBox_SerialPortNum_2.Items.AddRange(str);
            int count = comboBox_SerialPortNum_2.Items.Count;

            for (int i = 0; i < count; i++)
            {
                string str1 = comboBox_SerialPortNum_2.Items[i].ToString();
                for (int j = i + 1; j < count; j++)
                {
                    string str2 = comboBox_SerialPortNum_2.Items[j].ToString();
                    if (str1 == str2)
                    {
                        comboBox_SerialPortNum_2.Items.RemoveAt(j);
                        count--;
                        j--;
                    }
                }
            }
        }

        private void button32_Click(object sender, EventArgs e)
        {
            ComPortRecv.Close();
            Thread.Sleep(1000);
            //   ComPortRecv.DataReceived -= ComPortRecv_DataReceived;

            Thread.Sleep(1000);


            MyLog.Info("接收串口关闭成功");
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            DisLen = 2000000;
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            this.textBox11.Clear();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            double t = double.Parse(textBox12.Text);
            DisLen = (int)(t * 10000);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            USB.SendCMD(0, 0x82, 0x7f);
            Thread.Sleep(100);
            USB.SendCMD(0, 0x82, 0x00);
            Thread.Sleep(100);
            USB.SendCMD(0, 0x83, 0x01);
            Thread.Sleep(100);
            USB.SendCMD(0, 0x83, 0x00);
        }

        bool ExecDec = false;
        private void button11_Click(object sender, EventArgs e)
        {
            if (button11.Text == "开启-每次递减")
            {
                button11.Text = "关闭-每次递减";
                ExecDec = true;
            }
            else
            {
                button11.Text = "开启-每次递减";
                ExecDec = false;
            }
        }

        int DecKM = 5000;
        private void button12_Click(object sender, EventArgs e)
        {
            double t = double.Parse(textBox13.Text);
            DecKM = (int)(t * 10);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Data.AdFrmIsAlive)
            {
                for (int i = 0; i < 8; i++)
                {
                    Data.dt_AD01.Rows[i]["测量值"] = Data.daRe_AD01[i];
                    Data.dt_AD02.Rows[i]["测量值"] = Data.daRe_AD02[i];
                }
            }
        }

        private void 查看AD数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(Data.AdFrmIsAlive)
            {
                myADForm.Activate();
            }
            else
            {
                myADForm = new ADForm();
            }
            myADForm.Show();
        }
    }


}
