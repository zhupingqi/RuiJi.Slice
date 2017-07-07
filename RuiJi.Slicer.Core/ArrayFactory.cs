using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Slicer.Core
{
    public class ArrayFactory
    {
        public ArrayDefine ArrayDefine
        {
            get;
            set;
        }

        public ArrayFactory()
        {

        }

        public SlicePanelInfo[] CreatePlane(ArrayDefine define)
        {
            switch (define.ArrayType)
            {
                case ArrayType.Circle:
                    {
                        var creater = new CircleArrayCreater();
                        return creater.CreateArrayPlane(define);
                    }
            }

            return null;
        }
    }
}
