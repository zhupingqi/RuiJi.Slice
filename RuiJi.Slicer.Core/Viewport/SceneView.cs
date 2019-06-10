using Assimp;
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
        private Scene aiScene;
        private Thread aniThread;
        private AssimpAnimation assimpAnimation;

        public Model3DGroup MeshGroup { get; private set; }
        public DirectionalLightCamera LightCamera { get; private set; }

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
                var assimpContext = new AssimpContext();
                aiScene = assimpContext.ImportFile(file);
                var rotation = new AxisAngleRotation3D();
                if (file.EndsWith(".stl"))
                {
                    rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(1, 0, 0), -90);
                }

                if (aiScene.HasAnimations)
                {
                    viewport3D.Children.Add(new ModelVisual3D());
                    Play();
                }
                else
                {
                    RenderMesh(rotation);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return false;
        }

        public void Rotate(System.Windows.Media.Media3D.Vector3D axis, double angle)
        {
            var m = viewport3D.Children[viewport3D.Children.Count - 1].Transform as Transform3DGroup;
            var _rotation = (m.Children[2] as RotateTransform3D).Rotation as AxisAngleRotation3D;

            var q = new System.Windows.Media.Media3D.Quaternion(_rotation.Axis, _rotation.Angle);
            var delta = new System.Windows.Media.Media3D.Quaternion(axis, angle);
            q *= delta;

            _rotation.Axis = q.Axis;
            _rotation.Angle = q.Angle;

            m.Children[2] = new RotateTransform3D(_rotation);
        }

        public void Play(string name = "")
        {
            if (string.IsNullOrEmpty(name))
                name = aiScene.Animations[0].Name;

            Stop();

            var replaceCamera = true;

            aniThread = new Thread(() => {
                viewport3D.Dispatcher.Invoke(() =>
                {
                    assimpAnimation = new AssimpAnimation(aiScene, name);
                });

                while (true)
                {
                    var ani = aiScene.Animations.SingleOrDefault(m => m.Name == name);

                    if (ani == null)
                        break;

                    for (float tick = 0; tick < ani.DurationInTicks; tick++)
                    {
                        try
                        {
                            viewport3D.Dispatcher.Invoke(() =>
                            {
                                var transform = viewport3D.Children[2].Transform as Transform3DGroup;
                                var model = viewport3D.Children[2] as ModelVisual3D;
                                var aniModel = assimpAnimation.GetCache(tick);
                                model.Content = aniModel.Clone();

                                if (replaceCamera)
                                {
                                    viewport3D.Children[2].Transform = GetTransform(assimpAnimation.Bounds, new AxisAngleRotation3D());

                                    LightCamera.Lookat(new Point3D(0, 0, 0), 64);

                                    replaceCamera = false;
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            
                        }

                        Thread.Sleep((int)1000/(int)ani.TicksPerSecond);
                    }
                }
            });
            
            aniThread.Start();
        }

        public void Stop()
        {
            if (aniThread != null)
            {
                aniThread.Abort();
                aniThread = null;
            }
        }

        public Model3DGroup GetAnimationTick(double tick)
        {
            return assimpAnimation.GetCache(tick);
        }

        public Animation GetAnimationTicks(string name)
        {
            return aiScene.Animations.SingleOrDefault(m => m.Name == name);
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

            var model = new ModelVisual3D();
            model.Content = MeshGroup;
            viewport3D.Children.Add(model);
            viewport3D.Children[2].Transform = GetTransform(MeshGroup.Bounds,rotation3D);

            LightCamera.Lookat(new Point3D(0, 0, 0), 64);
        }

        private Transform3DGroup GetTransform(Rect3D bounds,Rotation3D rotation3D)
        {
            var center = new Point3D((bounds.X + bounds.SizeX / 2), (bounds.Y + bounds.SizeY / 2), (bounds.Z + bounds.SizeZ / 2));
            var radius = (center - bounds.Location).Length;

            var s = 32 / radius;
            if (bounds.SizeZ * s > 64)
            {
                s = 32 / bounds.SizeZ;
            }

            var model = new ModelVisual3D();
            model.Content = MeshGroup;

            var g = new Transform3DGroup();
            g.Children.Add(new TranslateTransform3D(-center.X, -center.Y, -center.Z));
            g.Children.Add(new ScaleTransform3D(s, s, s));
            g.Children.Add(new RotateTransform3D(rotation3D));

            return g;
        }
    }
}