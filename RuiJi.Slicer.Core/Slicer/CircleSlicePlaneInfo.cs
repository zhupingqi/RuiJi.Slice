/*
This file is part of RuiJi.Slice: A library for slicing 3D model.
RuiJi.Slice is part of RuiJiHG: RuiJiHG is holographic projection.
see http://www.ruijihg.com/ for more infomation.

Copyright (C) 2017 Pingqi(416803633@qq.com)
Copyright (c) 2017, githublixiang(271800249@qq.com)

RuiJi.Slice is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

RuiJi.Slice is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with wiringPi.
If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Slicer.Core.Slicer
{
    public class CircleSlicePlaneInfo : ISlicePlane
    {
        /// <summary>
        /// 初始面
        /// </summary>
        public Plane Plane
        {
            get;
            private set;
        }

        /// <summary>
        /// 旋转角度
        /// </summary>
        public float Angle
        {
            get;
            private set;
        }

        /// <summary>
        /// 旋转轴
        /// </summary>
        public Vector3 Axis
        {
            get;
            private set;
        }

        public CircleSlicePlaneInfo(Plane plane, Vector3 axis, float angle)
        {
            this.Plane = plane;
            this.Axis = axis;
            this.Angle = angle;
        }
    }
}
