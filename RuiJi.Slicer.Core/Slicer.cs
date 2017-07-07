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

            foreach (var difine in defines)
            {
                var result = new List<SlicedPlane>();

                var factory = new ArrayFactory();
                var planes = factory.CreatePlane(difine);

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

                results.Add(difine,result);
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