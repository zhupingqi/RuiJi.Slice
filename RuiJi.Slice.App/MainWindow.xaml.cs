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
using System.Security.Cryptography;
using System.IO;
using Microsoft.Win32;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Drawing;
using ControlzEx.Standard;
using static System.Windows.Forms.LinkLabel;

namespace RuiJi.Slice.App
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        BluetoothClient client;
        Dictionary<string, BluetoothAddress> deviceAddresses = new Dictionary<string, BluetoothAddress>();
        IAsyncResult connectResult;
        SceneView sceneView;

        public MainWindow()
        {
            InitializeComponent();
            sceneView = new SceneView(myViewport3D);
            InitCircleControlEvent();
        }

        #region 蓝牙
        private async void ButtonSearchBt_Click(object sender, RoutedEventArgs e)
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

            //btn_searchBt.Content = "...";
            //btn_searchBt.IsEnabled = false;

            var controller = await this.ShowProgressAsync("Please wait...", "Progress message");

            await Task.Run(new Action(() =>
            {
                BluetoothDeviceInfo[] Devices = client.DiscoverDevices();
                var percentage = 0.5;
                controller.SetProgress(percentage);
                controller.SetMessage(percentage * 100 + "%");
                Thread.Sleep(1000);
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
                        var brush = (System.Windows.Media.Brush)brushConverter.ConvertFromString("#cccccc");
                        item.Background = brush;
                        item.Content = radio;

                        lb_bt.Items.Add(item);
                    }));
                }
                percentage = 1;
                controller.SetProgress(percentage);
                controller.SetMessage(percentage * 100 + "%");
                Thread.Sleep(1000);
                controller.CloseAsync();
                //Dispatcher.Invoke(new Action(() =>
                //{
                //    btn_searchBt.Content = "搜索蓝牙";
                //    btn_searchBt.IsEnabled = true;
                //    sendMsg.Content = "搜索完成,请选择";
                //}));
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
                        circleControl.IsEnabled = true;
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
            stream.ReadTimeout = 10000;

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

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
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


                var buff = GetFrameBuff(sceneView.MeshGroup);

                Dispatcher.Invoke(new Action(() =>
                {
                    sendMsg.Content = "切片完成，开始发送";
                }));


                TransmitStart(1);
                SendData(buff, 0);
                TransmitEnd();

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
                var ani = sceneView.GetAnimationTicks(name);
                var ticks = ani.DurationInTicks;

                Task.Run(() =>
                {
                    TransmitStart((int)ticks);

                    for (int i = 0; i < ticks; i++)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            sendMsg.Content = "正在进行动画帧 " + i + " 切片...";
                        }));

                        var model = sceneView.GetAnimationTick(i);
                        var buff = GetFrameBuff(model);

                        Dispatcher.Invoke(new Action(() =>
                        {
                            sendMsg.Content = "动画帧 " + i + " 切片完成，开始发送";
                        }));

                        SendData(buff, (short)i);

                        Dispatcher.Invoke(new Action(() =>
                        {
                            sendMsg.Content = "动画帧 " + i + "发送完成";
                        }));
                    }

                    TransmitEnd();

                    Dispatcher.Invoke(new Action(() =>
                    {
                        sendMsg.Content = "传输完成";
                    }));
                });
            }
        }

        string GetHash(string path)
        {
            //var hash = SHA256.Create();
            //var hash = MD5.Create();
            var hash = SHA1.Create();
            var stream = new FileStream(path, FileMode.Open);
            byte[] hashByte = hash.ComputeHash(stream);
            stream.Close();
            return BitConverter.ToString(hashByte).Replace("-", "");
        }

        private void TransmitStart(int frames)
        {
            var stream = client.GetStream();
            var cmd = new byte[4];

            //reset led , stop irq and timer
            var wb = ConcatCMD(cmd);
            wb[0] = 1;
            wb[1] = (byte)(frames >> 8);
            wb[2] = (byte)frames;
            wb[3] = 0xC8;
            stream.Write(wb, 0, wb.Length);
            WaitResposne(stream);
        }

        private void SendData(byte[] buff, short frame)
        {
            var stream = client.GetStream();

            var cmd = new byte[4];
            var process = 0;

            //reset led , stop irq and timer
            var wb = ConcatCMD(cmd);
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
                    var r = WaitResposne(stream);
                    while (!r)
                    {
                        r = WaitResposne(stream);
                    }

                    process++;
                    Dispatcher.Invoke(new Action(() =>
                    {
                        sendMsg.Content = "帧" + frame + "动画发送进度:" + (i * 100f / 200) + "%";
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
            wb[1] = (byte)frame;
            stream.Write(wb, 0, wb.Length);
            WaitResposne(stream);
        }

        private void TransmitEnd()
        {
            var stream = client.GetStream();
            var cmd = new byte[4];

            //reset led , stop irq and timer
            var wb = ConcatCMD(cmd);
            wb[0] = 4;
            wb[1] = 0;
            stream.Write(wb, 0, wb.Length);
            WaitResposne(stream);
        }

        private byte[] GetFrameBuff(Model3DGroup meshGroup)
        {
            var frame = ",";
            var facets = new List<Facet>();
            var size = new ModelSize(64, 64, 32);
            var offsetX = 0;
            var offsetY = 0;

            Dispatcher.Invoke(new Action(() =>
                {
                    offsetY = Convert.ToInt32(offsetYTextBox.Text);
                    var cloneMeshGroup = meshGroup.Clone();

                    var transform = myViewport3D.Children[myViewport3D.Children.Count - 1].Transform.Clone() as Transform3DGroup;

                    foreach (GeometryModel3D geo in cloneMeshGroup.Children)
                    {
                        var mesh = geo.Geometry as MeshGeometry3D;

                        for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
                        {
                            var p = mesh.Positions[mesh.TriangleIndices[i]];
                            var p0 = transform.Transform(p);

                            p = mesh.Positions[mesh.TriangleIndices[i + 1]];
                            var p1 = transform.Transform(p);

                            p = mesh.Positions[mesh.TriangleIndices[i + 2]];
                            var p2 = transform.Transform(p);

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
                var images = CircleSlicer.ToImage(results[key], size, 64, 32, offsetX, offsetY);
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (animationsList.SelectedItem != null)
            {
                var sfd = new SaveFileDialog();
                sfd.Filter = "睿吉全息文件(*.rjh)|*.*";
                sfd.RestoreDirectory = true;

                if (sfd.ShowDialog().Value)
                {
                    var name = animationsList.SelectedItem.ToString().Split('@')[0];
                    var ani = sceneView.GetAnimationTicks(name);
                    var ticks = ani.DurationInTicks;

                    Task.Run(() =>
                    {
                        var wb = new List<byte>() { 0, 0, 0, 0 };
                        wb[0] = (byte)((int)ticks >> 8);
                        wb[1] = (byte)ticks;
                        wb[2] = 0xC8;
                        wb[3] = 0;

                        for (int i = 0; i < ticks; i++)
                        {
                            Dispatcher.Invoke(new Action(() =>
                            {
                                sendMsg.Content = "正在进行动画帧 " + i + " 切片...";
                            }));

                            var model = sceneView.GetAnimationTick(i);
                            var buff = GetFrameBuff(model);

                            wb.AddRange(buff);

                            Dispatcher.Invoke(new Action(() =>
                            {
                                sendMsg.Content = "动画帧 " + i + " 切片完成";
                            }));
                        }

                        var filename = sfd.FileName.EndsWith(".rjh", StringComparison.InvariantCultureIgnoreCase) ? sfd.FileName : sfd.FileName + ".RJH";

                        File.WriteAllBytes(sfd.FileName + ".RJH", wb.ToArray());

                        Dispatcher.Invoke(new Action(() =>
                        {
                            sendMsg.Content = "文件保存完成";
                        }));
                    });
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Task.Run(new Action(() =>
            {
                var sfd = new SaveFileDialog();
                sfd.Filter = "睿吉全息文件(*.rjh)|*.*";
                sfd.RestoreDirectory = true;

                if (sfd.ShowDialog().Value)
                {
                    Task.Run(() =>
                    {
                        var wb = new List<byte>() { 0, 0, 0, 0 };
                        wb[0] = 0;
                        wb[1] = 1;
                        wb[2] = 0xC8;
                        wb[3] = 0;

                        Dispatcher.Invoke(new Action(() =>
                        {
                            sendMsg.Content = "正在切片...";
                        }));


                        var buff = GetFrameBuff(sceneView.MeshGroup);

                        Dispatcher.Invoke(new Action(() =>
                        {
                            sendMsg.Content = "切片完成，正在保存文件";
                        }));

                        wb.AddRange(buff);

                        var filename = sfd.FileName.EndsWith(".rjh", StringComparison.InvariantCultureIgnoreCase) ? sfd.FileName : sfd.FileName + ".RJH";

                        File.WriteAllBytes(sfd.FileName + ".RJH", wb.ToArray());

                        Dispatcher.Invoke(new Action(() =>
                        {
                            sendMsg.Content = "文件保存完成";
                        }));
                    });
                }
            }));
        }

        private void InitCircleControlEvent()
        {
            circleControl.ResetClick += new RoutedEventHandler(Btn_SliceReset_Click);
            circleControl.PreClick += new RoutedEventHandler(Btn_FilePre_Click);
            circleControl.NextClick += new RoutedEventHandler(Btn_FileNext_Click);
            circleControl.LeftClick += new RoutedEventHandler(Btn_SliceMoveLeft_Click);
            circleControl.RightClick += new RoutedEventHandler(Btn_SliceMoveRight_Click);
        }

        private void Btn_FilePre_Click(object sender, RoutedEventArgs e)
        {

            //MessageBox.Show("pre");
            SendAction(12);
        }

        private void Btn_SliceMoveLeft_Click(object sender, RoutedEventArgs e)
        {

            //MessageBox.Show("left");
            SendAction(10);
        }

        private async void Btn_SliceReset_Click(object sender, RoutedEventArgs e)
        {
            //await this.ShowMessageAsync("Reset", "");
            SendAction(14);
        }

        private void Btn_SliceMoveRight_Click(object sender, RoutedEventArgs e)
        {

            // MessageBox.Show("right");
            SendAction(11);
        }

        private void Btn_FileNext_Click(object sender, RoutedEventArgs e)
        {

            // MessageBox.Show("next");
            SendAction(13);
        }

        private void SendAction(byte action)
        {
            circleControl.IsEnabled = false;

            var stream = client.GetStream();
            var wb = new byte[] { 0, 0, 0, 0 };
            stream.Write(wb, 0, wb.Length);
            wb[0] = action;
            stream.Write(wb, 0, wb.Length);
            WaitResposne(stream);

            circleControl.IsEnabled = true;
        }

        private void RotateX_Add_Click(object sender, RoutedEventArgs e)
        {
            sceneView.Rotate(new System.Windows.Media.Media3D.Vector3D(1, 0, 0), 90);
        }

        private void RotateX_Sub_Click(object sender, RoutedEventArgs e)
        {
            sceneView.Rotate(new System.Windows.Media.Media3D.Vector3D(1, 0, 0), -90);
        }

        private void RotateY_Add_Click(object sender, RoutedEventArgs e)
        {
            sceneView.Rotate(new System.Windows.Media.Media3D.Vector3D(0, 1, 0), 90);
        }

        private void RotateY_Sub_Click(object sender, RoutedEventArgs e)
        {
            sceneView.Rotate(new System.Windows.Media.Media3D.Vector3D(0, 1, 0), -90);
        }

        private void RotateZ_Add_Click(object sender, RoutedEventArgs e)
        {
            sceneView.Rotate(new System.Windows.Media.Media3D.Vector3D(0, 0, 1), 90);
        }

        private void RotateZ_Sub_Click(object sender, RoutedEventArgs e)
        {
            sceneView.Rotate(new System.Windows.Media.Media3D.Vector3D(0, 0, 1), -90);
        }

        private void Btn_CNC_Click(object sender, RoutedEventArgs e)
        {
            var facets = new List<Facet>();
            var cloneMeshGroup = sceneView.MeshGroup.Clone();

            Dispatcher.Invoke(new Action(() =>
            {
                var transform = myViewport3D.Children[myViewport3D.Children.Count - 1].Transform.Clone() as Transform3DGroup;
                var bb = cloneMeshGroup.Bounds;
                var f = SlicerHelper.ScaleWeight(new Size3D(bb.SizeX, bb.SizeY, bb.SizeZ), new Size3D(800, 800, 2000));//1500,2000
                var s = (transform.Children[1] as ScaleTransform3D);
                s.ScaleX = f;
                s.ScaleY = f;
                s.ScaleZ = f;

                foreach (GeometryModel3D geo in cloneMeshGroup.Children)
                {
                    var mesh = geo.Geometry as MeshGeometry3D;

                    for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
                    {
                        var p = mesh.Positions[mesh.TriangleIndices[i]];
                        var p0 = transform.Transform(p);
                        mesh.Positions[mesh.TriangleIndices[i]] = p0;

                        p = mesh.Positions[mesh.TriangleIndices[i + 1]];
                        var p1 = transform.Transform(p);
                        mesh.Positions[mesh.TriangleIndices[i + 1]] = p1;

                        p = mesh.Positions[mesh.TriangleIndices[i + 2]];
                        var p2 = transform.Transform(p);
                        mesh.Positions[mesh.TriangleIndices[i + 2]] = p2;

                        if (p0 != p1 && p0 != p2 && p1 != p2)
                        {
                            var face = new Facet(p0, p1, p2);
                            facets.Add(face);
                        }
                    }
                }

                var b = cloneMeshGroup.Bounds;
                var c = (int)Math.Round((b.SizeY - b.Y) / 10.0);
                var d = new LinearArrayDefine(new Vector3(0, 1, 0), c, (float)b.Y, (float)(b.Y + b.SizeY));
                var result = SlicerHelper.DoLinearSlice(facets.ToArray(), d);

                var form = new CNCForm(result);
                form.ShowDialog();

                /*
                var images = LinearSlicer.ToImage(result, 1600, 1600);
                for (int i = 0; i < images.Count; i++)
                {
                    var bmp = images[i];
                    bmp.Save(AppDomain.CurrentDomain.BaseDirectory + @"1\" + i + ".bmp");
                    bmp.Dispose();
                }
                */
            }));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => {
                var facets = new List<Facet>();
                var cloneMeshGroup = sceneView.MeshGroup.Clone();
                var transform = myViewport3D.Children[myViewport3D.Children.Count - 1].Transform.Clone() as Transform3DGroup;

                foreach (GeometryModel3D geo in cloneMeshGroup.Children)
                {
                    var mesh = geo.Geometry as MeshGeometry3D;

                    for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
                    {
                        var p = mesh.Positions[mesh.TriangleIndices[i]];
                        var p0 = transform.Transform(p);
                        mesh.Positions[mesh.TriangleIndices[i]] = p0;

                        p = mesh.Positions[mesh.TriangleIndices[i + 1]];
                        var p1 = transform.Transform(p);
                        mesh.Positions[mesh.TriangleIndices[i + 1]] = p1;

                        p = mesh.Positions[mesh.TriangleIndices[i + 2]];
                        var p2 = transform.Transform(p);
                        mesh.Positions[mesh.TriangleIndices[i + 2]] = p2;

                        if (p0 != p1 && p0 != p2 && p1 != p2)
                        {
                            var face = new Facet(p0, p1, p2);
                            facets.Add(face);
                        }
                    }
                }

                var fs = new List<PlaneFacet>();

                foreach (var f  in facets)
                {
                    var find = fs.SingleOrDefault(m=>m.Plane == f.Plane); 
                    if (find != null)
                    {
                        find.AddFacet(f);
                    }
                    else
                    {
                        fs.Add(new PlaneFacet(f));
                    }
                }

                if (fs.Count == 0)
                    return;

                fs = fs.OrderByDescending(f => f.Area).ToList();

                var pri = fs.First();
                pri.Deep = 0;

                BuildIntersecteFacet(fs.Skip(1).ToList(),pri);

                var axis = new Vector3(pri.Center.X - 1, pri.Center.Y, pri.Center.Z) - pri.Center;
                axis = Vector3.Normalize(axis);
                pri.Transform(axis, (float) (90 * Math.PI/180.0));
                pri.Flatten();

                var pageW = Convert.ToInt32(paperWidth.Text);
                var pageH = Convert.ToInt32(paperHeight.Text);

                //(毫米数/25.4)*DPI
                var width = (int)(pageW / 25.4 * 72);
                var height = (int)(pageH / 25.4 * 72);

                var bitmap = new Bitmap(width, height);
                var g = Graphics.FromImage(bitmap);
                g.FillRectangle(System.Drawing.Brushes.White,0,0, width, height);
                //DrawTriangleFacet(g, pri, pageW / 2, pageH / 2);
                DrawFacet(g,pri, pageW / 2, pageH / 2);

                var ms = new MemoryStream();
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                var bytes = ms.GetBuffer();
                ms.Close();
                bitmap.Dispose();
                bitmap = null;

                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = new MemoryStream(bytes);
                image.EndInit();

                boxImage.Source = image;
            });
        }

        private void DrawFacet(Graphics g, PlaneFacet planeFacet, int offsetX = 0, int offsetY = 0)
        {
            var deep = Convert.ToInt32(drawDeep.Text);
            if (deep == -1)
                deep = int.MaxValue;

            if (deep < planeFacet.Deep)
                return;

            //var fs = planeFacet.FlatFacets.Where(m => m.Deep <= deep).OrderBy(m => m.Deep).ToList();

            //foreach (var p in planeFacet.FlatFacets)
            //{
                foreach (var line in planeFacet.AroundLines)
                {
                    var start = line.Start;
                    var end = line.End;

                    var x0 = (float)((start.X + offsetX) / 25.4 * 72);
                    var y0 = (float)((start.Y + offsetY) / 25.4 * 72);
                    var x1 = (float)((end.X + offsetX) / 25.4 * 72);
                    var y1 = (float)((end.Y + offsetY) / 25.4 * 72);

                    var pen = new System.Drawing.Pen(System.Drawing.Color.Black, 1);
                    g.DrawLine(pen, x0, y0, x1, y1);

                    if (line.ChildFacet != null)
                        DrawFacet(g, line.ChildFacet, offsetX, offsetY);
                }
            //}
        }

        private void DrawTriangleFacet(Graphics g, PlaneFacet planeFacet, int offsetX = 0, int offsetY = 0)
        {
            var origin = new Vector3(0, 0, 0);
            if (planeFacet.Plane.D != 0)
                origin = planeFacet.Plane.Normal * planeFacet.Plane.D;

            var q = System.Numerics.Matrix4x4.CreateLookAt(planeFacet.Center, origin, new Vector3(0, 0, 1));

            foreach (var line in planeFacet.AroundLines)
            {
                var start = Vector3.Transform(line.Start, q);
                var end = Vector3.Transform(line.End, q);

                var x0 = (float)((start.X + offsetX) / 25.4 * 72);
                var y0 = (float)((start.Y + offsetY) / 25.4 * 72);
                var x1 = (float)((end.X + offsetX) / 25.4 * 72);
                var y1 = (float)((end.Y + offsetY) / 25.4 * 72);

                var pen = new System.Drawing.Pen(System.Drawing.Color.Black, 1);
                g.DrawLine(pen, x0, y0, x1, y1);

                if (line.ChildFacet != null)
                {
                    DrawTriangleFacet(g, line.ChildFacet, offsetX, offsetY);
                }
            }
        }

        private void BuildIntersecteFacet(List<PlaneFacet> facets,PlaneFacet planeFacet)
        {
            foreach (var facet in facets)
            {
                if (facet.ParentLineSegment != null)
                    continue;

                var line = planeFacet.Collinear(facet);

                if (line != null)
                {
                    line.ChildFacet = facet;
                    facet.ParentLineSegment = line;
                    facet.Parent = planeFacet;
                    facet.Deep = planeFacet.Deep + 1;

                    BuildIntersecteFacet(facets, facet);

                    //var q = System.Numerics.Matrix4x4.CreateFromAxisAngle(line.Normal, facet.Angle);
                }
            }
        }
    }
}