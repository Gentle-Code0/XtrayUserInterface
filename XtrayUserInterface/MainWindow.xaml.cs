using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Windows.Threading;
using System.IO;
using System;

//TODO: 之后需要隐藏真实的发送数据，只显示是否发送成功与发送的指令名字叫什么，只有在保存发送数据时才保存原始真实的发送数据
namespace XtrayUserInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer dtimer = new DispatcherTimer();
        private MyCom mCom = new MyCom();

        //接受数据中最大储存1MB的字符串,即524288个char, 524288 * 2 = 1048576 bytes = 1MB
        private int receivedDataMaxLength = 524288;
        private string receivedOldText = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dtimer.Interval = TimeSpan.FromMilliseconds(100); //时间间隔为100ms
            dtimer.Tick += new EventHandler(Timer_Tick); //注册定时中断事件
            dtimer.Start();

            string[] ports = SerialPort.GetPortNames();
            this.Portname.ItemsSource = ports;
            this.Portname.SelectedIndex = 1;

            string[] baudrate = new string[] { "57600", "115200"};
            this.BaudRate.ItemsSource = baudrate;
            this.BaudRate.SelectedIndex = 1;

        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            this.Portname.ItemsSource = ports;
            this.Portname.SelectedIndex = 1;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            COMclass.comName = Portname.Text;
            COMclass.comBaud = int.Parse(BaudRate.Text);

            //关闭串口时删除之前所有的串口信息
            if (COMclass.openState == true)
            {
                ReceiveData.Text = string.Empty;
                Senddata.Text = string.Empty;
            }
            mCom.ComOpen();
        }

        //加入定时器，用来显示数据
        void Timer_Tick(object sender, EventArgs e)
        {

            if (COMclass.comdata.Count > 0)
            {
                //加入当前系统时间戳
                ReceiveData.Text += "[" + DateTime.Now.ToString() + "] ";
                ReceiveData.Text += COMclass.comdata[0] + "\r\n";
                COMclass.comdata.RemoveAt(0);
            }
            if (COMclass.openState)
            {
                ConnectPort.Content = "关闭串口";
                ConnectPort.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            }
            else
            {
                ConnectPort.Content = "打开串口";
                ConnectPort.Background = new SolidColorBrush(Color.FromRgb(128, 128, 128));

            }
        }

        //向esp32发送更新后的wifi信息
        private void WifiSet_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult wifiConfirm = MessageBox.Show("是否确认向X-Tray发送新的WiFi信息？注意它会覆盖原有的WiFi信息！", "操作确认", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
            if (wifiConfirm == MessageBoxResult.OK)
            {
                Button_Disable_Callback(WifiSet, 10000);

                string ssid = WifiUserName.Text;
                string pwd = WifiPassword.Password;
                string messageToSend = $"attr:wifi.config=<{ssid},{pwd}>";
                byte[] bytes = Encoding.Default.GetBytes(messageToSend);
                mCom.WriteDate(bytes);

                //在发送数据框中打印
                Senddata.Text += "[" + DateTime.Now.ToString() + "] " + messageToSend + "\r\n";
            }
        }

        //向esp32发送更新后的mqtt服务器信息
        private void MqttSet_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult mqttConfirm = MessageBox.Show("是否确认向X-Tray发送新的mqtt信息？注意它会覆盖原有的mqtt信息！\n请不要在不确定新的信息是否真的正确之前确认操作！",
                "操作确认", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
            if (mqttConfirm == MessageBoxResult.OK)
            {
                Button_Disable_Callback(MqttSet, 10000);

                string serverIp = ServerIP.Text;
                string serverName = ServerName.Text;
                string mqttUserName = MqttUserName.Text;
                string mqttPwd = MqttPassword.Password;

                string messageToSend = $"attr:mqtt.config=<{serverIp},{serverName},{mqttUserName},{mqttPwd}>";
                byte[] bytes = Encoding.Default.GetBytes(messageToSend);
                mCom.WriteDate(bytes);

                //在发送数据框中打印
                Senddata.Text += "[" + DateTime.Now.ToString() + "] " + messageToSend + "\r\n";
            }
        }

        //软件重启esp32
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult resetConfirm = MessageBox.Show("是否确定需要X-Tray进行重启？\nX-Tray重启后会自动执行去皮操作，请确保X-Tray重启时托盘上没有放置任何有重量的物品。",
                "操作确认", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
            if(resetConfirm == MessageBoxResult.OK)
            {
                string messageToSend = $"cmd:esp32.restart";
                byte[] bytes = Encoding.Default.GetBytes(messageToSend);
                mCom.WriteDate(bytes);

                //在发送数据框中打印
                Senddata.Text += "[" + DateTime.Now.ToString() + "] " + messageToSend + "\r\n";
            }
        }

        //向esp32发送执行去皮操作的命令
        private void WeightTare_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult tareConfirm = MessageBox.Show("是否确认执行去皮操作？\n注意它会使称重传感器读数归零！请勿在X-Tray上仍放有其他物品时确认操作！",
                "操作确认", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
            if (tareConfirm == MessageBoxResult.OK)
            {
                Button_Disable_Callback(WeightTare, 30000);

                string messageToSend = $"cmd:weight.tare";
                byte[] bytes = Encoding.Default.GetBytes(messageToSend);
                mCom.WriteDate(bytes);

                //在发送数据框中打印
                Senddata.Text += "[" + DateTime.Now.ToString() + "] " + messageToSend + "\r\n";
            }
        }

        //将接收和发送的数据保存至文件
        private void saveData_Click(object sender, RoutedEventArgs e)
        {
            Button_Disable_Callback(saveData, 10000);

            string receivedText = ReceiveData.Text;
            string sentText = Senddata.Text;
            string currentDirectory = Directory.GetCurrentDirectory();
            string filePath = currentDirectory + "/recievedDataLog.txt";            
            File.WriteAllText(filePath, receivedText);
            filePath = currentDirectory + "/sentDataLog.txt";
            File.WriteAllText(filePath, sentText);
            MessageBox.Show($"名为recievedDataLog.txt和sentDataLog.txt的文件已保存至 {currentDirectory} 目录下。", "保存成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        //非阻塞地使按钮变灰一段时间的函数
        private async void Button_Disable_Callback(Button target, int timeMs)
        {
            target.IsEnabled = false;
            await Task.Delay(timeMs);
            target.IsEnabled = true;
        }

        //检测接收数据文本框中的数据总大小，如果到达最大数据则从前往后删除多余的数据
        private void RecieveData_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ReceiveData != null)
            {
                if (ReceiveData.Text.Length > receivedDataMaxLength) // 当文本长度超过最大长度时
                {
                    // 保存旧的文本，用于后续删除
                    receivedOldText = ReceiveData.Text;

                    // 截取新的文本，从旧文本的末尾开始
                    string newText = receivedOldText.Substring(receivedOldText.Length - receivedDataMaxLength);

                    // 更新文本框显示的文本
                    ReceiveData.Text = newText;
                }
            }
        }

        private void CN_Click(object sender, RoutedEventArgs e)
        {
            XtrayUserInterface.Resources.ResourceExtension.Instance.CurrentCulture = "zh-CN";
        }

        private void EN_Click(object sender, RoutedEventArgs e)
        {
            XtrayUserInterface.Resources.ResourceExtension.Instance.CurrentCulture = "en";
        }
    }
}