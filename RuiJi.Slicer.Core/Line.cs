using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Slicer.Core
{
    public class Line
    {
        public Vector2 Start { get; set; }

        public Vector2 End { get; set; }

        public double K
        {
            get
            {
                return (End.Y - Start.Y) / (End.X - End.Y);
            }
        }

        public Line(float x1, float y1, float x2, float y2)
        {
            Start = new Vector2(x1,y1);
            End = new Vector2(x2,y2);
        }

        public Line(double x1, double y1, double x2, double y2):this((float)x1, (float)y1, (float)x2, (float)y2)
        {

        }

        public Line(Vector2 start, Vector2 end)
        {
            this.Start = start;
            this.End = end;
        }
    }
}
