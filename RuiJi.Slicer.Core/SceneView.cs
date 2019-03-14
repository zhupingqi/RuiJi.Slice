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

namespace RuiJi.Slicer.Core
{
    public class SceneView
    {
        private Viewport3D viewport3D;
        private AssimpContext assimpContext;
        private Scene aiScene;
        private Point3D cameraResetPosition;
        private int zoomPercentage = 100;
        private DirectionalLight cameraLight;
        private Task aniTask;
        private Model3DGroup meshModelGroup = new Model3DGroup();

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
            assimpContext = new AssimpContext();
            this.viewport3D = viewport3D;

            var modelLight = new ModelVisual3D();
            var lightGroup = new Model3DGroup();

            cameraLight = new DirectionalLight();
            cameraLight.Color = Colors.White;
            lightGroup.Children.Add(cameraLight);

            var ambientLight = new AmbientLight(Colors.White);
            lightGroup.Children.Add(ambientLight);

            modelLight.Content = lightGroup;
            this.viewport3D.Children.Add(modelLight);

            DrawGroundPlane();

            var camera = new PerspectiveCamera();
            camera.FieldOfView = 60;
            camera.Changed += Camera_Changed;

            this.viewport3D.Camera = camera;

            Lookat(new Point3D(), 100);
        }

        private void Camera_Changed(object sender, EventArgs e)
        {
            var camera = viewport3D.Camera as PerspectiveCamera;
            cameraLight.Direction = camera.LookDirection * camera.Transform.Value;
        }

        private void DrawGroundPlane()
        {
            var model = new GeometryModel3D();
            var mesh = new MeshGeometry3D();
            mesh.Positions.Add(new Point3D(-500, 0, 500));
            mesh.Positions.Add(new Point3D(500, 0, 500));
            mesh.Positions.Add(new Point3D(500, 0, -500));
            mesh.Positions.Add(new Point3D(-500, 0, -500));
            mesh.TriangleIndices = new Int32Collection() { 0, 1, 2, 2, 3, 0 };
            mesh.Normals.Add(new System.Windows.Media.Media3D.Vector3D(0, 0, 1));
            mesh.Normals.Add(new System.Windows.Media.Media3D.Vector3D(0, 0, 1));
            mesh.Normals.Add(new System.Windows.Media.Media3D.Vector3D(0, 0, 1));
            mesh.Normals.Add(new System.Windows.Media.Media3D.Vector3D(0, 0, 1));
            mesh.TextureCoordinates.Add(new Point(0, 1));
            mesh.TextureCoordinates.Add(new Point(1, 1));
            mesh.TextureCoordinates.Add(new Point(1, 0));
            mesh.TextureCoordinates.Add(new Point(0, 0));
            model.Geometry = mesh;

            var bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + @"/worldline.bmp", UriKind.Relative);
            bi.EndInit();

            var brush = new ImageBrush(bi);
            brush.AlignmentX = AlignmentX.Left;
            brush.AlignmentY = AlignmentY.Top;
            brush.TileMode = TileMode.Tile;
            brush.Stretch = Stretch.Fill;
            brush.Opacity = 0.5;
            brush.Viewport = new System.Windows.Rect(0, 0, 0.01, 0.01);

            var material = new DiffuseMaterial(brush);

            model.Material = material;
            model.BackMaterial = material;
            model.Freeze();

            var worldline = new ModelVisual3D();
            worldline.Content = model;

            viewport3D.Children.Add(worldline);
        }

        public int CameraZoom
        {
            get => zoomPercentage;
            set
            {
                zoomPercentage = value;
                Zoom(value);
            }
        }

        public void Lookat(Point3D lookat, double radius)
        {
            var camera = viewport3D.Camera as PerspectiveCamera;

            cameraResetPosition = lookat;
            cameraResetPosition.Z += radius * 3;
            cameraResetPosition.Y += radius * 1.5;

            camera.Position = cameraResetPosition;
            camera.LookDirection = lookat - cameraResetPosition;
            camera.NearPlaneDistance = 0;
            camera.FarPlaneDistance = radius * 10000;
            camera.UpDirection = new System.Windows.Media.Media3D.Vector3D(0, 1, 0);

            var da = new Point3DAnimation();
            da.Duration = new Duration(TimeSpan.FromSeconds(1));
            da.To = cameraResetPosition;
            camera.BeginAnimation(PerspectiveCamera.PositionProperty, da);
        }

        public void Zoom(bool zoomIn)
        {
            if (zoomIn)
            {
                zoomPercentage += zoomPercentage < 500 ? 10 : 0;
            }
            else
            {
                zoomPercentage -= zoomPercentage > 0 ? 1 : 0;
            }

            Zoom(zoomPercentage);
        }

        public void Zoom(int zoom)
        {
            var camera = viewport3D.Camera as PerspectiveCamera;
            var v = new System.Windows.Media.Media3D.Vector3D(cameraResetPosition.X, cameraResetPosition.Y, cameraResetPosition.Z) * zoom / 100f;

            var da = new Point3DAnimation();
            da.Duration = new Duration(TimeSpan.FromMilliseconds(100));
            da.To = new Point3D(v.X, v.Y, v.Z); ;
            camera.BeginAnimation(PerspectiveCamera.PositionProperty, da);
        }

        public void Load(string file)
        {
            for (int i = 2; i < viewport3D.Children.Count; i++)
            {
                viewport3D.Children.RemoveAt(i);
            }

            aiScene = assimpContext.ImportFile(file);
            RenderMesh();
            if (aiScene.HasAnimations)
            {
                //Play();
            }
        }

        public void Play(string name = "")
        {
            if (string.IsNullOrEmpty(name))
                name = aiScene.Animations[0].Name;

            AnimationName = name;

            if (aniTask == null)
            {
                aniTask = new Task(() =>
                {
                    var anim = new Viewport.AssimpAnimation(aiScene);

                    while (true)
                    {
                        var ani = aiScene.Animations.First(m => m.Name == AnimationName);

                        for (float tick = 0; tick < ani.DurationInTicks; tick++)
                        {
                            try
                            {
                                viewport3D.Dispatcher.Invoke(() =>
                                {
                                    anim.Render(meshModelGroup, ani, tick);
                                });

                                Thread.Sleep(33);
                            }
                            catch { }
                        }
                    }
                });

                aniTask.Start();
            }
        }

        public void Stop()
        {
            aniTask.Dispose();
        }

        private void RenderMesh()
        {
            meshModelGroup.Children.Clear();

            if (!aiScene.HasMeshes)
                return;

            var material = new DiffuseMaterial(new SolidColorBrush(Colors.SandyBrown));
            material.AmbientColor = Colors.SaddleBrown;

            foreach (var aiMesh in aiScene.Meshes)
            {
                var geoModel = new GeometryModel3D();
                geoModel.Material = material;
                geoModel.BackMaterial = material;
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
                });

                geoModel.Geometry = mesh;
                meshModelGroup.Children.Add(geoModel);
            };

            var model = new ModelVisual3D();
            model.Content = meshModelGroup;
            viewport3D.Children.Add(model);

            var bounds = new Rect3D();
            meshModelGroup.Children.Select(m => m.Bounds).ToList().ForEach(m =>
            {
                bounds.Union(m);
            });

            var center = new Point3D((bounds.X + bounds.SizeX / 2), (bounds.Z + bounds.SizeZ / 2), (bounds.Y + bounds.SizeY / 2));
            var radius = (center - bounds.Location).Length;

            Lookat(center, radius);
        }
    }
}