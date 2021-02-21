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
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RuiJi.Slicer.Core.Slicer
{
    public class SlicedPlane
    {
        public List<LineSegment> Lines
        {
            get;
            set;
        }

        public ISlicePlane SlicePlane
        {
            get;
            set;
        }

        public float D
        {
            get
            {
                return this.SlicePlane.Plane.D;
            }
        }

        public SlicedPlane(ISlicePlane slicePlane)
        {
            this.Lines = new List<LineSegment>();
            this.SlicePlane = slicePlane;
        }
    }
}