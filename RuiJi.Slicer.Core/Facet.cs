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

using RuiJi.Slicer.Core.Slicer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace RuiJi.Slicer.Core
{
    public class Facet
    {
        public Plane Plane
        {
            get {
                return Plane.CreateFromVertices(Vertices[0], Vertices[1], Vertices[2]);
            }
        }

        public IList<Vector3> Vertices
        {
            get;
            private set;
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
            private set;
        }

        public Vector3 Center
        {
            get
            {
                var x = (float)Math.Round((Vertices[0].X + Vertices[1].X + Vertices[2].X) / 3, 2);
                var y = (float)Math.Round((Vertices[0].Y + Vertices[1].Y + Vertices[2].Y) / 3, 2);
                var z = (float)Math.Round((Vertices[0].Z + Vertices[1].Z + Vertices[2].Z) / 3, 2);

                return new Vector3(x, y, z);
            }
        }

        public bool TooSmall
        {
            get
            {
                return Math.Round(Lines[0].Lenght) == 0 && Math.Round(Lines[1].Lenght) == 0 && Math.Round(Lines[2].Lenght) == 0;
            }
        }

        public float Area
        {
            get
            {
                var area = 0f;

                for (int i = 0; i < Vertices.Count; i += 3)
                {
                    area += (float)Math.Round(getPolygonArea(Vertices.Skip(i).Take(3).ToList()), 2);
                }

                return area;
            }
        }

        public Facet(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            v1 = new Vector3((float)Math.Round(v1.X, 2), (float)Math.Round(v1.Y, 2), (float)Math.Round(v1.Z, 2));
            v2 = new Vector3((float)Math.Round(v2.X, 2), (float)Math.Round(v2.Y, 2), (float)Math.Round(v2.Z, 2));
            v3 = new Vector3((float)Math.Round(v3.X, 2), (float)Math.Round(v3.Y, 2), (float)Math.Round(v3.Z, 2));

            this.Vertices = new List<Vector3> {
                v1,
                v2,
                v3
            };

            this.Lines = new List<LineSegment>() {
                    new LineSegment(v1,v2),
                    new LineSegment(v2,v3),
                    new LineSegment(v1,v3)
            };
        }

        public Facet(Point3D p1, Point3D p2, Point3D p3) : this(
            new Vector3((float)Math.Round(p1.X, 2), (float)Math.Round(p1.Y, 2), (float)Math.Round(p1.Z, 2)),
            new Vector3((float)Math.Round(p2.X, 2), (float)Math.Round(p2.Y, 2), (float)Math.Round(p2.Z, 2)),
            new Vector3((float)Math.Round(p3.X, 2), (float)Math.Round(p3.Y, 2), (float)Math.Round(p3.Z, 2))
            )
        {

        }

        public Facet(IList<Vector3> vs) : this(vs[0], vs[1], vs[2])
        {

        }

        public void Merge(Facet facet)
        {
            foreach (var f in facet.Vertices)
            {
                this.Vertices.Add(f);
            }
        }

        private double getPolygonArea(List<Vector3> points)
        {
            var sizep = points.Count();
            if (sizep < 3)
                return 0.0;

            //根号(p*(p-a)*(p-b)*(p-c)) 其中p=(a+b+c)/2.

            var ls0 = new LineSegment(points[0] , points[1]);
            var ls1 = new LineSegment(points[1], points[2]);
            var ls2 = new LineSegment(points[2], points[0]);

            var a = ls0.Lenght;
            var b = ls1.Lenght;
            var c = ls2.Lenght;

            var p = (a + b + c) / 2;
            var s = p * (p - a) * (p - b) * (p - c);
            return Math.Sqrt(s);
        }

        public void Transform(Vector3 axis, float angle)
        {
            var q = System.Numerics.Matrix4x4.CreateFromAxisAngle(axis, angle);

            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = Vector3.Transform(Vertices[i], q);
            }

            var v1 = Vertices[0];
            var v2 = Vertices[1];
            var v3 = Vertices[2];

            //v1 = new Vector3((float)Math.Round(v1.X, 2), (float)Math.Round(v1.Y, 2), (float)Math.Round(v1.Z, 2));
            //v2 = new Vector3((float)Math.Round(v2.X, 2), (float)Math.Round(v2.Y, 2), (float)Math.Round(v2.Z, 2));
            //v3 = new Vector3((float)Math.Round(v3.X, 2), (float)Math.Round(v3.Y, 2), (float)Math.Round(v3.Z, 2));

            this.Lines[0].Start = v1;
            this.Lines[0].End = v2;
            this.Lines[1].Start = v2;
            this.Lines[1].End = v3;
            this.Lines[2].Start = v1;
            this.Lines[2].End = v3;
        }
    }
}