using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace RuiJi.Slicer.Core
{
    public class PlaneFacet
    {
        public List<Facet> Facets = new List<Facet>();

        public LineSegment ParentLineSegment
        {
            get;
            set;
        }

        public PlaneFacet Parent { get; set; }  

        public int Deep { get; set; }

        public Plane Plane
        {
            get { 
                return this.Facets.First().Plane;
            }
        }

        public List<LineSegment> AroundLines
        {
            get
            {
                var lines = new List<LineSegment>();

                foreach (var facet in Facets)
                {
                    foreach (var line in facet.Lines)
                    {
                        if (CountLine(line) == 1)
                            lines.Add(line);
                    }
                }

                return lines;
            }
        }

        public Vector3 Center
        {
            get
            {
                var x = Facets.Sum(m => m.Center.X) / Facets.Count;
                var y = Facets.Sum(m => m.Center.Y) / Facets.Count;
                var z = Facets.Sum(m => m.Center.Z) / Facets.Count;

                return new Vector3(x, y, z);
            }
        }

        public float Area
        {
            get
            {
                return Facets.Sum(m => m.Area);
            }
        }

        public float Angle
        {
            get
            {
                if(Parent == null)
                    return 0;

                return (float)((float)Math.Acos(Vector3.Dot(this.Plane.Normal, Parent.Plane.Normal)) * 180 / Math.PI);
            }
        }

        public PlaneFacet() 
        { 
            
        }

        public PlaneFacet(Facet facet)
        {            
            Facets.Add(facet);
        }

        public void AddFacet(Facet facet)
        {
            Facets.Add(facet);
        }

        public int CountLine(LineSegment line)
        {
            var c = 0;

            foreach (var facet in Facets)
            {
                c += facet.Lines.Count(m => m.Equals(line));
            }

            return c;
        }

        public LineSegment Collinear(PlaneFacet planeFacet)
        {
            LineSegment line = null;
            foreach (var line0 in AroundLines)
            {
                foreach (var line1 in planeFacet.AroundLines)
                {
                    if(line0.Equals(line1))
                        line = line0;
                }
            }

            return line;
        }

        public void Flatten(int deep = int.MaxValue)
        {
            if(this.Deep > deep)
            {
                return;
            }

            if (ParentLineSegment != null)
            {
                var angle = (float)((Angle-45) * Math.PI / 180.0);
                var axis = Vector3.Normalize(ParentLineSegment.Normal);
                Transform(axis, angle);
            }

            foreach (var line in AroundLines)
            {
                if (line.ChildFacet != null)
                {
                    line.ChildFacet.Flatten(deep);
                }
            }
        }

        public void Transform(Vector3 axis,float angle)
        {
            foreach (var facet in Facets)
            {
                facet.Transform(axis, angle);
            }

            foreach (var line in AroundLines)
            {
                if (line.ChildFacet != null)
                {
                    line.ChildFacet.Transform(axis, angle);
                }
            }
        }
    }
}