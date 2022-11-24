using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using netDxf;

namespace RuiJi.Robotic
{
    public partial class MainForm : Form
    {
        private string dxfFileName = "";
        private int a4_w = 210;
        private int a4_h = 297;
        private float f = 1f;

        public MainForm()
        {
            InitializeComponent();
        }

        private void openFile_Click(object sender, EventArgs e)
        {
            var d = new OpenFileDialog();
            d.Filter = "dxf file (*.dxf)|*.dxf";
            if(d.ShowDialog() == DialogResult.OK)
            {
                dxfFileName = d.FileName;
                this.pictureBox1.Invalidate();
            }
        }

        private void openDxf(Graphics g)
        {
            var doc = DxfDocument.Load(dxfFileName);
            var pen = new Pen(Color.Black,1);

            foreach (var line in doc.Lines)
            {
                if (line.Layer.Name == "图框")
                    continue;

                g.DrawLine(pen,(float)line.StartPoint.X * f, (float)line.StartPoint.Y * f, (float)line.EndPoint.X * f, (float)line.EndPoint.Y * f);
            }

            foreach (var cir in doc.Circles)
            {
                if (cir.Layer.Name == "图框")
                    continue;

                var rect = new RectangleF();
                rect.X = (float)(cir.Center.X - cir.Radius) * f;
                rect.Y = (float)(cir.Center.Y - cir.Radius) * f;
                rect.Width = (float)cir.Radius * 2f * f;
                rect.Height = (float)cir.Radius * 2f * f;

                g.DrawEllipse(pen,rect);
            }

            foreach (var arc in doc.Arcs)
            {
                if (arc.Layer.Name == "图框")
                    continue;

                var rect = new RectangleF();
                rect.X = (float)(arc.Center.X - arc.Radius) * f;
                rect.Y = (float)(arc.Center.Y - arc.Radius) * f;
                rect.Width = (float)arc.Radius * 2f * f;
                rect.Height = (float)arc.Radius * 2f * f;

                g.DrawArc(pen, rect, (float)arc.StartAngle, (float)(arc.EndAngle - arc.StartAngle));
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (!string.IsNullOrEmpty(dxfFileName))
                openDxf(e.Graphics);
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            var f1 = pictureBox1.Width / a4_w;
            var f2 = pictureBox1.Height / a4_h;

            f = Math.Min(f1,f2);
        }

        private void splitContainer1_Panel2_Resize(object sender, EventArgs e)
        {
            var f1 = splitContainer1.Panel2.Width / a4_w;
            var f2 = splitContainer1.Panel2.Height / a4_h;

            f = Math.Min(f1, f2);

            var p = new Point();
            p.X = 20;

            pictureBox1.Width = splitContainer1.Panel2.Width - 40;
            pictureBox1.Height = pictureBox1.Width * a4_w / a4_h;
            p.Y = (splitContainer1.Panel2.Height - pictureBox1.Height) / 2;
            pictureBox1.Location = p;
        }
    }
}
