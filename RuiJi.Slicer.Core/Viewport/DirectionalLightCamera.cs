using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace RuiJi.Slicer.Core.Viewport
{
    public class DirectionalLightCamera
    {
        private Viewport3D viewport;
        private Point3D resetPosition;
        private int zoomPercentage = 100;
        public PerspectiveCamera Camera { get; private set; }
        public DirectionalLight Light { get; private set; }

        public int CameraZoom
        {
            get => zoomPercentage;
            set
            {
                zoomPercentage = value;
                Zoom(value);
            }
        }

        public DirectionalLightCamera(Viewport3D viewport)
        {
            this.viewport = viewport;
            this.Camera = new PerspectiveCamera();
            Camera.FieldOfView = 60;
            Camera.Changed += Camera_Changed;

            Light = new DirectionalLight();
            Light.Color = Colors.White;
        }

        private void Camera_Changed(object sender, EventArgs e)
        {
            Light.Direction = Camera.LookDirection * Camera.Transform.Value;
        }

        public void Lookat(Point3D lookat, double radius)
        {
            resetPosition = lookat;
            resetPosition.Z += radius * 3;
            resetPosition.Y += radius * 1.5;

            Camera.Position = resetPosition;
            Camera.LookDirection = lookat - resetPosition;
            Camera.NearPlaneDistance = 0;
            Camera.FarPlaneDistance = radius * 10000;
            Camera.UpDirection = new System.Windows.Media.Media3D.Vector3D(0, 1, 0);

            var da = new Point3DAnimation();
            da.Duration = new Duration(TimeSpan.FromSeconds(1));
            da.To = resetPosition;
            Camera.BeginAnimation(PerspectiveCamera.PositionProperty, da);
        }

        public void Zoom(bool zoomIn)
        {
            if (zoomIn)
            {
                zoomPercentage += zoomPercentage < 500 ? 10 : 0;
            }
            else
            {
                zoomPercentage -= zoomPercentage > 0 ? 10 : 0;
            }

            Zoom(zoomPercentage);
        }

        public void Zoom(int zoom)
        {
            var v = new System.Windows.Media.Media3D.Vector3D(resetPosition.X, resetPosition.Y, resetPosition.Z) * zoom / 100f;

            var da = new Point3DAnimation();
            da.Duration = new Duration(TimeSpan.FromMilliseconds(100));
            da.To = new Point3D(v.X, v.Y, v.Z); ;
            Camera.BeginAnimation(PerspectiveCamera.PositionProperty, da);
        }
    }
}
