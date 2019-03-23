using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace RuiJi.Slicer.Core.ImageMould
{
    public class LED6432LP : IImageMould
    {
        public string GetFrameCode(int prefix, int frameIndex, Bitmap bmp)
        {
            throw new NotImplementedException();
        }

        public string GetFramesCode(int prefix, List<string> frameTable)
        {
            throw new NotImplementedException();
        }

        public string GetMould(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;

            if (width != 64 || height != 64)
                throw new Exception("bitmap must be 64*64");

            bmp.RotateFlip(RotateFlipType.Rotate180FlipX);

            var b = new List<string>();

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (IsInCircle(32, 32, 32, i, j))
                    {
                        var p = bmp.GetPixel(j, i);
                        if (p.Name == "ffffffff")
                        {
                            b.Add("0");
                        }
                        else
                        {
                            b.Add("1");
                        }
                    }
                }
            }

            return string.Join(",",b.ToArray());

            //var c = MakeBuff2(b);

           // return c;
        }

        private string MakeBuff2(Dictionary<int, List<string>> buff)
        {
            var ps = new List<string>();

            for (int i = 0; i < 256; i++)
            {
                var tmp = new List<string>();
                for (int j = 0; j < 8; j++)
                {
                    tmp.Add(buff[j].ElementAt(i));
                }
                var s = string.Join("", tmp);
                if (i % 32 == 0)
                {
                    var row = Convert.ToInt32(Math.Floor(i / 32f));
                    var t = new List<string>();

                    for (int r = 0; r < 8; r++)
                    {
                        if (r == row)
                        {
                            t.Add("0x00");
                        }
                        else
                        {
                            t.Add("0xFF");
                        }
                    }

                    //t.Reverse();
                    ps.AddRange(t);
                }

                ps.Add("0x" + string.Format("{0:X2}", Convert.ToByte(s, 2)));
            }

            return string.Join(",", ps.ToArray());
        }

        private bool IsInCircle(float x0, float y0,float r,float x, float y)
        {
            return Math.Sqrt((x - x0) * (x - x0) + (y - y0) * (y - y0)) <= r;
        }
    }
}
