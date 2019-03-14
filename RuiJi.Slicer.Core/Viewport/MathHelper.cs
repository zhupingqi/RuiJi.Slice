using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Slicer.Core.Viewport
{
    public static class MathHelper
    {
        public static System.Numerics.Matrix4x4 ToSysMatrix4(this Assimp.Matrix4x4 matrix)
        {
            return new System.Numerics.Matrix4x4(
                matrix.A1, matrix.B1, matrix.C1, matrix.D1,
                matrix.A2, matrix.B2, matrix.C2, matrix.D2,
                matrix.A3, matrix.B3, matrix.C3, matrix.D3,
                matrix.A4, matrix.B4, matrix.C4, matrix.D4
            );
        }

        public static mat4 ToMat4(this Assimp.Matrix4x4 matrix)
        {
            return new mat4(
                new vec4(matrix.A1, matrix.B1, matrix.C1, matrix.D1),
                new vec4(matrix.A2, matrix.B2, matrix.C2, matrix.D2),
                new vec4(matrix.A3, matrix.B3, matrix.C3, matrix.D3),
                new vec4(matrix.A4, matrix.B4, matrix.C4, matrix.D4)
            );
        }
    }
}
