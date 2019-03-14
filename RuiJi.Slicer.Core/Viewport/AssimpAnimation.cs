using Assimp;
using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace RuiJi.Slicer.Core.Viewport
{
    public class AssimpAnimation
    {
        private Scene aiScene;
        private Dictionary<string, BoneInfo> allBones = new Dictionary<string, BoneInfo>();
        private Dictionary<int, Dictionary<int, List<VertexBoneInfo>>> meshVertexBoneInfos = new Dictionary<int, Dictionary<int, List<VertexBoneInfo>>>();
        private mat4 globalInverseTransform;
        private bool isCache = false;
        private Dictionary<int, Mesh> defaultCache = new Dictionary<int, Mesh>();

        public AssimpAnimation(Scene scene)
        {
            this.aiScene = scene;
            var mat = scene.RootNode.Transform;
            mat.Inverse();
            globalInverseTransform = mat.ToMat4();

            Init();
        }

        private void Init()
        {
            for (int meshId = 0; meshId < aiScene.MeshCount; meshId++)
            {
                var mesh = aiScene.Meshes[meshId];
                mesh.Bones.ForEach(b => {
                    if (!allBones.ContainsKey(b.Name))
                        allBones.Add(b.Name, new BoneInfo(b));
                });

                meshVertexBoneInfos.Add(meshId, new Dictionary<int, List<VertexBoneInfo>>());
                var vertexBoneInfos = meshVertexBoneInfos[meshId];

                mesh.Bones.ForEach(bone => {
                    bone.VertexWeights.ForEach(vertexWeight => {
                        if (!vertexBoneInfos.ContainsKey(vertexWeight.VertexID))
                            vertexBoneInfos.Add(vertexWeight.VertexID, new List<VertexBoneInfo>());

                        vertexBoneInfos[vertexWeight.VertexID].Add(new VertexBoneInfo(bone.Name, vertexWeight.Weight));
                    });
                });
            }
        }

        public void Render(Model3DGroup meshs, Animation animation, float time)
        {
            time = time % (float)animation.DurationInTicks;
            TransformNode(time, aiScene.RootNode, animation, globalInverseTransform);

            for (int meshId = 0; meshId < aiScene.MeshCount; meshId++)
            {
                if (!meshVertexBoneInfos.ContainsKey(meshId))
                    continue;

                var mesh = aiScene.Meshes[meshId];
                var mesh3D = (meshs.Children[meshId] as GeometryModel3D).Geometry as MeshGeometry3D;
                var vertextBoneWeights = meshVertexBoneInfos[meshId];

                for (int vertexId = 0; vertexId < mesh.VertexCount; vertexId++)
                {
                    var ver = mesh.Vertices[vertexId];
                    if (vertextBoneWeights.ContainsKey(vertexId))
                    {
                        var vertexWeights = vertextBoneWeights[vertexId];

                        var boneTransform = mat4.Identity;

                        vertexWeights.ForEach(info =>
                        {
                            boneTransform += allBones[info.BoneName].finalTransformation * (float)info.Weight;
                        });

                        var animPosition = new vec4(ver.X, ver.Y, ver.Z, 1.0f);
                        animPosition = boneTransform * animPosition;

                        mesh3D.Positions[vertexId] = new Point3D(animPosition.x, animPosition.y, animPosition.z);
                    }
                    else
                    {
                        mesh3D.Positions[vertexId] = new Point3D(ver.X, ver.Y, ver.Z);
                    }
                }
            }
        }

        private void TransformNode(float time, Assimp.Node aiNode, Animation animation, mat4 parentTransform)
        {
            var name = aiNode.Name;
            var nodeTransform = aiNode.Transform.ToMat4();

            var animationChannel = animation.NodeAnimationChannels.SingleOrDefault(m => m.NodeName == name);

            if (animationChannel != null)
            {
                var scaling = InterpolatedHelper.CalcInterpolatedScaling(time, animationChannel);
                var scalingM = mat4.Scale(scaling.X, scaling.Y, scaling.Z);

                var rotation = InterpolatedHelper.CalcInterpolatedRotation(time, animationChannel).GetMatrix();
                var rotationM = new Assimp.Matrix4x4(rotation).ToMat4();

                var translation = InterpolatedHelper.CalcInterpolatedPosition(time, animationChannel);
                var translationM = mat4.Translate(translation.X, translation.Y, translation.Z);

                nodeTransform = translationM * rotationM * scalingM;
            }

            var globalTransform = parentTransform * nodeTransform;

            if(allBones.ContainsKey(name))
            {
                allBones[name].finalTransformation = globalTransform * allBones[name].Bone.OffsetMatrix.ToMat4();
            }

            for (int i = 0; i < aiNode.ChildCount; i++)
            {
                TransformNode(time, aiNode.Children[i], animation, globalTransform);
            }
        }
    }
}