using RuiJi.Slicer.Core.Slicer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace RuiJi.Slice.App
{
    public partial class CNCForm : Form
    {
        private SerialPort serialPort = null;
        private bool send = false;
        private string cmd = "";
        private Timer timer;
        public CNCForm()
        {
            InitializeComponent();

            timer = new Timer();
            timer.Interval = 20;
            timer.Tick += Timer_Tick;
            timer.Start();

            LoadPortName();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (send && serialPort != null && serialPort.IsOpen)
            {
                serialPort.WriteLine(cmd);
            }
        }

        public CNCForm(Dictionary<float, List<SlicedPlane>> results) :this()
        {
            RuiJiCNC.Instance.SetDict(results);            
        }

        private void LoadPortName()
        {
            var coms = SerialPort.GetPortNames();
            listBox1.Items.Clear();

            foreach (var c in coms)
            {
                listBox1.Items.Add(c);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            LoadPortName();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;

            var name = listBox1.SelectedItem.ToString();

            serialPort = new SerialPort(name, 115200, Parity.None, 8, StopBits.One);
            serialPort.Handshake = Handshake.None;
            serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
            serialPort.WriteTimeout = 100;
            serialPort.RtsEnable = true;
            serialPort.DtrEnable = true;
            serialPort.NewLine = "\r\n";
            serialPort.Open();

            button1.Enabled = false;
            button22.Enabled = true;
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            this.Invoke(new EventHandler(DoUpdate));
        }

        private void DoUpdate(object s, EventArgs e)
        {
            var r = serialPort.ReadLine();

            if(r == "i")
            {
                RuiJiCNC.Instance.SetEvent();
                idleResetEvent.Set();
            }
            else
            {
                this.textBox1.Text += r + Environment.NewLine;
            }
        }

        private void button22_Click(object sender, EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Dispose();
                serialPort.Close();

                button1.Enabled = true;
                button22.Enabled = false;
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }
        }

        private void button24_Click(object sender, EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                RuiJiCNC.Instance.Start(serialPort);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RuiJiCNC.Instance.Stop();
        }

        private void CNCForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                RuiJiCNC.Instance.Stop();

                serialPort.Dispose();
                serialPort.Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.WriteLine("m:20,20");
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.WriteLine("reset");
            }
        }

        private ManualResetEvent idleResetEvent = new ManualResetEvent(false);
        private bool cancelTask = false;
        private bool taskRun = false;
        private int stepSleep = 1;

        private void button4_Click(object sender, EventArgs e)
        {
            if (!taskRun)
            {
                taskRun = true;
                cancelTask = false;
                button4.Text = "stop";

                Task.Run(() =>
                {
                    while(true)
                    {
                        if (cancelTask)
                            break;

                        idleResetEvent.Reset();
                        idleResetEvent.WaitOne();

                        if (serialPort != null && serialPort.IsOpen)
                        {
                            serialPort.WriteLine("step_a_200");
                        }

                        Thread.Sleep(stepSleep * 10);
                    }
                });
            }
            else
            {
                cancelTask = true;
                taskRun = false;
                button4.Text = "step a";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.WriteLine("speed_quick");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.WriteLine("speed_slow");
            }
        }

        private void action_MouseDown(object sender, MouseEventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                try
                {
                    cmd = ((Button)sender).Tag.ToString();
                    if (!checkBox1.Checked)
                    {
                        send = true;
                    }
                    else
                    {
                        serialPort.WriteLine(cmd);
                    }
                }
                catch { }
            }
        }

        private void action_MouseUp(object sender, MouseEventArgs e)
        {
            send = false;
        }

        private void button23_Click(object sender, EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.WriteLine(((Button)sender).Tag.ToString());
            }
        }

        private void button25_Click(object sender, EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                if (button25.Text != "stop")
                {
                    serialPort.WriteLine("move");
                    button25.Text = "stop";
                }
                else
                {
                    serialPort.WriteLine("stop_move");
                    button25.Text = "move 2";
                }
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.WriteLine(((Button)sender).Tag.ToString());
            }
        }

        private void button26_Click(object sender, EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.WriteLine($"set_speed:{ Convert.ToUInt16(numericUpDown1.Value)}");
            }
        }
    }
}
