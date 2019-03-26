using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using RuiJi.Slicer.Core;
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
using _3DTools;
using RuiJi.Slicer.Core.Viewport;
using RuiJi.Slicer.Core.Array;
using RuiJi.Slicer.Core.Slicer;

namespace RuiJi.Slice.App
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        BluetoothClient client;
        Dictionary<string, BluetoothAddress> deviceAddresses = new Dictionary<string, BluetoothAddress>();
        IAsyncResult connectResult;
        SceneView sceneView;

        public MainWindow()
        {
            InitializeComponent();
            sceneView = new SceneView(myViewport3D);
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

            client.SetPin(address, "1234");
            sendMsg.Content = "正在链接...";
            connectResult = client.BeginConnect(point, RemoteDeviceConnect, deviceName);
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
                        btn_send.IsEnabled = true;
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

        private byte[] ConcatCMD(byte[] cmd, byte[] data = null)
        {
            if (data != null)
                return cmd.Concat(data).ToArray();
            else
                return cmd.Concat(new byte[320]).ToArray();
        }

        private bool WaitResposne(NetworkStream stream)
        {
            var rb = new byte[2];
            stream.ReadTimeout = 100;

            Thread.Sleep(100);

            try
            {
                stream.Read(rb, 0, 1);
                stream.Read(rb, 1, 1);
            }
            catch
            {

            }

            if (rb[0] == 79 && rb[1] == 75)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region 滚轮事件
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            sceneView.LightCamera.Zoom(e.Delta < 0);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
        } 
        #endregion

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var fileDlg = new System.Windows.Forms.OpenFileDialog();
            fileDlg.Filter = "all files(*.*)|*.*|STL file(*.stl)|*.stl|FBX files(*.fbx)|*.fbx";
            fileDlg.FilterIndex = 0;
            var result = fileDlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        path.Text = fileDlg.FileName;

                        var loading = new Loading();
                        trackBallDec.Visibility = Visibility.Hidden;
                        main_panel.Children.Add(loading);

                        if (sceneView.Load(fileDlg.FileName))
                        {
                            animationsList.Visibility = Visibility.Hidden;
                            animationsList.Items.Clear();

                            if (sceneView.HasAnimations)
                            {
                                sceneView.Animations.ForEach(m =>
                                {
                                    animationsList.Items.Add(m);
                                });

                                animationsList.Visibility = Visibility.Visible;                               
                            }
                        }

                        main_panel.Children.RemoveAt(main_panel.Children.Count - 1);
                        trackBallDec.Visibility = Visibility.Visible;                        
                    });
                });
            }
        }

        private void animationsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (animationsList.SelectedItem != null)
            {
                var name = animationsList.SelectedItem.ToString().Split('@')[0];
                sceneView.Play(name);
            }
        }

        private void BtnMeshSend_Click(object sender, RoutedEventArgs e)
        {
            if (!client.Client.Connected)
            {
                MessageBox.Show("请先链接一个蓝牙设备");
                return;
            }

            Task.Run(new Action(() =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    sendMsg.Content = "正在切片...";
                }));

                var r = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(1, 0, 0), 90);

                var buff = GetFrameBuff(sceneView.MeshGroup, r);

                Dispatcher.Invoke(new Action(() =>
                {
                    sendMsg.Content = "切片完成，开始发送";
                }));

                SendData(buff, 0, true);

                Dispatcher.Invoke(new Action(() =>
                {
                    sendMsg.Content = "发送完成";
                }));
            }));
        }

        private void Btn_AnimationSend_Click(object sender, RoutedEventArgs e)
        {
            if (animationsList.SelectedItem != null)
            {
                var name = animationsList.SelectedItem.ToString().Split('@')[0];
                var ticks = sceneView.GetAnimationTicks(name);
                var mt = 30;
                if (ticks < mt)
                    mt = ticks;

                var tickList = new List<double>();
                for (double i = 0; i < mt; i++)
                {
                    tickList.Add(Math.Round(ticks*i/(mt-1))-1);
                }

                tickList[0] = 0;

                Task.Run(() =>
                {
                    for (int i = 0; i < tickList.Count; i++)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            sendMsg.Content = "正在进行动画帧 " + i + " 切片...";
                        }));

                        var model = sceneView.GetAnimationTick(tickList[i]);
                        var buff = GetFrameBuff(model);

                        Dispatcher.Invoke(new Action(() =>
                        {
                            sendMsg.Content = "动画帧 " + i + " 切片完成，开始发送";
                        }));

                        SendData(buff, (byte)i, i == tickList.Count - 1);

                        Dispatcher.Invoke(new Action(() =>
                        {
                            sendMsg.Content = "动画帧 " + i + "发送完成";
                        }));
                    }

                    Dispatcher.Invoke(new Action(() =>
                    {
                        sendMsg.Content = "传输完成";
                    }));
                });
            }
        }

        private void SendData(byte[] buff, byte tick,bool start = false)
        {
            var stream = client.GetStream();

            var cmd = new byte[4];
            var process = 0;

            //reset led , stop irq and timer
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
                        sendMsg.Content = "帧" + tick + "动画发送进度:" + (i * 100f / 200) + "%";
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    i = 200;
                    break;
                }
            }

            //save buff to spi flash
            wb[0] = 3;
            wb[1] = tick;
            stream.Write(wb, 0, wb.Length);
            WaitResposne(stream);

            //start irq and timer
            if (start)
            {
                wb[0] = 4;
                wb[1] = 0;
                stream.Write(wb, 0, wb.Length);
                WaitResposne(stream);
            }
        }

        private byte[] GetFrameBuff(Model3DGroup meshGroup, AxisAngleRotation3D rotation = null)
        {
            var frame = ",";
            var facets = new List<Facet>();
            var size = new ModelSize(64, 64, 32);

            var axis = new System.Windows.Media.Media3D.Vector3D(); 
            var angle = 0d;
            if(rotation != null)
            {
                axis = rotation.Axis;
                angle = rotation.Angle;
            }

            Dispatcher.Invoke(new Action(() =>
            {
                var cloneMeshGroup = meshGroup.Clone();

                var transform = myViewport3D.Children[myViewport3D.Children.Count - 1].Transform.Clone() as Transform3DGroup;

                foreach (GeometryModel3D geo in cloneMeshGroup.Children)
                {
                    var mesh = geo.Geometry as MeshGeometry3D;

                    for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
                    {
                        var p = mesh.Positions[mesh.TriangleIndices[i]];
                        p = transform.Transform(p);                        
                        var p0 = new Vector3((float)p.X, (float)p.Y, (float)p.Z);

                        p = mesh.Positions[mesh.TriangleIndices[i + 1]];
                        p = transform.Transform(p);
                        var p1 = new Vector3((float)p.X, (float)p.Y, (float)p.Z);

                        p = mesh.Positions[mesh.TriangleIndices[i + 2]];
                        p = transform.Transform(p);
                        var p2 = new Vector3((float)p.X, (float)p.Y, (float)p.Z);

                        facets.Add(new Facet(p0, p1, p2));
                    }
                }
            }));

            //facets.RemoveAll(m => m.TooSmall);

            var results = SlicerHelper.DoCircleSlice(facets.ToArray(), new CircleArrayDefine[] {
                new CircleArrayDefine(new System.Numerics.Plane(0, 0, 1, 0), 200,360)
            });

            IImageMould im = new LED6432P();

            foreach (var key in results.Keys)
            {
                var images = CircleSlicer.ToImage(results[key], size, 64, 32, 0, 0);
                for (int i = 0; i < images.Count; i++)
                {
                    var bmp = images[i];
                    //bmp.Save(AppDomain.CurrentDomain.BaseDirectory + i + ".bmp");
                    frame += "," + im.GetMould(bmp);
                }
            }

            frame = frame.TrimStart(',');
            return frame.Split(',').Select(m => Byte.Parse(m.Replace("0x", ""), System.Globalization.NumberStyles.AllowHexSpecifier)).ToArray();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            sceneView.Stop();
        }
    }
}