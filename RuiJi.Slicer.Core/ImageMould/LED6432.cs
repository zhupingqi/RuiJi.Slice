using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Slicer.Core.ImageMould
{
    public class LED6432 : IImageMould
    {
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
                        buff.Add("0");
                    }
                    else
                    {
                        buff.Add("1");
                    }
                }
            }

            return MakeBuff(buff, width);
        }

        private string MakeBuff(List<string> buff, int width)
        {
            var ps = new List<string>();
            var tmp = new List<string>();
            var s = "";

            for (int i = 0; i < buff.Count; i++)
            {
                var b = buff.ElementAt(i);

                if (tmp.Count == 8)
                {
                    //tmp.Reverse();
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

        public string GetFrameCode(int prefix,int frameIndex,Bitmap bmp)
        {
            return "uint8_t _" + prefix + "_frame_" + frameIndex + "[] = { " + GetMould(bmp) + " }; \n";
        }

        public string GetFramesCode(int prefix, List<string> frameTable)
        {
            return "uint8_t *frames_table[] = { " + string.Join(",", frameTable.ToArray()) + " };";
        }
    }
}