using RuiJi.Slicer.Core.Slicer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Slicer.Core.Array
{
    public class LinearArrayCreater : IArrayCreater<LinearArrayDefine>
    {
        public ISlicePlane[] CreateArrayPlane(LinearArrayDefine define)
        {
            var planes = new List<ISlicePlane>();

            var step = (define.Dmax - define.Dmin) / define.Count;
            for (int i = 0; i < define.Count; i++)
            {
                var p = new Plane(define.Normal, define.Dmin + step * i);

                planes.Add(new LinearSlicePlaneInfo(p));
            }

            return planes.ToArray();
        }
    }
}
