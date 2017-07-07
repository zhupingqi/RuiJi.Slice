using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Slicer.Core.ImageMould
{
    public class SSD1306 : IImageMould
    {
        public int Pages
        {
            get;
            set;
        }

        public int PageSize
        {
            get;
            set;
        }

        public SSD1306()
        {
            Pages = 8;
            PageSize = 8;
        }

        public string GetMould(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;

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

            return MakeBuff(buff, width);
        }

        private string MakeBuff(List<string> buff, int width)
        {
            var ps = new List<string>();
            var p = new List<int>();

            for (int i = 0; i < Pages; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var by = new List<string>();
                    for (int m = 0; m < PageSize; m++)
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

            return string.Join(",", ps.ToArray());
        }
    }
}
