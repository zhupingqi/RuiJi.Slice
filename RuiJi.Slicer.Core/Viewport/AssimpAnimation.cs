using Assimp;
using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace RuiJi.Slicer.Core.Viewport
{
    public class AssimpAnimation
    {
        private Scene aiScene;
        private Dictionary<string, BoneInfo> allBones = new Dictionary<string, BoneInfo>();
        private Dictionary<int, Dictionary<int, List<VertexBoneInfo>>> meshVertexBoneInfos = new Dictionary<int, Dictionary<int, List<VertexBoneInfo>>>();
        private mat4 globalInverseTransform;

        public Transform3D Transform { get; set; }

        public Model3DGroup BaseModel { get; private set; }

        public Animation animation { get; private set; }

        private Dictionary<float, Model3DGroup> cache = new Dictionary<float, Model3DGroup>();

        public Rect3D Bounds { get; private set; }

        public AssimpAnimation(Scene scene,string animationName)
        {
            this.aiScene = scene;
            var mat = scene.RootNode.Transform;
            mat.Inverse();
            globalInverseTransform = mat.ToMat4();
            animation = aiScene.Animations.SingleOrDefault(m => m.Name == animationName);
            Bounds = new Rect3D();

            Init();

            PreRender();
        }

        private void Init()
        {
            if (!aiScene.HasMeshes)
                return;

            BaseModel = new Model3DGroup();

            var material = new DiffuseMaterial(new SolidColorBrush(Colors.SandyBrown));
            material.AmbientColor = Colors.SaddleBrown;

            for (int meshId = 0; meshId < aiScene.MeshCount; meshId++)
            {
                var aiMesh = aiScene.Meshes[meshId];
                aiMesh.Bones.ForEach(b => {
                    if (!allBones.ContainsKey(b.Name))
                        allBones.Add(b.Name, new BoneInfo(b));
                });

                meshVertexBoneInfos.Add(meshId, new Dictionary<int, List<VertexBoneInfo>>());
                var vertexBoneInfos = meshVertexBoneInfos[meshId];

                aiMesh.Bones.ForEach(bone => {
                    bone.VertexWeights.ForEach(vertexWeight => {
                        if (!vertexBoneInfos.ContainsKey(vertexWeight.VertexID))
                            vertexBoneInfos.Add(vertexWeight.VertexID, new List<VertexBoneInfo>());

                        vertexBoneInfos[vertexWeight.VertexID].Add(new VertexBoneInfo(bone.Name, vertexWeight.Weight));
                    });
                });

                var geoModel = new GeometryModel3D();
                geoModel.Material = material;
                var mesh = new MeshGeometry3D();

                aiMesh.Vertices.ForEach(m =>
                {
                    mesh.Positions.Add(new Point3D(m.X, m.Y, m.Z));
                });

                aiMesh.Normals.ForEach(m =>
                {
                    mesh.Normals.Add(new System.Windows.Media.Media3D.Vector3D(m.X, m.Y, m.Z));
                });

                aiMesh.Faces.ForEach(m =>
                {
                    mesh.TriangleIndices.Add(m.Indices[0]);
                    mesh.TriangleIndices.Add(m.Indices[1]);
                    mesh.TriangleIndices.Add(m.Indices[2]);

                    if (m.IndexCount == 4)
                    {
                        mesh.TriangleIndices.Add(m.Indices[2]);
                        mesh.TriangleIndices.Add(m.Indices[3]);
                        mesh.TriangleIndices.Add(m.Indices[0]);
                    }
                });

                geoModel.Geometry = mesh;
                BaseModel.Children.Add(geoModel);
            }

            //var b = MeshGroup.Bounds;
            //var center = new Point3D((b.X + b.SizeX / 2), (b.Y + b.SizeY / 2), (b.Z + b.SizeZ / 2));
            //var radius = (center - b.Location).Length;

            //var s = 32 / radius;
            //if (b.SizeZ * s > 64)
            //{
            //    s = 32 / b.SizeZ;
            //}

            //var model = new ModelVisual3D();
            //model.Content = MeshGroup;

            //var g = new Transform3DGroup();
            //g.Children.Add(new TranslateTransform3D(-center.X, -center.Y, -center.Z));
            //g.Children.Add(new ScaleTransform3D(s, s, s));
            //g.Children.Add(new RotateTransform3D(rotation3D));
        }

        public Model3DGroup Render(float time)
        {
            var meshs = BaseModel.Clone();

            time = time % (float)animation.DurationInTicks;
            TransformNode(time, aiScene.RootNode, globalInverseTransform);

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

                        var sum = vertexWeights.Sum(m=>m.Weight);

                        vertexWeights.ForEach(info =>
                        {
                            boneTransform += allBones[info.BoneName].finalTransformation * (float)(info.Weight / sum);
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

            return meshs;
        }

        public Model3DGroup GetCache(float time)
        {
            if (!cache.ContainsKey(time))
            {
                PreRender(time);
            }

            return cache[time];
        }

        private void PreRender(float? time = null)
        {
            if (!time.HasValue)
            {
                for (float tick = 0; tick < animation.DurationInTicks; tick++)
                {
                    TransformNode(tick, aiScene.RootNode, globalInverseTransform);

                    var model = Render(tick);

                    cache.Add(tick, model);

                    Bounds = Rect3D.Union(Bounds, model.Bounds);
                }
            }
            else
            {
                TransformNode(time.Value, aiScene.RootNode, globalInverseTransform);

                var model = Render(time.Value);

                cache.Add(time.Value, model);
            }
        }

        private void TransformNode(float time, Node aiNode, mat4 parentTransform)
        {
            var name = aiNode.Name;
            var nodeTransform = aiNode.Transform.ToMat4();

            var animationChannel = animation.NodeAnimationChannels.SingleOrDefault(m => m.NodeName == name);

            if (animationChannel != null)
            {
                var scaling = InterpolatedHelper.CalcInterpolatedScaling(time, animationChannel);
                var scalingM = mat4.Scale(scaling.X, scaling.Y, scaling.Z);
                scalingM = mat4.Identity;

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
                TransformNode(time, aiNode.Children[i], globalTransform);
            }
        }
    }
}