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

        /// <summary>
        /// 根据图像获取机器显示代码
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
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
