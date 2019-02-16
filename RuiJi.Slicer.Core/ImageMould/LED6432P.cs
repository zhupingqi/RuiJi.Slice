using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Slicer.Core.ImageMould
{
    public class LED6432P : IImageMould
    {
        public string GetFrameCode(int prefix, int frameIndex, Bitmap bmp)
        {
            return "uint8_t _" + prefix + "_frame_" + frameIndex + "[] = { " + GetMould(bmp) + " }; \n";
        }

        public string GetFramesCode(int prefix, List<string> frameTable)
        {
            return "uint8_t *frames_table[] = { " + string.Join(",", frameTable.ToArray()) + " };";
        }

        public string GetMould(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;

            if (width != 64 || height != 32)
                throw new Exception("bitmap must be 64*32");

            bmp.RotateFlip(RotateFlipType.Rotate180FlipX);

            var buff = new List<string>();
            var b = new Dictionary<int, List<string>>();

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int pos = Convert.ToInt32(Math.Floor(i / 8f) * 2 + Math.Floor(j / 32f));

                    if (!b.ContainsKey(pos))
                        b.Add(pos, new List<string>());

                    var p = bmp.GetPixel(j, i);
                    if (p.Name == "ffffffff")
                    {
                        b[pos].Add("0");
                    }
                    else
                    {
                        b[pos].Add("1");
                    }
                }
            }

            var c = MakeBuff2(b);

            return c;
        }

        private string MakeBuff(List<string> buff, int width)
        {
            var ps = new List<string>();
            var tmp = new List<string>();
            var s = "";

            for (int b  = 0; b < 8; b++)
            {
                int offsetX = b;
                int offsetY = b;

                for (int row = 0; row < 8; row++)
                {

                }
            }

            for (int i = 0; i < buff.Count; i++)
            {
                var b = buff.ElementAt(i);

                if (tmp.Count == 8)
                {
                    tmp.Reverse();
                    s = string.Join("", tmp);
                    ps.Add("0x" + string.Format("{0:X}", Convert.ToByte(s, 2)));

                    tmp.Clear();
                }

                tmp.Add(b);
            }

            tmp.Reverse();
            s = string.Join("", tmp);
            ps.Add("0x" + string.Format("{0:X}", Convert.ToByte(s, 2)));

            return string.Join(",", ps.ToArray());
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
                if (i % 32 == 0) {
                    var row = Convert.ToInt32(Math.Floor(i / 32f));
                    var t = new List<string>();

                    for (int r = 0; r < 8; r++)
                    {
                        if (r == row) {
                            t.Add("0x0");
                        } else{
                            t.Add("0xFF");
                        }
                    }

                    //t.Reverse();
                    ps.AddRange(t);
                }

                ps.Add("0x" + string.Format("{0:X}", Convert.ToByte(s, 2)));
            }

            return string.Join(",", ps.ToArray());
        }
    }
}
