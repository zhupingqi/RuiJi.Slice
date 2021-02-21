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

using RuiJi.Slicer.Core.Array;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace RuiJi.Slicer.Core.Slicer
{
    public class SlicerHelper
    {
        public static Dictionary<CircleArrayDefine, List<SlicedPlane>> DoCircleSlice(Facet[] facets, CircleArrayDefine[] defines)
        {
            var results = new Dictionary<CircleArrayDefine,List<SlicedPlane>>();

            foreach (var define in defines)
            {
                var result = new List<SlicedPlane>();

                var factory = new CircleArrayCreater();
                var planes = factory.CreateArrayPlane(define);

                foreach (var p in planes)
                {
                    var planeInfo = p as CircleSlicePlaneInfo;
                    var sp = new SlicedPlane(planeInfo);

                    result.Add(sp);

                    foreach (var f in facets)
                    {
                        var segs = GetPlaneCross(f, planeInfo.Plane);
                        if(segs.Count > 0)
                            sp.Lines.AddRange(segs);
                    }
                }

                results.Add(define,result);
            }

            return results;
        }

        public static Dictionary<LinearArrayDefine, List<SlicedPlane>> DoLinearSlice(Facet[] facets, LinearArrayDefine[] defines)
        {
            var results = new Dictionary<LinearArrayDefine, List<SlicedPlane>>();

            foreach (var define in defines)
            {
                var result = new List<SlicedPlane>();

                var factory = new LinearArrayCreater();
                var planes = factory.CreateArrayPlane(define);

                foreach (var p in planes)
                {
                    var planeInfo = p as LinearSlicePlaneInfo;
                    var sp = new SlicedPlane(planeInfo);

                    result.Add(sp);

                    foreach (var f in facets)
                    {
                        var segs = GetPlaneCross(f, planeInfo.Plane);
                        if (segs.Count > 0)
                            sp.Lines.AddRange(segs);
                    }
                }

                results.Add(define, result);
            }

            return results;
        }

        public static Dictionary<float,List<SlicedPlane>> DoLinearSlice(Facet[] facets, LinearArrayDefine define)
        {
            var result = new List<SlicedPlane>();

            var factory = new LinearArrayCreater();
            var planes = factory.CreateArrayPlane(define);

            foreach (var p in planes)
            {
                var planeInfo = p as LinearSlicePlaneInfo;
                var sp = new SlicedPlane(planeInfo);

                result.Add(sp);

                foreach (var f in facets)
                {
                    var segs = GetPlaneCross(f, planeInfo.Plane);
                    if (segs.Count > 0)
                        sp.Lines.AddRange(segs);
                }
            }

            return result.OrderBy(m => m.D).GroupBy(m => m.D).ToDictionary(m => m.Key, n => n.ToList());
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

        public static float ScaleWeight(Size3D currentBound, Size3D targetbound)
        {
            if(currentBound.X > targetbound.X || currentBound.Y > targetbound.Y || currentBound.Z > targetbound.Z)
            {
                var fx = targetbound.X / currentBound.X;
                var fy = targetbound.Y / currentBound.Y;
                var fz = targetbound.Z / currentBound.Z;

                return (float)Math.Min(fz, Math.Min(fx, fy));
            }
            else
            {
                var fx = targetbound.X / currentBound.X;
                var fy = targetbound.Y / currentBound.Y;
                var fz = targetbound.Z / currentBound.Z;

                return (float)Math.Min(fz, Math.Min(fx, fy));
            }
        }
    }
}