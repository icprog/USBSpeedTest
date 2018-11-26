using CyUSB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USBSpeedTest
{
    class USB
    {
        public static USBDeviceList usbDevices;

        public static CyUSBDevice MyDevice00 = null;
        public static CyUSBDevice MyDevice01 = null;
        public static CyUSBDevice MyDevice02 = null;
        public static CyUSBDevice MyDevice03 = null;
        public static CyUSBDevice MyDevice04 = null;
        public static CyUSBDevice MyDevice05 = null;
        public static CyUSBDevice MyDevice06 = null;
        public static CyUSBDevice MyDevice07 = null;
        public static CyUSBDevice MyDevice08 = null;
        public static CyUSBDevice MyDevice09 = null;
        public static CyUSBDevice MyDevice10 = null;
        public static CyUSBDevice MyDevice11 = null;
        public static CyUSBDevice MyDevice12 = null;
        public static CyUSBDevice MyDevice13 = null;
        public static CyUSBDevice MyDevice14 = null;
        public static CyUSBDevice MyDevice15 = null;

        public static List<CyUSBDevice> MyDeviceList = new List<CyUSBDevice>();

        public static void Init()
        {
            MyDeviceList.Add(MyDevice00); MyDeviceList.Add(MyDevice01); MyDeviceList.Add(MyDevice02); MyDeviceList.Add(MyDevice03);
            MyDeviceList.Add(MyDevice04); MyDeviceList.Add(MyDevice05); MyDeviceList.Add(MyDevice06); MyDeviceList.Add(MyDevice07);
            MyDeviceList.Add(MyDevice08); MyDeviceList.Add(MyDevice09); MyDeviceList.Add(MyDevice10); MyDeviceList.Add(MyDevice11);
            MyDeviceList.Add(MyDevice12); MyDeviceList.Add(MyDevice13); MyDeviceList.Add(MyDevice14); MyDeviceList.Add(MyDevice15);
        }

        public static void SendCMD(int id, byte ReqCode, byte Value)
        {
            if (MyDeviceList[id] != null)
            {
                CyControlEndPoint CtrlEndPt = null;
                CtrlEndPt = MyDeviceList[id].ControlEndPt;
                if (CtrlEndPt != null)
                {
                    lock (MyDeviceList[id])
                    {
                        CtrlEndPt.Target = CyConst.TGT_DEVICE;
                        CtrlEndPt.ReqType = CyConst.REQ_VENDOR;
                        CtrlEndPt.Direction = CyConst.DIR_TO_DEVICE;
                        CtrlEndPt.Index = 0;
                        CtrlEndPt.ReqCode = ReqCode;
                        CtrlEndPt.Value = (ushort)Value;
                        int len = 8;
                        byte[] buf = new byte[8];
                        CtrlEndPt.XferData(ref buf, ref len);

                        MyLog.Info("向USB机箱" + id.ToString() + "发送指令0x" + ReqCode.ToString("x2") + " 0x" + Value.ToString("x2") + "成功");
                    }
                }
            }
            else
            {
                MyLog.Error("USB设备未连接！");
            }
        }

        //向USB发送数据,数据区不加0
        public static void SendData(int id, byte[] temp)
        {
            int TempLength = temp.Length;
            if (MyDeviceList[id] != null)
            {
                lock (MyDeviceList[id])
                {
                    Register.Byte80H = (byte)(Register.Byte80H | 0x02);
                    SendCMD(id, 0x80, Register.Byte80H);
                    Register.Byte80H = (byte)(Register.Byte80H & 0xFD);
                    SendCMD(id, 0x80, Register.Byte80H);
                    if (MyDeviceList[id].BulkOutEndPt != null)
                    {
                        bool tag = MyDeviceList[id].BulkOutEndPt.XferData(ref temp, ref TempLength);
                        if (!tag)
                        {
                            MyLog.Error("传输数据到USB板卡失败");
                        }
                        else
                        {
                            MyLog.Info("传输成功:" + TempLength.ToString());
                        }

                    }
                }
            }
            else
            {
                MyLog.Error("USB设备未连接！");
            }

        }

        //向USB发送数据,数据区加3个0
        public static void SendDataByInt(int id, byte[] temp)
        {
            byte[] SendBytes = new byte[1024 * 16];

            int temp_lenth = temp.Length;
            int SendBytes_lenth = 4 * (temp_lenth - 4) + 4;
            for (int j = 0; j < 4; j++)
            {
                SendBytes[j] = temp[j];
            }
            for (int i = 4; i < SendBytes_lenth; i++)
            {
                if (i % 4 == 0) SendBytes[i] = temp[(i / 4) + 3];
                if (i % 4 == 1) SendBytes[i] = 0x0;
                if (i % 4 == 2) SendBytes[i] = 0x0;
                if (i % 4 == 3) SendBytes[i] = 0x0;
            }
            if (MyDeviceList[id] != null)
            {
                lock (MyDeviceList[id])
                {
                    Register.Byte80H = (byte)(Register.Byte80H | 0x02);
                    SendCMD(id, 0x80, Register.Byte80H);
                    Register.Byte80H = (byte)(Register.Byte80H & 0xFD);
                    SendCMD(id, 0x80, Register.Byte80H);

                    if (MyDeviceList[id].BulkOutEndPt != null)
                    {
                        bool tag = MyDeviceList[id].BulkOutEndPt.XferData(ref SendBytes, ref SendBytes_lenth);
                        if (tag)
                            MyLog.Info("传输数据到USB板卡成功");
                        else
                            MyLog.Error("传输数据到USB板卡失败");
                    }
                }
            }
            else
            {
                MyLog.Error("USB设备未连接！");
            }

        }
    }
}
