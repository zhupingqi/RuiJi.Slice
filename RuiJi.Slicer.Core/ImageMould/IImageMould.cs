using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Slicer.Core.ImageMould
{
    public interface IImageMould
    {
        string GetMould(Bitmap bmp);
    }
}
