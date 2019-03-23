using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Slicer.Core.Slicer
{
    public class LinearSlicePlaneInfo : ISlicePlane
    {
        public Plane Plane
        {
            get;
            private set;
        }

        public LinearSlicePlaneInfo(Plane plane)
        {
            this.Plane = plane;
        }
    }
}
