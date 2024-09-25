using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace XtrayUserInterface
{
    public class MyCom
    {
        public void ComOpen()
        {
            if(COMclass.openState == false)
            {
                COMclass.com.PortName = COMclass.comName;
                COMclass.com.BaudRate = COMclass.comBaud;
                //COMclass.com.DataBits = COMclass.comDataBit;
                //if (COMclass.comStopBit == "1") COMclass.com.StopBits = System.IO.Ports.StopBits.One;
                //if (COMclass.comStopBit == "2") COMclass.com.StopBits = System.IO.Ports.StopBits.Two;
                //if (COMclass.comVerify == "None") COMclass.com.Parity = System.IO.Ports.Parity.None;
                //if (COMclass.comVerify == "Odd") COMclass.com.Parity = System.IO.Ports.Parity.Odd;
                //if (COMclass.comVerify == "Even") COMclass.com.Parity = System.IO.Ports.Parity.Even;
                COMclass.com.NewLine = "\r\n";
                Comthread();
            }
            else //关闭串口
            {
                COMclass.comdata.Add("关闭串口");
                COMclass.com.Close();
                COMclass.openState = false;
            }
        }

        public void ReadDate()
        {
            COMclass.comdata.Add("打开串口完成");
            COMclass.openState = true;
            while(COMclass.openState)
            {
                Thread.Sleep(50); //延时50ms再读取数据
                try
                {
                    int dataAmount = COMclass.com.BytesToRead; //查看串口目前储存了多少数据
                    byte[] buff = new byte[dataAmount];
                    COMclass.com.Read(buff, 0, dataAmount); //读取相应数量的串口数据

                    if(buff.Length > 0)
                    {
                        string dataStr = Encoding.Default.GetString(buff); //将寄存器中的数据转化为一个字符串
                        COMclass.comdata.Add(dataStr);
                    }
                }
                catch
                {
                    COMclass.openState = false;
                    COMclass.com.Close();
                }
            }
        }

        public void WriteDate(byte[] bytes)
        {
            try
            {
                if(COMclass.openState && bytes != null)
                {
                    COMclass.com.Write(bytes, 0, bytes.Length);
                }
            }
            catch { }
        }

        private void Comthread()
        {
            COMclass.com.Open();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            COMclass.com.Encoding = Encoding.GetEncoding("GB2312");
            Thread thread = new Thread(ReadDate);
            thread.IsBackground = true;
            thread.Start();
        }
    }
}
