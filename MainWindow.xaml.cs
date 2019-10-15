using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Net;
using System.IO.Ports;
using System.Diagnostics;
using System.Configuration;
using System.Threading;
using System.Windows.Threading;

namespace SliderSerialDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        string comport = ConfigurationManager.AppSettings["COM"];
        int Valmax = int.Parse(ConfigurationManager.AppSettings["MAX"].ToString());
        private SerialPort ComDevice = new SerialPort();
        bool isCanSend = false;
        bool isCanSend1 = false;
        DispatcherTimer time = new DispatcherTimer();

        string txtpath = Environment.CurrentDirectory + @"/val.txt";

        public MainWindow()
        {
            InitializeComponent();
            


            slider1.Maximum = Valmax;



            string[] ports = SerialPort.GetPortNames();
            //ComDevice.PortName = ports[0];
            ComDevice.PortName = comport;
            ComDevice.BaudRate = 115200;
            ComDevice.Parity = Parity.None;
            ComDevice.DataBits = 8;
            ComDevice.StopBits = StopBits.One;
            ComDevice.DataReceived += new SerialDataReceivedEventHandler(Com_DataReceived);//绑定事件
            ComDevice.Open();
            isCanSend1 = true;

            string tempval = TxtHelper.ReadAll(txtpath);
            if (!string.IsNullOrEmpty(tempval))
            {
                double lastSaveValue = double.Parse(tempval);
                slider1.Value = lastSaveValue;
                _val = lastSaveValue;
                LastVal = lastSaveValue;
                txt.Text = _val.ToString();
                ComDevice.Write(lastSaveValue + ",");
                Thread.Sleep(200);
                ComDevice.Write(lastSaveValue + ",");
                ComDevice.Write(lastSaveValue + ",");
            }
            else
            {
                ComDevice.Write("21,");
                Thread.Sleep(200);
            }

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {


            ComDevice.Write("tozero,");
           
            CompositionTarget.Rendering -= CompositionTarget_Rendering;

            TxtHelper.Write(txtpath, LastVal.ToString());

            ComDevice.Close();
            ComDevice.Dispose();
            Thread.Sleep(2000);
            //time.Stop();

        }



        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
            //Thread.Sleep(2000);
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            time.Interval = TimeSpan.FromMilliseconds(33);
            time.Tick += Time_Tick;
            // time.Start();

        }

        private void Time_Tick(object sender, EventArgs e)
        {
            try
            {
                //if (slider1.Value < 220)
                //    slider1.Value += 1;
                if (isCanSend1)
                {
                    if (cycleState != CycleState.pause)
                    {
                        if (index % 2 == 0)
                        {

                            #region left
                            if (cycleState == CycleState.left)
                            {
                                if (_val < Valmax)
                                    _val += 1;
                                else
                                      if ((bool)chk1.IsChecked)
                                    cycleState = CycleState.right;
                            }
                            #endregion

                            #region right
                            if (cycleState == CycleState.right)
                            {
                                if (_val > 21)
                                    _val -= 1;
                                else
                                    if ((bool)chk1.IsChecked)
                                    cycleState = CycleState.left;
                            }
                            #endregion

                            slider1.Value = _val;
                        }

                    }

                    #region send val position
                    CurrentVal = _val;

                    if (LastVal != CurrentVal)
                    {
                        double _sub = CurrentVal - LastVal;
                        if (cycleState == CycleState.pause || cycleState == CycleState.slider)
                        {
                            if (Math.Abs(_sub) > 1)
                            {
                                if (_sub > 0)
                                    speed = 1;
                                if (_sub < 0)
                                    speed = -1;
                                LastVal += speed;
                                Console.WriteLine("***************************************************************current value:" + LastVal);
                                //Thread.Sleep(20);
                                ComDevice.Write(LastVal.ToString() + ",");
                                if (LastVal == CurrentVal)
                                    LastVal = _val;
                            }
                        }
                        if (cycleState == CycleState.left || cycleState == CycleState.right)
                        {
                            //Thread.Sleep(20);
                            ComDevice.Write(_val.ToString() + ",");
                            txt.Text = _val.ToString();

                            LastVal = _val;
                            Console.WriteLine("----------------------------------------------------------------------------------------");
                        }

                    }

                    #endregion
                    index++;
                    if (index > 10000)
                        index = 1;
                }

                #region 注释



                //    txt_Copy.Text = rees;
                //    if (index % 3 == 0)
                //    {

                //        if (isCanSend1&& IsAutomatic)
                //        {
                //            //ComDevice.Write(CurrentVal.ToString());
                //            if (_val >= 1000)
                //            {
                //                if (_CirCleNum >= 100000000)
                //                    _CirCleNum = 0;
                //                _CirCleNum++;
                //                isBack = false;
                //                _val = 999;
                //                ErrorLog.WriteLog("It's turn back now!++:"+ _CirCleNum);

                //            }
                //            if (_val <= 0)
                //            {
                //                _CirCleNum++;
                //                if (_CirCleNum >= 100000000)
                //                    _CirCleNum = 0;

                //                _val = 1;
                //                isBack = true;
                //                ErrorLog.WriteLog("It's turn back now!--:"+ _CirCleNum);
                //            }
                //            if (isBack)
                //                _val++;
                //            else
                //                _val--;
                //            txt_Copy1.Text = "当前值：" + _val.ToString() + " 第：" + _CirCleNum + "圈";
                //            Console.WriteLine("-------------------------------:" + _val);
                //        }
                //    }
                //    if (isCanSend)
                //    {
                //        isCanSend = false;
                //        ComDevice.Write(_val.ToString() + ",");
                //        _val1 = _val;
                //    }
                //    if (_val1 == _val)
                //        isCanSend = false;
                //    else
                //        isCanSend = true;
                //    index++;
                //    if (index > 10000)
                //        index = 1;

                #endregion
            }
            catch (Exception ex)
            {
                ErrorLog.WriteLog(ex);
                Process.GetCurrentProcess().Kill();
            }


        }



        string rees = "";
        int index = 0;

        double CurrentVal = 0;
        double LastVal = 21;

        bool isBack = true;
        int _CirCleNum = 0;
        bool IsAutomatic = true;

        CycleState cycleState = CycleState.slider;
        double speed = 1;
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            try
            {
                //if (slider1.Value < 220)
                //    slider1.Value += 1;
                if (isCanSend1)
                {
                    if (cycleState != CycleState.pause)
                    {
                        if (index % 2 == 0)
                        {

                            #region left
                            if (cycleState == CycleState.left)
                            {
                                if (_val < Valmax)
                                    _val += 1;
                                else
                                      if ((bool)chk1.IsChecked)
                                    cycleState = CycleState.right;
                            }
                            #endregion

                            #region right
                            if (cycleState == CycleState.right)
                            {
                                if (_val > 21)
                                    _val -= 1;
                                else
                                    if ((bool)chk1.IsChecked)
                                    cycleState = CycleState.left;
                            }
                            #endregion

                            slider1.Value = _val;
                        }

                    }

                    #region send val position
                    CurrentVal = _val;

                    if (LastVal != CurrentVal)
                    {
                        double _sub = CurrentVal - LastVal;
                        if (cycleState == CycleState.pause || cycleState == CycleState.slider)
                        {
                            if (Math.Abs(_sub) > 1)
                            {
                                if (_sub > 0)
                                    speed = 1;
                                if (_sub < 0)
                                    speed = -1;
                                LastVal += speed;
                                Console.WriteLine("***************************************************************current value:" + LastVal);
                                Thread.Sleep(60);
                                ComDevice.Write(LastVal.ToString() + ",");
                                if (LastVal == CurrentVal)
                                    LastVal = _val;
                            }
                        }
                        if (cycleState == CycleState.left || cycleState == CycleState.right)
                        {
                            Thread.Sleep(60);
                            ComDevice.Write(_val.ToString() + ",");
                            txt.Text = _val.ToString();

                            LastVal = _val;
                            Console.WriteLine("----------------------------------------------------------------------------------------");
                        }

                    }

                    #endregion
                    index++;
                    if (index > 10000)
                        index = 1;
                }

                #region 注释



                //    txt_Copy.Text = rees;
                //    if (index % 3 == 0)
                //    {

                //        if (isCanSend1&& IsAutomatic)
                //        {
                //            //ComDevice.Write(CurrentVal.ToString());
                //            if (_val >= 1000)
                //            {
                //                if (_CirCleNum >= 100000000)
                //                    _CirCleNum = 0;
                //                _CirCleNum++;
                //                isBack = false;
                //                _val = 999;
                //                ErrorLog.WriteLog("It's turn back now!++:"+ _CirCleNum);

                //            }
                //            if (_val <= 0)
                //            {
                //                _CirCleNum++;
                //                if (_CirCleNum >= 100000000)
                //                    _CirCleNum = 0;

                //                _val = 1;
                //                isBack = true;
                //                ErrorLog.WriteLog("It's turn back now!--:"+ _CirCleNum);
                //            }
                //            if (isBack)
                //                _val++;
                //            else
                //                _val--;
                //            txt_Copy1.Text = "当前值：" + _val.ToString() + " 第：" + _CirCleNum + "圈";
                //            Console.WriteLine("-------------------------------:" + _val);
                //        }
                //    }
                //    if (isCanSend)
                //    {
                //        isCanSend = false;
                //        ComDevice.Write(_val.ToString() + ",");
                //        _val1 = _val;
                //    }
                //    if (_val1 == _val)
                //        isCanSend = false;
                //    else
                //        isCanSend = true;
                //    index++;
                //    if (index > 10000)
                //        index = 1;

                #endregion
            }
            catch (Exception ex)
            {
                ErrorLog.WriteLog(ex);
                Process.GetCurrentProcess().Kill();
            }

        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] ReDatas = new byte[ComDevice.BytesToRead];
            ComDevice.Read(ReDatas, 0, ReDatas.Length);//读取数据
            //this.AddData(ReDatas);//输出数据

            rees = Encoding.Default.GetString(ReDatas);

            Console.WriteLine(rees);
            //txt_Copy.Text = ll;
        }
        double _val = 0;

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isCanSend1)
            {
                //cycleState = CycleState.slider;
                _val = (int)(e.NewValue);
                //ComDevice.Write(_val.ToString()+",");
                txt.Text = _val.ToString();
                Console.WriteLine();
            }
        }

        private void Btn_left_Click(object sender, RoutedEventArgs e)
        {
            cycleState = CycleState.right;
            txt_Copy1.Text = "LEFT";
        }

        private void Btn_right_Click(object sender, RoutedEventArgs e)
        {
            cycleState = CycleState.left;
            txt_Copy1.Text = "RIGHT";
        }

        private void Btn_pause_Click(object sender, RoutedEventArgs e)
        {
            cycleState = CycleState.pause;
            txt_Copy1.Text = "PAUSE";
        }

        private void Btn_slider_Click(object sender, RoutedEventArgs e)
        {
            cycleState = CycleState.slider;
            txt_Copy1.Text = "USE SLIDER";
        }
    }

    public enum CycleState
    {
        left,
        right,
        pause,
        slider
    }
}
