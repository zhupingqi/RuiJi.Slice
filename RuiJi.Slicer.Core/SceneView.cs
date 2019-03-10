using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace RuiJi.Slicer.Core
{
    public class SceneView
    {
        private Viewport3D viewport3D;
        private Point3D cameraResetPosition;
        private int zoomPercentage = 100;
        private DirectionalLight cameraLight;

        public SceneView(Viewport3D viewport3D)
        {
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
            camera.Position = cameraResetPosition = new Point3D(0, 100, 200);
            camera.LookDirection = new System.Windows.Media.Media3D.Vector3D(0, -0.5, -1);
            camera.UpDirection = new System.Windows.Media.Media3D.Vector3D(0, 0, -1);
            cameraLight.Direction = camera.LookDirection * camera.Transform.Value;
            camera.Changed += Camera_Changed;

            this.viewport3D.Camera = camera;
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
            cameraResetPosition.Z += radius * 2;
            cameraResetPosition.Y += radius * 2;

            camera.Position = cameraResetPosition;
            camera.LookDirection = lookat - cameraResetPosition;
            camera.NearPlaneDistance = 0;
            camera.FarPlaneDistance = radius * 100;
            camera.UpDirection = new System.Windows.Media.Media3D.Vector3D(0,1,0);
        }

        public void Zoom(bool zoomIn)
        {
            if (zoomIn)
            {
                zoomPercentage += zoomPercentage < 500 ? 10 : 0;
            }
            else
            {
                zoomPercentage -= zoomPercentage > 50 ? 10 : 0;
            }

            Zoom(zoomPercentage);
        }

        public void Zoom(int zoom)
        {
            var camera = viewport3D.Camera as PerspectiveCamera;
            var v = new System.Windows.Media.Media3D.Vector3D(cameraResetPosition.X, cameraResetPosition.Y, cameraResetPosition.Z) * zoom / 100f;
            camera.Position = new Point3D(v.X, v.Y, v.Z);
        }

        public void Load(string file)
        {
            for (int i = 2; i < viewport3D.Children.Count; i++)
            {
                viewport3D.Children.RemoveAt(i);
            }

            var context = new AssimpContext();
            var s = context.ImportFile(file);

            var geoModel = new GeometryModel3D();
            var material = new DiffuseMaterial(new SolidColorBrush(Colors.SandyBrown));
            material.AmbientColor = Colors.SaddleBrown;
            geoModel.Material = material;

            var mesh = new MeshGeometry3D();
            s.Meshes.ForEach(mm =>
            {
                mm.Vertices.ForEach(m =>
                {
                    mesh.Positions.Add(new Point3D(m.X, m.Y, m.Z));
                });

                mm.Normals.ForEach(m =>
                {
                    mesh.Normals.Add(new System.Windows.Media.Media3D.Vector3D(m.X, m.Y, m.Z));
                });

                mm.Faces.ForEach(m => {
                    mesh.TriangleIndices.Add(m.Indices[0]);
                    mesh.TriangleIndices.Add(m.Indices[1]);
                    mesh.TriangleIndices.Add(m.Indices[2]);
                });
            });
            geoModel.Geometry = mesh;
            
            var center = new Point3D((mesh.Bounds.X + mesh.Bounds.SizeX / 2), (mesh.Bounds.Y + mesh.Bounds.SizeY / 2),
                                 (mesh.Bounds.Z + mesh.Bounds.SizeZ / 2));
            var radius = (center - mesh.Bounds.Location).Length;

            Lookat(center,radius);

            var model = new ModelVisual3D();
            model.Content = geoModel;
            var rotation = new RotateTransform3D();
            rotation.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(1, 0, 0), -90);
            model.Transform = rotation;
            viewport3D.Children.Add(model);
        }
    }
}