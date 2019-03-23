using RuiJi.Slicer.Core.File;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Slicer.Core.Slicer
{
    public class LinearSlicer
    {
        public static List<Bitmap> ToImage(List<SlicedPlane> slicedPlane, ModelSize size, int imageWidth, int imageHeight, int offsetX = 0, int offsetY = 0)
        {
            var firstNormal = (slicedPlane.First().SlicePlane as LinearSlicePlaneInfo).Plane.Normal;
            var images = new List<Bitmap>();

            foreach (var sp in slicedPlane)
            {
                var lines = new List<Vector2[]>();
                var info = sp.SlicePlane as LinearSlicePlaneInfo;

                foreach (var line in sp.Lines)
                {
                    lines.Add(new Vector2[] {
                        new Vector2(line.Start.X,line.Start.Z),
                        new Vector2(line.End.X,line.End.Z)
                    });
                }

                var img = ToImage(lines, size, imageWidth, imageHeight, offsetX, offsetY);
                images.Add(img);
            }

            return images;
        }

        public static Bitmap ToImage(List<Vector2[]> lines, ModelSize size, int imageWidth, int imageHeight, int offsetX = 0, int offsetY = 0)
        {
            var fd = 1f;
            var fw = 1f;
            var fh = 1f;

            if (size.Length > imageWidth)
                fw = imageWidth / (float)size.Length;
            if (size.Height > imageHeight)
                fh = imageHeight / (float)size.Height;
            var f = Math.Min(fd, Math.Min(fw, fh));

            var ow = imageWidth / 2f + offsetX;
            var oh = imageHeight / 2f + offsetY;

            var bmp = new Bitmap(imageWidth, imageHeight);
            var g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, imageWidth, imageHeight));

            foreach (var line in lines)
            {
                int x1 = Convert.ToInt32((line[0].X * f) + ow);
                int y1 = Convert.ToInt32((line[0].Y * f) + oh);

                int x2 = Convert.ToInt32((line[1].X * f) + ow);
                int y2 = Convert.ToInt32((line[1].Y * f) + oh);

                Point p1 = new Point(x1, y1);
                Point p2 = new Point(x2, y2);

                g.DrawLine(new Pen(new SolidBrush(Color.Red)), p1, p2);
            }

            bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
            return bmp;
        }
    }
}
