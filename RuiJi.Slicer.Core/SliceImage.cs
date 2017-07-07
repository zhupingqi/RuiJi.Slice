using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace RuiJi.Slicer.Core
{
    public class SliceImage
    {
        public static void ToImage(List<SlicedPlane> slicedPlane,int w,int h,int width,int height,string prefix)
        {
            var filename = AppDomain.CurrentDomain.BaseDirectory + prefix + "_frame.h";
            System.IO.File.Delete(filename);

            var firstNormal = slicedPlane.First().Plane.Normal;
            var frame = 0;
            var code = "";

            foreach (var sp in slicedPlane)
            {
                var lines = new List<Vector2[]>();

                //var origin = new Vector3(0, 0, 0);
                //if (sp.Plane.D != 0)
                //    origin = firstNormal * sp.Plane.D;

                //var a = -sp.Angle;
                //var q = Matrix4x4.CreateFromAxisAngle(sp.Axis, a);
                //q.Translation = -origin;

                foreach (var line in sp.Lines)
                {
                    //var s = Vector3.Transform(line.Start, q);
                    //var e = Vector3.Transform(line.End, q);

                    var s = To2D(sp.Plane,line.Start);
                    var e = To2D(sp.Plane,line.End);

                    lines.Add(new Vector2[] {
                        new Vector2(s.X,s.Y),
                        new Vector2(e.X,e.Y)
                    });
                }

                code += ToImage(lines,frame++,w,h, width, height, prefix);
            }

            var frameTable = new List<string>();

            for (int i = 0; i < frame; i++)
            {
                frameTable.Add(prefix + "_frames_" + i);
            }

            code += "unsigned char* " + prefix + "_frames_table[] = { " + string.Join(",", frameTable.ToArray()) + " };";
            System.IO.File.AppendAllText(filename, code);
        }

        public static string ToImage(List<Vector2[]> lines,int frame,int w,int h,int width,int height,string prefix)
        {
            var f1 = w / (float)width;
            var f2 = h / (float)height;
            var f = Math.Min(f1, f2);
            if (f > 1)
                f = 1;

            var ow = width / 2;
            var oh = height / 2;

            var bmp = new Bitmap(width, height, PixelFormat.Format16bppRgb565);
            var g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, width, height));

            foreach (var line in lines)
            {
                int x1 = (int)(line[0].X * f) + ow;
                int y1 = (int)(line[0].Y * f) + oh;

                int x2 = (int)(line[1].X * f) + ow;
                int y2 = (int)(line[1].Y * f) + oh;

                Point p1 = new Point(x1, y1);
                Point p2 = new Point(x2, y2);

                g.DrawLine(new Pen(new SolidBrush(Color.Red)), p1, p2);
            }

            bmp.RotateFlip(RotateFlipType.Rotate180FlipX);

            var buff = new List<string>();

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var p = bmp.GetPixel(j, i);
                    if (p.Name != "ffffffff")
                    {
                        buff.Add("1");
                    }
                    else
                    {
                        buff.Add("0");
                    }
                }
            }

            g.Dispose();
            bmp.Save(AppDomain.CurrentDomain.BaseDirectory + "/" + prefix + "_" + frame + ".bmp", ImageFormat.Bmp);
            bmp.Dispose();

            return MakeFrameBuff(frame, buff, width, prefix);
        }

        public static string MakeFrameBuff(int frame, List<string> buff, int width,string prefix = "", int pages = 8, int pageSize = 8)
        {
            var ps = new List<string>();
            var p = new List<int>();

            for (int i = 0; i < pages; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var by = new List<string>();
                    for (int m = 0; m < pageSize; m++)
                    {
                        var pos = i * (128 * 8) + width * m + j;
                        p.Add(pos);

                        var v = buff.ElementAt(pos);
                        by.Add(v);
                    }

                    by.Reverse();
                    var s = string.Join("", by);
                    ps.Add("0x" + string.Format("{0:X}", Convert.ToByte(s, 2)));
                }
            }

            return "static unsigned char " + prefix + "_frames_" + frame + "[] = { " + string.Join(",", ps.ToArray()) + "};\n";
        }

        public static Vector2 To2D(Plane plane,Vector3 p)
        {
            var PO = new Vector3(0, 0, 0);
            if (plane.D != 0)
                PO = plane.Normal * plane.D;

            Vector3 vectorX = new Vector3(1, 0, 0);
            Vector3 vectorY = new Vector3(0, 1, 0); // plane.Normal; 
            Vector3 vectorZ = new Vector3(0, 0, 1);

            var q = new Matrix4x4()
            {
                M11 = vectorX.X,
                M12 = vectorY.X,
                M13 = vectorZ.X,
                M14 = PO.X,
                M21 = vectorX.Y,
                M22 = vectorY.Y,
                M23 = vectorZ.Y,
                M24 = PO.Y,
                M31 = vectorX.Z,
                M32 = vectorY.Z,
                M33 = vectorZ.Z,
                M34 = PO.Z,
                M41 = 0,
                M42 = 0,
                M43 = 0,
                M44 = 1
            };

            var qq = Quaternion.CreateFromRotationMatrix(q);

            var np = Vector3.Transform(p, qq);
            

            return new Vector2(np.X,np.Z);
        }
    }
}