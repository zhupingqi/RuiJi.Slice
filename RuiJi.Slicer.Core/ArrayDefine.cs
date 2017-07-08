using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RuiJi.Slicer.Core
{
    public class ArrayDefine
    {
        /// <summary>
        /// 旋转面
        /// </summary>
        public Plane Plane
        {
            get;
            set;
        }

        /// <summary>
        /// 旋转轴
        /// </summary>
        public Vector3 Axis
        {
            get;
            set;
        }

        public Vector3 Offset
        {
            get;
            set;
        }

        /// <summary>
        /// 切片数量
        /// </summary>
        public int Count
        {
            get;
            set;
        }

        /// <summary>
        /// 切片弧度
        /// </summary>
        public float Angle
        {
            get;
            set;
        }

        /// <summary>
        /// 阵列类型
        /// </summary>
        public ArrayType ArrayType
        {
            get;
            set;
        }

        public ArrayDefine(Plane plane, ArrayType sliceType = ArrayType.Circle , int count = 50, float angle = 360f)
        {
            this.Plane = plane;
            this.ArrayType = sliceType;
            this.Count = count;
            this.Angle = angle;
            this.Axis = new Vector3(0, 0, 1);
        }
    }
}
