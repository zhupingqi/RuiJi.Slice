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
