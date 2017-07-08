using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Slicer.Core
{
    public class SlicePanelInfo
    {
        public Plane Plane
        {
            get;
            private set;
        }

        public float Angle
        {
            get;
            private set;
        }

        /// <summary>
        /// 原始旋转轴
        /// </summary>
        public Vector3 Axis
        {
            get;
            private set;
        }

        public SlicePanelInfo(Plane plane, Vector3 axis, float angle)
        {
            this.Plane = plane;
            this.Axis = axis;
            this.Angle = angle;
        }
    }
}
