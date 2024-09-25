using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtrayUserInterface
{
    public static class COMclass
    {
        public static SerialPort com { get; set; } = new SerialPort();
        //串口名字
        public static string comName { get; set; }
        //波特率
        public static int comBaud { get; set; }
        //数据位
        public static int comDataBit { get; set; }
        //校验位
        public static string comVerify { get; set; }
        //停止位
        public static string comStopBit { get; set; }
        //串口的打开状态标记位
        public static bool openState { get; set; }

        public static List<string> comdata = new List<string>();

    }
}
