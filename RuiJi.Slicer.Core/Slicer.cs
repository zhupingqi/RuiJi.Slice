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
    public class Slicer
    {
        public static Dictionary<ArrayDefine, List<SlicedPlane>> DoSlice(Facet[] facets, ArrayDefine[] defines)
        {
            var results = new Dictionary<ArrayDefine,List<SlicedPlane>>();

            foreach (var define in defines)
            {
                var result = new List<SlicedPlane>();

                var factory = new ArrayFactory();
                var planes = factory.CreatePlane(define);

                foreach (var p in planes)
                {
                    var sp = new SlicedPlane(p.Plane,p.Axis,p.Angle);

                    result.Add(sp);

                    foreach (var f in facets)
                    {
                        var segs = GetPlaneCross(f, p.Plane);
                        if(segs.Count > 0)
                            sp.Lines.AddRange(segs);
                    }
                }

                results.Add(define,result);
            }

            return results;
        }

        public static List<LineSegment> GetPlaneCross(Facet facet, Plane plane)
        {
            var n1 = facet.Plane.Normal;
            var n2 = plane.Normal;

            if (n1.Equals(n2))
            {
                if (facet.Plane.D == plane.D)
                    return facet.Lines;

                return new List<LineSegment>();
            }

            var points = new List<Vector3>();

            foreach (var line in facet.Lines)
            {
                var p = IntersectPoint(plane, line);
                if (p != null)
                    points.Add(p.Value);
            }

            if (points.Count != 2)
                return new List<LineSegment>();

            return new List<LineSegment>() {
                new LineSegment(points[0],points[1])
            };
        }

        public static Vector3? IntersectPoint(Plane plane, LineSegment line)
        {
            Vector3 LN = line.Normal;
            Vector3 PN = plane.Normal;

            float s = PN.X * LN.X + PN.Y * LN.Y + PN.Z * LN.Z;

            if (s == 0.0)
                return null;

            Vector3 LP = line.Start;
            Vector3 PP = new Vector3(0, 0, 0);
            if (plane.D != 0)
                PP = PN * plane.D;
            double D = plane.D;

            var t = ((PP.X - LP.X) * PN.X + (PP.Y - LP.Y) * PN.Y + (PP.Z - LP.Z) * PN.Z) / s;

            var x = LP.X + (float)t * LN.X;
            var y = LP.Y + (float)t * LN.Y;
            var z = LP.Z + (float)t * LN.Z;

            var point = new Vector3(x,y,z);

            if (point.X < Math.Min(line.Start.X, line.End.X) || point.X > Math.Max(line.Start.X, line.End.X))
                return null;
            if (point.Y < Math.Min(line.Start.Y, line.End.Y) || point.Y > Math.Max(line.Start.Y, line.End.Y))
                return null;
            if (point.Z < Math.Min(line.Start.Z, line.End.Z) || point.Z > Math.Max(line.Start.Z, line.End.Z))
                return null;

            return point;
        }
    }
}