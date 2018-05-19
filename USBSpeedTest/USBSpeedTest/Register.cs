using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace USBSpeedTest
{
    class Register
    {
        public static byte Byte80H = 0x00;
        public static byte Byte81H = 0x00;
        public static byte Byte82H = 0x00;
        public static byte Byte83H = 0x00;
        public static byte Byte84H = 0x00;
        public static byte Byte85H = 0x00;

        public static byte Byte91H = 0x00;      //串口1设置
        public static byte Byte92H = 0x00;//串口2设置
        public static byte Byte93H = 0x00;//串口3设置
        public static byte Byte94H = 0x00;//串口4设置
        public static byte Byte95H = 0x00;//串口5设置
        public static byte Byte96H = 0x00;//串口6设置
        public static byte Byte97H = 0x00;//串口7设置
        public static byte Byte98H = 0x00;//串口8设置
        public static byte Byte99H = 0x00;//串口9设置
        public static byte Byte9AH = 0x00;//串口10设置
        public static byte Byte9BH = 0x00;//串口11设置
        public static byte Byte9CH = 0x00;//串口12设置

        public static byte[] Byte9xHs = { Byte91H, Byte92H, Byte93H, Byte94H, Byte95H, Byte96H,
            Byte97H, Byte98H, Byte99H,Byte9AH,Byte9BH,Byte9CH };

        public static byte ByteE7H = 0x00;

    }

    class Register_BoardOC
    {
        //80H对应系统初始化寄存器
        //bit7-|------bit6--|----bit5---|---bit4---bit3---|--------bit2--|------bit1---|---bit0---|
        //  0  |   LED控制  | 调试模式  |                 |  USB接收使能 | USB读写控制 | 总复位   |
        public static byte Byte80H = 0x00;
    }
}
