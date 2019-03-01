using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using RuiJi.Slicer.Core;
using RuiJi.Slicer.Core.File;
using RuiJi.Slicer.Core.ImageMould;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Assimp;

namespace RuiJi.Slice.App
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ShowSTL3D stlModel = null;

        private int middleSpeed = 10;     //滚轮的速度

        BluetoothClient client;
        Dictionary<string, BluetoothAddress> deviceAddresses = new Dictionary<string, BluetoothAddress>();
        IAsyncResult connectResult = null;

        public MainWindow()
        {
            InitializeComponent();
            //fileDlg.InitialDirectory = "D:\\";

            var context = new AssimpContext();
            var s = context.ImportFile(@"D:\云同步\vcoded\unity3d\Warrior\Assets\Models\hero.fbx");

        }

        #region 蓝牙
        private void ButtonSearchBt_Click(object sender, RoutedEventArgs e)
        {
            if (client != null)
            {
                if (client.Client.Connected)
                {
                    client.Client.Disconnect(true);
                    client.EndConnect(connectResult);
                }

                client = null;
            }

            client = new BluetoothClient();

            btn_searchBt.Content = "...";
            btn_searchBt.IsEnabled = false;

            sendMsg.Content = "正在搜索...";
            Task.Run(new Action(() =>
            {
                BluetoothRadio BuleRadio = BluetoothRadio.PrimaryRadio;
                BuleRadio.Mode = RadioMode.Connectable;
                BluetoothDeviceInfo[] Devices = client.DiscoverDevices();


                deviceAddresses.Clear();
                Dispatcher.Invoke(new Action(() =>
                {
                    lb_bt.Items.Clear();
                }));
                BrushConverter brushConverter = new BrushConverter();
                foreach (BluetoothDeviceInfo device in Devices)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        deviceAddresses[device.DeviceName] = device.DeviceAddress;
                        var radio = new RadioButton();
                        radio.GroupName = "bt";
                        radio.Content = device.DeviceName;
                        radio.Checked += ButtonCheckBt_Click;

                        var item = new ListBoxItem();
                        Brush brush = (Brush)brushConverter.ConvertFromString("#cccccc");
                        item.Background = brush;
                        item.Content = radio;

                        lb_bt.Items.Add(item);
                    }));
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    btn_searchBt.Content = "搜索蓝牙";
                    btn_searchBt.IsEnabled = true;
                    sendMsg.Content = "搜索完成,请选择";
                }));
            }));
        }

        private void ButtonCheckBt_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as RadioButton;
            var deviceName = btn.Content.ToString();
            BluetoothAddress address = deviceAddresses[deviceName];
            var point = new BluetoothEndPoint(address, BluetoothService.SerialPort);


            //while ((!(BluetoothSecurity.PairRequest(address, "1234"))) && count < max)
            //{
            //    Thread.Sleep(100);
            //}

            //if (count == max)
            //{
            //    MessageBox.Show("配对错误");
            //}
            //else
            //{
            client.SetPin(address, "1234");
            sendMsg.Content = "正在链接...";
            connectResult = client.BeginConnect(point, RemoteDeviceConnect, deviceName);
            //}


            //client.Connect(address, BluetoothService.SerialPort);
            // MessageBox.Show("已连接" + deviceName);
        }

        private void RemoteDeviceConnect(IAsyncResult result)// 异步连接完成
        {
            try
            {
                if (client.Client.Connected)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        sendMsg.Content = "链接" + result.AsyncState.ToString() + "成功";
                        btn_send.Visibility = Visibility.Visible;
                    }));
                }
                else
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        sendMsg.Content = "链接" + result.AsyncState.ToString() + "失败";
                    }));

                }
            }
            catch { }
        }

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            if (!client.Client.Connected)
            {
                MessageBox.Show("请先链接一个蓝牙设备");
                return;
            }
            sendMsg.Content = "启动切片";
            Task.Run(new Action(() =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    sendMsg.Content = "正在切片...";
                }));
                var buff = GetFrameCode();//生成文件
                Dispatcher.Invoke(new Action(() =>
                {
                    sendMsg.Content = "切片完成，开始发送";
                }));
                var stream = client.GetStream();

                var cmd = new byte[4];
                var process = 0;
                var sum = 200 * 10;

                //reset led
                var wb = ConcatCMD(cmd);
                wb[0] = 1;
                wb[1] = 0xC8;
                stream.Write(wb, 0, wb.Length);
                WaitResposne(stream);

                cmd[0] = 2;

                //transmit frame data
                for (byte i = 0; i < 200; i++)
                {
                    cmd[1] = i;

                    var data = buff.Skip(i * 320).Take(320).ToArray();
                    wb = ConcatCMD(cmd, data).ToArray();

                    try
                    {
                        stream.Write(wb, 0, wb.Length);
                        WaitResposne(stream);

                        process++;
                        Dispatcher.Invoke(new Action(() =>
                        {
                            sendMsg.Content = "发送进度:" + (i * 100f / 200) + "%";
                        }));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        i = 200;
                        break;
                    }
                }

                wb[0] = 3;
                wb[1] = 0;
                stream.Write(wb, 0, wb.Length);
                WaitResposne(stream);

                Dispatcher.Invoke(new Action(() =>
                {
                    sendMsg.Content = "发送完成";
                }));
            }));

            //client.Client.Send(dataBuffer, System.Net.Sockets.SocketFlags.None);
            //stream.Close();

            //MessageBox.Show("发送完成");
        }

        private byte[] ConcatCMD(byte[] cmd, byte[] data = null)
        {
            if (data != null)
                return cmd.Concat(data).ToArray();
            else
                return cmd.Concat(new byte[320]).ToArray();
        }

        private void WaitResposne(NetworkStream stream)
        {
            var rb = new byte[2];

            while (true)
            {
                stream.Read(rb, 0, 2);
                if (rb[0] == 79 && rb[1] == 75)
                    break;

                Thread.Sleep(10);
            }
        }
        #endregion

        private byte[] GetFrameCode()
        {
            string stlpath = "";
            var frame = ",";
            Dispatcher.Invoke(new Action(() =>
            {
                stlpath = path.Text;
            }));
            var doc = STLDocument.Open(stlpath);
            doc.MakeCenter();

            var results = RuiJi.Slicer.Core.Slicer.DoSlice(doc.Facets.ToArray(), new ArrayDefine[] {
                new ArrayDefine(new System.Numerics.Plane(0, 1, 0, 0), ArrayType.Circle, 200,360)
            });

            IImageMould im = new LED6432P();

            foreach (var key in results.Keys)
            {
                var images = SliceImage.ToImage(results[key], doc.Size, 64, 32, 0, 0);
                for (int i = 0; i < images.Count; i++)
                {
                    var bmp = images[i];
                    frame += "," + im.GetMould(bmp);
                }
            }

            frame = frame.TrimStart(',');
            return frame.Split(',').Select(m => Byte.Parse(m.Replace("0x", ""), System.Globalization.NumberStyles.AllowHexSpecifier)).ToArray();
        }
        /*
         * 打开文件对话框按钮
         * 返回值：所选的文件的路径
         */
        private void ButtonOpenStlFile_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Forms.OpenFileDialog fileDlg = new System.Windows.Forms.OpenFileDialog();
            fileDlg.Filter = "STL file(*.stl)|*.stl|All files(*.*)|*.*";
            fileDlg.FilterIndex = 0;
            fileDlg.ShowDialog();

            if (!string.IsNullOrEmpty(fileDlg.FileName))
            {
                Task.Run(new Action(() =>
              {
                  Dispatcher.Invoke(new Action(() =>
                  {
                      path.Text = fileDlg.FileName;
                  }));
                  Dispatcher.Invoke(new Action(() =>
                  {
                      var loading = new Loading();
                      loading.VerticalAlignment = VerticalAlignment.Center;
                      loading.MinWidth = 800;
                      loading.MinHeight = 150;
                      main_panel.Children.Insert(main_panel.Children.Count - 1, loading);
                      main_panel.RegisterName("stl_loading", loading);
                  }));
                  //Thread.Sleep(2000);

                  if (fileDlg.FileName != null && fileDlg.FileName.ToLower().EndsWith(".stl"))
                  {
                      Dispatcher.Invoke(new Action(() =>
                      {
                          base_panel.Visibility = Visibility.Visible;
                          btn_searchBt.Visibility = Visibility.Visible;
                          bt_panel.Visibility = Visibility.Visible;

                          var findviewer = FindName("trackBallDec") as _3DTools.TrackballDecorator;
                          main_panel.Children.Remove(findviewer);
                          main_panel.UnregisterName("trackBallDec");

                          var myviewer = FindName("myViewport3D") as Viewport3D;
                          main_panel.Children.Remove(myviewer);
                          main_panel.UnregisterName("myViewport3D");

                          var viewer = new _3DTools.TrackballDecorator();
                          viewer.Name = "trackBallDec";
                          viewer.Visibility = Visibility.Visible;
                          var view = new Viewport3D();
                          view.Name = "myViewport3D";
                          viewer.Content = view;
                          main_panel.Children.Insert(main_panel.Children.Count - 1, viewer);
                          main_panel.RegisterName("trackBallDec", viewer);
                          main_panel.RegisterName("myViewport3D", view);
                          ShowSTLModel();
                      }));
                  }
                  Dispatcher.Invoke(new Action(() =>
                  {
                      var findloading = FindName("stl_loading") as Loading;
                      main_panel.Children.Remove(findloading);
                      main_panel.UnregisterName("stl_loading");
                  }));

              }));
            }
        }

        /*
         * 生成Gcode按钮
         * 问题：参数未配置
         */
        //private void ButtonMakeGcode_Click(object sender, RoutedEventArgs e)
        //{
        //    if (stlModel == null)
        //    {
        //        MessageBox.Show("请先加载STL文件", "无法生成");
        //        return;
        //    }
        //    ParameterizedThreadStart threadStart = new ParameterizedThreadStart(MakeThreadProc);
        //    Thread thread = new Thread(threadStart);
        //    thread.Start(filePath);
        //}
        //public static void MakeThreadProc(object obj)
        //{
        //    MakeSTLGcode makeSTLGcode = new MakeSTLGcode(obj.ToString());
        //}

        /*
         * 调用ShowSTL3D类，显示3D图形
         */
        public void ShowSTLModel()
        {
            stlModel = new ShowSTL3D(path.Text);
            var myviewer = FindName("myViewport3D") as Viewport3D;
            myviewer.Children.Clear();
            myviewer.Children.Add(stlModel.GetMyModelVisual3D());
            myviewer.Children.Add(stlModel.myModelVisual3D());

            myviewer.Children.Add(stlModel.DrawWroldLine());

            myviewer.Camera = stlModel.MyCamera();
        }

        //滚轮事件
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (stlModel != null && e.LeftButton == MouseButtonState.Released)  //放大缩小
            {
                var myviewer = FindName("myViewport3D") as Viewport3D;
                myviewer.Camera = stlModel.nearerCamera(e.Delta / 120 * middleSpeed * (-1));
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            //if (e.MiddleButton == MouseButtonState.Pressed)
            //{
            //    middleSpeed += 5;
            //    if (middleSpeed > 20)
            //    {
            //        middleSpeed = 10;
            //    }
            //}
            if (e.RightButton == MouseButtonState.Pressed && stlModel != null)
            {
                //var myviewer = FindName("myViewport3D") as Viewport3D;
                //var findviewer = FindName("trackBallDec") as _3DTools.TrackballDecorator;
                //Transform3D transfrom3D = findviewer.Transform;
                
                //myviewer.Children.Remove(stlModel.GetModelVisual3D());
                //myviewer.Children.Add(stlModel.TransModelVisual3D(transfrom3D));

            }
            else if (e.LeftButton == MouseButtonState.Pressed && stlModel != null)
            {
                //var myviewer = FindName("myViewport3D") as Viewport3D;
                //var findviewer = FindName("trackBallDec") as _3DTools.TrackballDecorator;

                //Transform3D transfrom3D = findviewer.Transform;
                //myviewer.Children.Remove(stlModel.GetModelVisual3D());

                //myviewer.Children.Add(stlModel.TransModelVisual3DWithoutWorld(transfrom3D));
            }
        }

        //private void btn_world_Click(object sender, RoutedEventArgs e)
        //{
        //    if (stlModel != null)
        //    {
        //        if (!m_worldArrow)
        //        {
        //            myViewport3D.Children.Add(stlModel.DrawWroldLine());
        //        }
        //        //                 else
        //        //                 {
        //        //                     int index = myViewport3D.Children.IndexOf(stlModel.GetWorldLine());
        //        //                     myViewport3D.Children.RemoveAt(index);
        //        //                 }
        //        m_worldArrow = !m_worldArrow;
        //    }
        //}
    }
}
