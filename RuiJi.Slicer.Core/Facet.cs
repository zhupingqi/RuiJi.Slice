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

namespace RuiJi.Slicer.Core
{
    public class Facet
    {
        public Plane Plane
        {
            get;
            private set;
        }

        public IList<Vector3> Vertices
        {
            get;
            internal set;
        }

        /// <summary>
        /// 法线
        /// </summary>
        public Vector3 Normal
        {
            get
            {
                return Plane.Normal;
            }
        }

        public List<LineSegment> Lines
        {
            get;
            internal set;
        }

        public bool TooSmall
        {
            get
            {
                return Math.Round(Lines[0].Lenght) == 0 || Math.Round(Lines[1].Lenght) == 0 || Math.Round(Lines[2].Lenght) == 0;
            }
        }

        public Facet(Vector3 v1,Vector3 v2,Vector3 v3 )
        {
            this.Vertices = new Vector3[] {
                v1,
                v2,
                v3
            };
            this.Plane = Plane.CreateFromVertices(v1, v2, v3);
            this.Lines = new List<LineSegment>() {
                new LineSegment(v1,v2),
                new LineSegment(v1,v3),
                new LineSegment(v2,v3)
            };
        }

        public Facet(IList<Vector3> vs) : this(vs[0],vs[1],vs[2])
        {

        }
    }
}
