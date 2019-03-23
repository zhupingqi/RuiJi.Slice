using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Slicer.Core.Array
{
    public class LinearArrayDefine : ArrayBase
    {

        public float Dmin
        {
            get;
            set;
        }

        public float Dmax
        {
            get;
            set;
        }

        /// <summary>
        /// 切平面法向量
        /// </summary>
        public Vector3 Normal
        {
            get;
            set;
        }

        public LinearArrayDefine(Vector3 normal,int count,float dmin,float dmax)
        {
            this.Normal = normal;
            this.Count = count;
            this.Dmin = dmin;
            this.Dmax = dmax;
        }
    }
}
