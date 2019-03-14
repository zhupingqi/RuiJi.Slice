using Assimp;
using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Slicer.Core.Viewport
{
    public class BoneInfo
    {
        public Bone Bone { get; set; }

        public mat4 finalTransformation { get; internal set; }

        public BoneInfo(Bone bone)
        {
            Bone = bone;
            finalTransformation = mat4.Identity;
        }
    }
}