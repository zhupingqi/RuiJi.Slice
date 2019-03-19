using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace RuiJi.Slicer.Core.Viewport
{
    public class GroundPlane
    {
        public static void DrawGround(Viewport3D viewport3D)
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

            var worldlPlane = new ModelVisual3D();
            worldlPlane.Content = model;

            viewport3D.Children.Add(worldlPlane);
        }
    }
}
