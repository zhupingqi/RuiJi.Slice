using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RuiJi.Slicer.Core
{
    public class SlicedPlane
    {
        public Plane Plane
        {
            get;
            set;
        }

        public List<LineSegment> Lines
        {
            get;
            set;
        }

        public float Angle
        {
            get;
            private set;
        }

        public Vector3 Axis
        {
            get;
            private set;
        }

        public SlicedPlane(Plane plane,Vector3 axis ,float angle)
        {
            this.Plane = plane;
            this.Lines = new List<LineSegment>();
            this.Axis = axis;
            this.Angle = angle;
        }
    }
}