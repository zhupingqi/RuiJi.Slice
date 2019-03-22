﻿using Assimp;
using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace RuiJi.Slicer.Core.Viewport
{
    public class SceneView
    {
        private Viewport3D viewport3D;
        private AssimpContext assimpContext;
        private Scene aiScene;
        private Task aniTask;
        private CancellationTokenSource cancellationToken;
        private bool replaceCamera = false;

        public Model3DGroup MeshGroup { get; private set; }
        public DirectionalLightCamera LightCamera { get; private set; }

        public string AnimationName { get; private set; }

        public bool HasAnimations
        {
            get
            {
                return aiScene.HasAnimations;
            }
        }

        public List<string> Animations
        {
            get
            {
                return aiScene.Animations.Select(m => m.Name + "@" + m.DurationInTicks).ToList();
            }
        }

        public SceneView(Viewport3D viewport3D)
        {
            MeshGroup = new Model3DGroup();

            assimpContext = new AssimpContext();
            this.viewport3D = viewport3D;

            LightCamera = new DirectionalLightCamera(this.viewport3D);
            this.viewport3D.Camera = LightCamera.Camera;
            LightCamera.Lookat(new Point3D(), 100);

            var modelLight = new ModelVisual3D();
            var lightGroup = new Model3DGroup();
            lightGroup.Children.Add(LightCamera.Light);

            var ambientLight = new AmbientLight(Colors.White);
            lightGroup.Children.Add(ambientLight);

            modelLight.Content = lightGroup;
            this.viewport3D.Children.Add(modelLight);

            GroundPlane.DrawGround(viewport3D);
        }

        public bool Load(string file)
        {
            for (int i = 2; i < viewport3D.Children.Count; i++)
            {
                viewport3D.Children.RemoveAt(i);
            }

            Stop();

            try
            {
                aiScene = assimpContext.ImportFile(file);
                var rotation = new AxisAngleRotation3D();
                if (file.EndsWith(".stl"))
                {
                    rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(1, 0, 0), -90);
                }

                RenderMesh(rotation);

                if (aiScene.HasAnimations)
                {
                    Play();
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return false;
        }

        public void Play(string name = "")
        {
            if (string.IsNullOrEmpty(name))
                name = aiScene.Animations[0].Name;

            AnimationName = name;

            Stop();

            cancellationToken = new CancellationTokenSource();
            replaceCamera = true;

            aniTask = new Task(() =>
            {
                var anim = new Viewport.AssimpAnimation(aiScene);

                while (!cancellationToken.Token.IsCancellationRequested)
                {
                    var ani = aiScene.Animations.SingleOrDefault(m => m.Name == AnimationName);

                    if (ani == null)
                        break;

                    for (float tick = 0; tick < ani.DurationInTicks; tick++)
                    {
                        if (cancellationToken.Token.IsCancellationRequested)
                            break;

                        viewport3D.Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                var transform = viewport3D.Children[2].Transform as Transform3DGroup;
                                anim.Render(MeshGroup, ani, tick);

                                if (replaceCamera)
                                {
                                    LightCamera.Lookat(new Point3D(0, 0, 0), 64);

                                    replaceCamera = false;
                                }
                            }
                            catch
                            {
                                cancellationToken.Cancel();
                            }
                        });

                        Thread.Sleep((int)ani.TicksPerSecond);
                    }
                }
            }, cancellationToken.Token);

            aniTask.Start();
        }

        public Model3DGroup GetAnimationTick(string name,int tick)
        {
            var anim = new Viewport.AssimpAnimation(aiScene);
            var ani = aiScene.Animations.SingleOrDefault(m => m.Name == name);
            var cloneMesh = MeshGroup.Clone();
            anim.Render(cloneMesh, ani, tick);

            return cloneMesh;
        }

        public int GetAnimationTicks(string name)
        {
            var ani = aiScene.Animations.SingleOrDefault(m => m.Name == name);
            return (int)ani.DurationInTicks;
        }

        public void Stop()
        {
            if (cancellationToken != null)
            {
                cancellationToken.Cancel();

                while (true)
                {
                    if (aniTask.Status != TaskStatus.Running)
                        break;

                    Thread.Sleep(100);
                }
            }
        }

        private void RenderMesh(Rotation3D rotation3D)
        {
            MeshGroup.Children.Clear();

            if (!aiScene.HasMeshes)
                return;

            var material = new DiffuseMaterial(new SolidColorBrush(Colors.SandyBrown));
            material.AmbientColor = Colors.SaddleBrown;

            foreach (var aiMesh in aiScene.Meshes)
            {
                var geoModel = new GeometryModel3D();
                geoModel.Material = material;
                //geoModel.BackMaterial = material;
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
                MeshGroup.Children.Add(geoModel);
            }

            var b = MeshGroup.Bounds;
            var center = new Point3D((b.X + b.SizeX / 2), (b.Y + b.SizeY / 2), (b.Z + b.SizeZ / 2));
            var radius = (center - b.Location).Length;

            var s = 32 / radius;
            if (b.SizeZ * s > 64)
            {
                s = 32 / b.SizeZ;
            }

            var model = new ModelVisual3D();
            model.Content = MeshGroup;

            var g = new Transform3DGroup();            
            g.Children.Add(new TranslateTransform3D(-center.X, -center.Y, -center.Z));
            g.Children.Add(new ScaleTransform3D(s, s, s));
            g.Children.Add(new RotateTransform3D(rotation3D));

            viewport3D.Children.Add(model);
            viewport3D.Children[2].Transform = g;

            LightCamera.Lookat(new Point3D(0, 0, 0), 64);
        }
    }
}