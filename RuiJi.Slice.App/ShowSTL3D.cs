using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using _3DTools;

namespace RuiJi.Slice.App
{
    class ShowSTL3D
    {
        private string filePath;
        private PerspectiveCamera myCamera;  //摄像机
        public double lookx = 0;            //摄像机初始位置 X 坐标
        public double looky = 0;            //摄像机初始位置 Y 坐标
        public double lookz = 1000;         //摄像机初始位置 Z 坐标
        public double tmpNear = 0;            //记录上一次缩放的大小

        public double lengthyz = 0;           //左右旋转的轴
        public double lengthxz = 0;           //上下旋转的轴

        private Point ModelPos;
        private ModelVisual3D myModel;      //模型
        private Point3D _center;            //设置模型中心点
        private List<Point3D> m_listPoint3D;

        private Rect3D rect3D;              //模型的外切矩形
        ModelVisual3D myModelLight;         //光源

        Vector3D preCameraDirection;        //相机旋转前的位置
        Vector3D preVisualDirection;        //光源旋转前的方向

        private int worldLineLength = 80;
        ScreenSpaceLines3D _worldLine;      //世界坐标
        Transform3D LockWorldLineTrans = null;

        private GeometryModel3D myGeomentryMode1;

        /*
         * 构造函数，初始化文件路径
         */
        public ShowSTL3D(string filePath)
        {
            m_listPoint3D = new List<Point3D>();
            m_listPoint3D.Clear();

            preCameraDirection = new Vector3D();
            preVisualDirection = new Vector3D();
            _worldLine = new ScreenSpaceLines3D();

            this.filePath = filePath;
            ModelPos = new Point(0, 0);
        }

        /*
       * 绘画世界坐标
       */
        public ScreenSpaceLines3D DrawWroldLine()
        {
            Point3DCollection collection = new Point3DCollection();

            //X轴
            collection.Add(new Point3D(-worldLineLength, -worldLineLength, -worldLineLength));
            collection.Add(new Point3D(worldLineLength, -worldLineLength, -worldLineLength));
            //X轴箭头
            collection.Add(new Point3D(worldLineLength, 0 - worldLineLength, 0 - worldLineLength));
            collection.Add(new Point3D(worldLineLength - 3, 3 - worldLineLength, 0 - worldLineLength));
            collection.Add(new Point3D(worldLineLength, 0 - worldLineLength, 0 - worldLineLength));
            collection.Add(new Point3D(worldLineLength - 3, -3 - worldLineLength, 0 - worldLineLength));
            ////X标志
            collection.Add(new Point3D(worldLineLength, -worldLineLength - 5, -worldLineLength));
            collection.Add(new Point3D(worldLineLength + 3, -worldLineLength - 8, -worldLineLength));
            collection.Add(new Point3D(worldLineLength + 3, -worldLineLength - 5, -worldLineLength));
            collection.Add(new Point3D(worldLineLength, -worldLineLength - 8, -worldLineLength));

            //Y轴
            collection.Add(new Point3D(-worldLineLength, worldLineLength, -worldLineLength));
            collection.Add(new Point3D(-worldLineLength, -worldLineLength, -worldLineLength));

            collection.Add(new Point3D(0 - worldLineLength, worldLineLength, 0 - worldLineLength));
            collection.Add(new Point3D(3 - worldLineLength, worldLineLength - 3, 0 - worldLineLength));
            collection.Add(new Point3D(0 - worldLineLength, worldLineLength, 0 - worldLineLength));
            collection.Add(new Point3D(-3 - worldLineLength, worldLineLength - 3, 0 - worldLineLength));

            collection.Add(new Point3D(-11 - worldLineLength, worldLineLength, 0 - worldLineLength));
            collection.Add(new Point3D(-9 - worldLineLength, worldLineLength - 2, 0 - worldLineLength));
            collection.Add(new Point3D(-7 - worldLineLength, worldLineLength, 0 - worldLineLength));
            collection.Add(new Point3D(-9 - worldLineLength, worldLineLength - 2, 0 - worldLineLength));
            collection.Add(new Point3D(-9 - worldLineLength, worldLineLength - 2, 0 - worldLineLength));
            collection.Add(new Point3D(-9 - worldLineLength, worldLineLength - 5, 0 - worldLineLength));

            //Z轴
            collection.Add(new Point3D(-worldLineLength, -worldLineLength, worldLineLength));
            collection.Add(new Point3D(-worldLineLength, -worldLineLength, -worldLineLength));

            collection.Add(new Point3D(0 - worldLineLength, 0 - worldLineLength, worldLineLength));
            collection.Add(new Point3D(0 - worldLineLength, -3 - worldLineLength, worldLineLength - 3));
            collection.Add(new Point3D(0 - worldLineLength, 0 - worldLineLength, worldLineLength));
            collection.Add(new Point3D(0 - worldLineLength, 3 - worldLineLength, worldLineLength - 3));

            collection.Add(new Point3D(10 - worldLineLength, 0 - worldLineLength, worldLineLength));
            collection.Add(new Point3D(5 - worldLineLength, 0 - worldLineLength, worldLineLength));
            collection.Add(new Point3D(5 - worldLineLength, 0 - worldLineLength, worldLineLength));
            collection.Add(new Point3D(10 - worldLineLength, 0 - worldLineLength, worldLineLength - 4));
            collection.Add(new Point3D(10 - worldLineLength, 0 - worldLineLength, worldLineLength - 4));
            collection.Add(new Point3D(5 - worldLineLength, 0 - worldLineLength, worldLineLength - 4));


            //平面
            var tempLength = 5;
            while (tempLength < worldLineLength * 2)
            {
                collection.Add(new Point3D(-worldLineLength + tempLength, -worldLineLength, -worldLineLength));
                collection.Add(new Point3D(-worldLineLength + tempLength, worldLineLength, -worldLineLength));

                collection.Add(new Point3D(-worldLineLength, -worldLineLength + tempLength, -worldLineLength));
                collection.Add(new Point3D(worldLineLength, -worldLineLength + tempLength, -worldLineLength));
                tempLength += 5;
            }


            _worldLine.Points = collection;
            _worldLine.Color = Colors.LightSlateGray;
            _worldLine.Thickness = 2;
            return _worldLine;
        }

        public ScreenSpaceLines3D GetWorldLine()
        {
            return _worldLine;
        }

        /* 
         * 根据不同的文件类型（二进制或ascii），进行对STL文件解析
         * 返回值：三维模型的三角基元
         */
        public MeshGeometry3D IsAsciiOrBinary()
        {
            long fileLong = 0;
            fileLong = new FileInfo(filePath).Length;//获取文件长度

            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fs);
            binaryReader.ReadBytes(80);
            byte[] numArray = new byte[4];
            binaryReader.Read(numArray, 0, 4);
            int num = BitConverter.ToInt32(numArray, 0);

            if ((num * 50 + 84) == fileLong)
            {
                return ReadBinary();
            }
            return ReadAscii();
        }

        /*
         * 读取STL的ascii格式文件，生成三角形形状的三角形基元
         * 返回值：三维模型的三角基元
         */
        public MeshGeometry3D ReadAscii()
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            StreamReader sr = new StreamReader(filePath);
            string line;
            string[] split = new string[4];

            while ((line = sr.ReadLine()) != null)
            {
                line = line.TrimStart();
                if (line.StartsWith("facet normal"))
                {
                    Point3D point3D;
                    //向量
                    line = line.Substring(12);

                    //点
                    line = sr.ReadLine();

                    for (int i = 0; i < 3; i++)
                    {
                        line = sr.ReadLine();
                        split = line.TrimStart().Split(' ');
                        point3D = new Point3D(double.Parse(split[1]), double.Parse(split[2]), double.Parse(split[3]));
                        m_listPoint3D.Add(point3D);
                        mesh.Positions.Add(point3D);
                    }
                }
            }
            return mesh;
        }

        /*
         * 读取STL的二进制格式文件，生成三角形形状的三角形基元
         * 返回值：三维模型的三角基元
         */
        public MeshGeometry3D ReadBinary()
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fs);

            binaryReader.ReadBytes(80);
            byte[] numArray = new byte[4];
            binaryReader.Read(numArray, 0, 4);
            int num = BitConverter.ToInt32(numArray, 0);

            while (num > 0)
            {
                binaryReader.ReadBytes(12);
                for (int i = 1; i <= 3; i++)
                {
                    mesh.Positions.Add(addPoint3D(binaryReader));
                }
                binaryReader.ReadBytes(2);
                num--;
            }
            return mesh;
        }

        //移动模型
        public MeshGeometry3D movedModel3D()
        {
            MeshGeometry3D movedMesh = new MeshGeometry3D();

            foreach (Point3D point3D in m_listPoint3D)
            {
                Vector3D newPoint3D = new Vector3D();
                newPoint3D = point3D - _center;
                movedMesh.Positions.Add(
                    new Point3D(newPoint3D.X, newPoint3D.Y, newPoint3D.Z));
            }

            return movedMesh;
        }

        /*
         * 分析二进制的3D坐标
         * 返回值：Point3D对象
         */
        public Point3D addPoint3D(BinaryReader binaryReader)
        {
            Point3D point3D;
            byte[] x = new byte[4];
            byte[] y = new byte[4];
            byte[] z = new byte[4];
            binaryReader.Read(x, 0, 4);
            binaryReader.Read(y, 0, 4);
            binaryReader.Read(z, 0, 4);

            float X = BitConverter.ToSingle(x, 0);
            float Y = BitConverter.ToSingle(y, 0);
            float Z = BitConverter.ToSingle(z, 0);
            point3D = new Point3D(X, Y, Z);

            m_listPoint3D.Add(point3D);
            return point3D;
        }

        /*
         * 默认摄像机为正交投影
         */
        public PerspectiveCamera MyCamera()
        {
            myCamera = new PerspectiveCamera();//表示正交投影摄像机
            double radius = (_center - rect3D.Location).Length; //获取外切球体半径

            Point3D position = _center;
            position.Z += radius * 2;
            position.X = 0;
            position.Y -= radius * 2;

            myCamera.Position = position;
            myCamera.FieldOfView = 60;
            //myCamera.UpDirection = new Point3D(0, 0, 10) - new Point3D(0, 0, 0);
            myCamera.LookDirection = new Point3D(0, 0, 0) - position;
            myCamera.NearPlaneDistance = radius / 100;
            myCamera.FarPlaneDistance = radius * 100;

            preCameraDirection = myCamera.Position - new Point3D(0, 0, 0);

            return myCamera;
        }

        /* 
         *设置3D世界的光源
         * 默认的光源为平行光
         * ModelVisual3D 为光源的父类
         * 返回值：光源信息
         */
        public ModelVisual3D myModelVisual3D(bool def = false)
        {
            ModelVisual3D ModelLight = new ModelVisual3D();//描述光源

            DirectionalLight myDirectionLight = new DirectionalLight();//平行光

            myDirectionLight.Direction = new Vector3D(0, -1, -1);

            myDirectionLight.Color = Brushes.White.Color;
            ModelLight.Content = myDirectionLight;

            preVisualDirection = myDirectionLight.Direction;
            myModelLight = ModelLight;

            return ModelLight;
        }
        /*
         * 获取光源,旋转的时候，消除原来的光源，消除内存过大
         */
        public ModelVisual3D GetModelVisual3D()
        {
            return myModelLight;
        }
        /*
         * 根据模型的变换轨迹，变换光源的位置
         */
        public ModelVisual3D TransModelVisual3D(Transform3D transfrom3D)
        {
            myModelLight.Transform = transfrom3D;
            return myModelLight;
        }
        public ModelVisual3D TransModelVisual3DWithoutWorld(Transform3D transfrom3D)
        {
            //myModelLight.Transform = transfrom3D;
            //_worldLine.Transform = transfrom3D;
            return myModelLight;
        }

        /* 
         * 设置物体的材质
         * 这里使用的是 自发光-渐变色画刷
         * 返回值：材质
         */
        public MaterialGroup GetMaterial(int Kindmaterial = 0)
        {
            MaterialGroup materialGroup = new MaterialGroup();
            Material myMaterial;

            //myMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.LightSkyBlue));//自发光-传统画刷
            //myMaterial = new DiffuseMaterial(new RadialGradientBrush(Colors.LightSkyBlue, Colors.Blue));//自发光-渐变色画刷      
            myMaterial = new SpecularMaterial(new SolidColorBrush(Colors.White), 10);
            materialGroup.Children.Add(myMaterial);
            //myMaterial = new EmissiveMaterial(new SolidColorBrush(Colors.LightGray));
            myMaterial = new DiffuseMaterial(new LinearGradientBrush(Colors.Blue, Colors.Gray, 10));//自发光-渐变色画刷
            materialGroup.Children.Add(myMaterial);

            return materialGroup;
        }

        /*
         * 加载读取的STL文件的坐标
         * 这里设置的STL模型的颜色为：Brown
         * 返回值：加载好的STL模型对象
         */
        public ModelVisual3D GetMyModelVisual3D()//显示3D图形
        {
            myModel = new ModelVisual3D();
            Model3DGroup myModelGroup = new Model3DGroup();//允许使用多个 三维 模型作为一个单元。

            myGeomentryMode1 = new GeometryModel3D(IsAsciiOrBinary(), GetMaterial());//加载3D几何模型(模型，材质)
            myGeomentryMode1.BackMaterial = new DiffuseMaterial(Brushes.Gray);//3D模型的背面材质

            myModelGroup.Children.Add(myGeomentryMode1);
            myModel.Content = myModelGroup;

            rect3D = Rect3D.Empty;
            rect3D = myModel.Content.Bounds;
            _center = new Point3D((rect3D.X + rect3D.SizeX / 2), (rect3D.Y + rect3D.SizeY / 2),
                                 (rect3D.Z + rect3D.SizeZ / 2));

            var maxLenght = rect3D.SizeX;
            if (maxLenght < rect3D.SizeY)
                maxLenght = rect3D.SizeY;

            if (maxLenght < rect3D.SizeZ)
                maxLenght = rect3D.SizeZ;

            worldLineLength = Convert.ToInt32(Math.Ceiling(maxLenght / 2 + 20));

            ModelVisual3D model = new ModelVisual3D();
            Model3DGroup modelGroup = new Model3DGroup();//允许使用多个 三维 模型作为一个单元。

            GeometryModel3D geomentryMode1 = new GeometryModel3D(movedModel3D(), GetMaterial());//加载3D几何模型(模型，材质)
            geomentryMode1.BackMaterial = new DiffuseMaterial(Brushes.Brown);//3D模型的背面材质

            modelGroup.Children.Add(geomentryMode1);
            model.Content = modelGroup;

            return model;
        }

        /*
         * 根据设置摄像机位置，进行物体远近缩放
         * 运用的是向量的办法进行的缩放
         */
        public PerspectiveCamera nearerCamera(double distance)
        {
            Point3D cameraPos = myCamera.Position;
            Vector3D centerVec = new Vector3D(0, 0, 0);
            Vector3D preCameraVec = new Vector3D(cameraPos.X, cameraPos.Y, cameraPos.Z) - centerVec;

            Vector3D newCameraVec = ((preCameraVec.Length + distance) / preCameraVec.Length) * preCameraVec + centerVec;

            myCamera.Position = new Point3D(newCameraVec.X, newCameraVec.Y, newCameraVec.Z);
            return myCamera;
        }
    }
}
