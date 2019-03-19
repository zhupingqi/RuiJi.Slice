using Assimp;
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
                return aiScene.Animations.Select(m => m.Name).ToList();
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

            if (cancellationToken != null)
            {
                cancellationToken.Cancel();
            }

            try
            {
                aiScene = assimpContext.ImportFile(file);
                RenderMesh();
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

            if (cancellationToken != null)
            {
                cancellationToken.Cancel();

                while (true)
                {
                    if (aniTask.Status != TaskStatus.Running)
                        break;

                    Thread.Sleep(100);
                }

                aniTask.Dispose();
                aniTask = null;
            }

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
                                anim.Render(MeshGroup, ani, tick);

                                if (replaceCamera)
                                {
                                    var bounds = MeshGroup.Bounds;
                                    var center = new Point3D((bounds.X + bounds.SizeX / 2), (bounds.Z + bounds.SizeZ / 2), (bounds.Y + bounds.SizeY / 2));
                                    var radius = (center - bounds.Location).Length;

                                    LightCamera.Lookat(center, radius);

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

        private void RenderMesh()
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
            };

            var model = new ModelVisual3D();
            model.Content = MeshGroup;
            viewport3D.Children.Add(model);

            var bounds = MeshGroup.Bounds;

            var center = new Point3D((bounds.X + bounds.SizeX / 2), (bounds.Z + bounds.SizeZ / 2), (bounds.Y + bounds.SizeY / 2));
            var radius = (center - bounds.Location).Length;

            var t = MeshGroup.Transform as MatrixTransform3D;
            t.Matrix.Translate(new System.Windows.Media.Media3D.Vector3D() - new System.Windows.Media.Media3D.Vector3D(-center.X, Math.Abs(center.Y), -center.Z)); 

            LightCamera.Lookat(center, radius);
        }
    }
}