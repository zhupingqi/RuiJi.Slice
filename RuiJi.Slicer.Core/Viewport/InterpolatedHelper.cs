using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Slicer.Core.Viewport
{
    public class InterpolatedHelper
    {
        public static Assimp.Vector3D CalcInterpolatedPosition(float animationTime, Assimp.NodeAnimationChannel nodeAnim)
        {
            if (nodeAnim.PositionKeyCount == 1)
            {
                return nodeAnim.PositionKeys[0].Value;
            }

            Assimp.Vector3D result;
            int index = FindPosition(animationTime, nodeAnim);
            int nextIndex = (index + 1);
            float deltaTime = (float)(nodeAnim.PositionKeys[nextIndex].Time - nodeAnim.PositionKeys[index].Time);
            float factor = (animationTime - (float)nodeAnim.PositionKeys[index].Time) / deltaTime;
            Assimp.Vector3D start = nodeAnim.PositionKeys[index].Value;
            Assimp.Vector3D end = nodeAnim.PositionKeys[nextIndex].Value;
            Assimp.Vector3D delta = end - start;
            result = start + factor * delta;
            return result;
        }

        public static int FindPosition(float animationTime, Assimp.NodeAnimationChannel nodeAnim)
        {
            for (int i = 0; i < nodeAnim.PositionKeyCount - 1; i++)
            {
                if (animationTime < (float)nodeAnim.PositionKeys[i + 1].Time)
                {
                    return i;
                }
            }

            return 0;
        }

        public static Assimp.Quaternion CalcInterpolatedRotation(float animationTime, Assimp.NodeAnimationChannel nodeAnim)
        {
            if (nodeAnim.RotationKeyCount == 1)
            {
                return nodeAnim.RotationKeys[0].Value;
            }

            Assimp.Quaternion result;
            int index = FindRotation(animationTime, nodeAnim);
            int nextIndex = (index + 1);
            float deltaTime = (float)(nodeAnim.RotationKeys[nextIndex].Time - nodeAnim.RotationKeys[index].Time);
            float factor = (animationTime - (float)nodeAnim.RotationKeys[index].Time) / deltaTime;
            Assimp.Quaternion start = nodeAnim.RotationKeys[index].Value;
            Assimp.Quaternion end = nodeAnim.RotationKeys[nextIndex].Value;
            result = Interpolate(start, end, factor);
            result.Normalize();
            return result;
        }

        public static int FindRotation(float animationTime, Assimp.NodeAnimationChannel nodeAnim)
        {
            //assert(pNodeAnim->mNumRotationKeys > 0);

            for (int i = 0; i < nodeAnim.RotationKeyCount - 1; i++)
            {
                if (animationTime < (float)nodeAnim.RotationKeys[i + 1].Time)
                {
                    return i;
                }
            }

            //assert(0);

            return 0;
        }

        public static Assimp.Quaternion Interpolate(Assimp.Quaternion start, Assimp.Quaternion end, float factor)
        {
            Assimp.Quaternion result;
            float cosom = start.X * end.X + start.Y * end.Y + start.Z * end.Z + start.W * end.W;

            if (cosom < 0.0f)
            {
                cosom = -cosom;
                end.X = -end.X;   // Reverse all signs
                end.Y = -end.Y;
                end.Z = -end.Z;
                end.W = -end.W;
            }

            float sclp, sclq;
            if (((1.0f) - cosom) > (0.0001f)) // 0.0001 -> some epsillon
            {
                float omega, sinom;
                omega = (float)Math.Acos(cosom); // extract theta from dot product's cos theta
                sinom = (float)Math.Sin(omega);
                sclp = (float)Math.Sin(((1.0f) - factor) * omega) / sinom;
                sclq = (float)Math.Sin(factor * omega) / sinom;
            }
            else
            {
                sclp = (1.0f) - factor;
                sclq = factor;
            }

            result.X = sclp * start.X + sclq * end.X;
            result.Y = sclp * start.Y + sclq * end.Y;
            result.Z = sclp * start.Z + sclq * end.Z;
            result.W = sclp * start.W + sclq * end.W;

            return result;
        }

        public static Assimp.Vector3D CalcInterpolatedScaling(float animationTime, Assimp.NodeAnimationChannel nodeAnim)
        {
            if (nodeAnim.ScalingKeyCount == 1)
            {
                return nodeAnim.ScalingKeys[0].Value;
            }

            Assimp.Vector3D result;
            int index = FindScaling(animationTime, nodeAnim);
            int nextIndex = (index + 1);
            float deltaTime = (float)(nodeAnim.ScalingKeys[nextIndex].Time - nodeAnim.ScalingKeys[index].Time);
            float factor = (animationTime - (float)nodeAnim.ScalingKeys[index].Time) / deltaTime;
            Assimp.Vector3D start = nodeAnim.ScalingKeys[index].Value;
            Assimp.Vector3D end = nodeAnim.ScalingKeys[nextIndex].Value;
            Assimp.Vector3D delta = end - start;
            result = start + factor * delta;
            return result;
        }

        public static int FindScaling(float animationTime, Assimp.NodeAnimationChannel nodeAnim)
        {
            for (int i = 0; i < nodeAnim.ScalingKeyCount - 1; i++)
            {
                if (animationTime < (float)nodeAnim.ScalingKeys[i + 1].Time)
                {
                    return i;
                }
            }

            return 0;
        }
    }
}