using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;

namespace RuiJi.Slice.Window
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = dlg.FileName;
            }
        }

        private void btn_search_Click(object sender, EventArgs e)
        {
            BluetoothClient client = new BluetoothClient();
            BluetoothRadio radio = BluetoothRadio.Primary
        }
    }
}
