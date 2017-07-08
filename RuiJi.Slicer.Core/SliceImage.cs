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
        public static List<Bitmap> ToImage(List<SlicedPlane> slicedPlane,int w,int h,int width,int height)
        {
            var firstNormal = slicedPlane.First().Plane.Normal;
            var images = new List<Bitmap>();

            foreach (var sp in slicedPlane)
            {
                var lines = new List<Vector2[]>();

                var origin = new Vector3(0, 0, 0);
                if (sp.Plane.D != 0)
                    origin = firstNormal * sp.Plane.D;

                var a = -sp.Angle;
                var q = Matrix4x4.CreateFromAxisAngle(sp.Axis, a);
                q.Translation = -origin;

                foreach (var line in sp.Lines)
                {
                    var s = Vector3.Transform(line.Start, q);
                    var e = Vector3.Transform(line.End, q);

                    //var s = To2D(sp,line.Start);
                    //var e = To2D(sp,line.End);

                    lines.Add(new Vector2[] {
                        new Vector2(s.X,s.Z),
                        new Vector2(e.X,e.Z)
                    });
                }

                var img = ToImage(lines, w, h, width, height);
                images.Add(img);
            }

            return images;
        }

        public static Bitmap ToImage(List<Vector2[]> lines,int w,int h,int width,int height)
        {
            var f1 = w / (float)width;
            var f2 = h / (float)height;
            var f = Math.Min(f1, f2);
            if (f > 1)
                f = 1;

            var ow = width / 2;
            var oh = height / 2;

            var bmp = new Bitmap(width, height);
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
            return bmp;
        }

        public static Vector2 To2D(SlicedPlane sp, Vector3 p)
        {
            var PO = new Vector3(0, 0, 0);
            if (sp.Plane.D != 0)
                PO = sp.Plane.Normal * sp.Plane.D;

            var n = Vector3.Cross( sp.Axis , sp.Plane.Normal);

            Vector3 vectorX = new Vector3(sp.Axis.X, 0, 0);
            Vector3 vectorY = new Vector3(0, sp.Axis.Y, 0); // plane.Normal; 
            Vector3 vectorZ = new Vector3(0, 0, sp.Axis.Z);

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