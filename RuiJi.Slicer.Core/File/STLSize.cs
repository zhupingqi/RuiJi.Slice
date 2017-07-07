using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Slicer.Core.File
{
    public class STLSize
    {
        public float Length { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public STLSize(float length,float width,float height)
        {
            this.Length = length;
            this.Width = width;
            this.Height = height;
        }
    }
}
