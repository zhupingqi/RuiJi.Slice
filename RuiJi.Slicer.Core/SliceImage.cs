/*
This file is part of RuiJi.Slice: A library for slicing 3D model.
RuiJi.Slice is part of RuiJiHG: RuiJiHG is holographic projection.
see http://www.ruijihg.com/ for more infomation.

Copyright (C) 2017 Pingqi(416803633@qq.com)
Copyright (c) 2017, githublixiang(271800249@qq.com)

RuiJi.Slice is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

RuiJi.Slice is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with wiringPi.
If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using RuiJi.Slicer.Core.File;

namespace RuiJi.Slicer.Core
{
    public class SliceImage
    {
        /// <summary>
        /// 切片转图片
        /// </summary>
        /// <param name="slicedPlane">切片集合</param>
        /// <param name="size">模型尺寸</param>
        /// <param name="imageWidth">图片宽度</param>
        /// <param name="imageHeight">图片高度</param>
        /// <param name="offsetX">x偏移量</param>
        /// <param name="offsetY">y偏移量</param>
        /// <returns></returns>
        public static List<Bitmap> ToImage(List<SlicedPlane> slicedPlane, ModelSize size, int imageWidth, int imageHeight, int offsetX = 0, int offsetY = 0)
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

                var img = ToImage(lines, size, imageWidth, imageHeight, offsetX, offsetY);
                images.Add(img);
            }

            return images;
        }

        /// <summary>
        /// 使用线段绘图
        /// </summary>
        /// <param name="lines">线段集合</param>
        /// <param name="size">模型尺寸</param>
        /// <param name="imageWidth">图片宽度</param>
        /// <param name="imageHeight">图片高度</param>
        /// <param name="offsetX">x偏移量</param>
        /// <param name="offsetY">y偏移量</param>
        /// <returns></returns>
        public static Bitmap ToImage(List<Vector2[]> lines, ModelSize size, int imageWidth, int imageHeight, int offsetX = 0, int offsetY = 0)
        {
            //对角线不应超出宽度
            var fd = 1f;
            //var diagonal = (float)Math.Sqrt(size.Width * size.Width + size.Height * size.Height);
            //if (diagonal > imageWidth)
            //{
                //fd = imageWidth / diagonal;
            //}

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

        public static Vector2 To2D(SlicedPlane sp, Vector3 p)
        {
            var PO = new Vector3(0, 0, 0);
            if (sp.Plane.D != 0)
                PO = sp.Plane.Normal * sp.Plane.D;

            var n = Vector3.Cross(sp.Axis, sp.Plane.Normal);

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


            return new Vector2(np.X, np.Z);
        }
    }
}