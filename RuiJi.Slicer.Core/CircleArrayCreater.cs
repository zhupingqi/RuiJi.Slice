using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Slicer.Core
{
    public class CircleArrayCreater : IArrayCreater
    {
        public SlicePanelInfo[] CreateArrayPlane(ArrayDefine define)
        {
            var planes = new List<SlicePanelInfo>();

            var step = define.Angle / define.Count;
            for (int i = 0; i < define.Count; i++)
            {
                var angle = i * step * (float)Math.PI / 180f;
                var m = Matrix4x4.CreateFromAxisAngle(define.Axis,angle);
                
                var p = Plane.Transform(define.Plane, m);
                planes.Add(new SlicePanelInfo(p,define.Axis, angle));
            }

            return planes.ToArray();
        }
    }
}