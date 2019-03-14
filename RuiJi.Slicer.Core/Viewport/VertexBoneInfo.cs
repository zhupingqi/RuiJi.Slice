using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Slicer.Core.Viewport
{

    public class VertexBoneInfo
    {
        public string BoneName { get; set; }

        public double Weight { get; set; }

        public VertexBoneInfo(string boneName, double weight)
        {
            this.BoneName = boneName;
            this.Weight = weight;
        }
    }
}
