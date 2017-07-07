using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RuiJi.Slicer.Core
{
    public class LineSegment
    {
        public Vector3 Start
        {
            get;
            private set;
        }

        public Vector3 End
        {
            get;
            private set;
        }

        public Vector3 Normal
        {
            get;
            private set;
        }

        public LineSegment(Vector3 a, Vector3 b)
        {
            this.Start = a;
            this.End = b;

            this.Normal = Vector3.Normalize(End - Start);
        }
    }
}
