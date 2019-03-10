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
        private Point3D cameraResetPosition = new Point3D();
        private int zoomPercentage = 100;

        public SceneView(Viewport3D viewport3D)
        {
            this.viewport3D = viewport3D;

            var modelLight = new ModelVisual3D();
            var light = new DirectionalLight();
            light.Color = Brushes.White.Color;
            modelLight.Content = light;
            this.viewport3D.Children.Add(modelLight);

            DrawWorldLine();

            var camera = new PerspectiveCamera();
            camera.FieldOfView = 60;
            camera.Position = new Point3D(0, 500, 0);
            camera.LookDirection = new Vector3D(0, -1, 0);
            camera.UpDirection = new Vector3D(0, 0, -1);

            this.viewport3D.Camera = camera;
        }

        private void DrawWorldLine()
        {
            var model = new GeometryModel3D();
            var mesh = new MeshGeometry3D();
            mesh.Positions.Add(new Point3D(-500, 0, 500));
            mesh.Positions.Add(new Point3D(500, 0, 500));
            mesh.Positions.Add(new Point3D(500, 0, -500));
            mesh.Positions.Add(new Point3D(-500, 0, -500));
            mesh.TriangleIndices = new Int32Collection() { 0, 1, 2, 2, 3, 0 };
            mesh.Normals.Add(new Vector3D(0, 0, 1));
            mesh.Normals.Add(new Vector3D(0, 0, 1));
            mesh.Normals.Add(new Vector3D(0, 0, 1));
            mesh.Normals.Add(new Vector3D(0, 0, 1));
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
            brush.Opacity = 0.85;
            brush.Viewport = new System.Windows.Rect(0, 0, 0.01, 0.01);
            //brush.ViewboxUnits = BrushMappingMode.Absolute;

            var material = new DiffuseMaterial(brush);

            model.Material = material;

            var b = new SolidColorBrush(Colors.White);
            b.Opacity = 0.5;            
            model.BackMaterial = new DiffuseMaterial(b);

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
            cameraResetPosition.X = 0;
            cameraResetPosition.Y -= radius * 2;

            camera.Position = cameraResetPosition;
            camera.LookDirection = new Point3D(0, 0, 0) - cameraResetPosition;
            camera.NearPlaneDistance = 0;
            camera.FarPlaneDistance = radius * 100;
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
            var v = new Vector3D(cameraResetPosition.X, cameraResetPosition.Y, cameraResetPosition.Z) * zoom / 100f;
            camera.Position = new Point3D(v.X, v.Y, v.Z);
        }

        public void Load(string file)
        {
            for (int i = 2; i < viewport3D.Children.Count; i++)
            {
                viewport3D.Children.RemoveAt(i);
            }

            //var model = new ShowSTL3D(file);
            //viewport3D.Children.Add(model.GetMyModelVisual3D());

            //Lookat(model._center, model.radius);
        }
    }
}