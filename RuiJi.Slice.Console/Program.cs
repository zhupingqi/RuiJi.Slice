using RuiJi.Slicer.Core;
using RuiJi.Slicer.Core.File;
using RuiJi.Slicer.Core.ImageMould;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            TestSlicer();
            //TestTo2D();
            Console.ReadLine();
        }

        /// <summary>
        /// 控制台切片工具(未完成)
        /// </summary>
        static void ConsoleSlicer()
        {
            Console.WriteLine("----------------------切片工具----------------------");
            Console.WriteLine("请输入要切片的3D图绝对路径：");
            var path = Console.ReadLine();
            while (true)
            {
                if (!Path.IsPathRooted(path))
                {
                    Console.WriteLine("请输入正确的地址：");
                    path = Console.ReadLine();
                }
                else
                    break;
            }
            //Console.WriteLine("请输入法线的X分量");
            //var x = Console.ReadLine();
            //Console.WriteLine("请输入法线的Y分量");
            //var y = Console.ReadLine();
            //Console.WriteLine("请输入法线的Z分量");
            //var z = Console.ReadLine();
            //Console.WriteLine("请输入平面从原点沿其法线的距离");
            //var d = Console.ReadLine();
            Console.WriteLine("请输入切片数量");
            var count = Console.ReadLine();
            Console.WriteLine("请输入切片弧度");
            var angle = Console.ReadLine();

        }

        /// <summary>
        /// 测试绕轴圆周切片
        /// </summary>
        static void TestArrayCreate()
        {
            var p = new Plane(0, 1, 0, 1);
            var d = new ArrayDefine(p, ArrayType.Circle, 8);
            var c = new CircleArrayCreater();
            var ps = c.CreateArrayPlane(d);

            foreach (var mp in ps)
            {
                Console.WriteLine(mp.ToString());
            }

            Console.WriteLine("TestArrayCreate finish");
        }

        /// <summary>
        /// 测试读取图片并切片
        /// </summary>
        static void TestSlicer()
        {
            //-0.03141076,0.9995066
            var doc = STLDocument.Open(AppDomain.CurrentDomain.BaseDirectory + @"/stl/Mount_Fuji.stl");
            doc.MakeCenter();

            var results = Slicer.DoSlice(doc.Facets.ToArray(), new ArrayDefine[] {
                new ArrayDefine(new Plane(0, 1, 0, 0), ArrayType.Circle, 50,180)
                //new ArrayDefine(new Plane(-0.03141076f, 0.9995066f, 0, 8), ArrayType.Circle, 8)
            });

            var prefix = 0;
            foreach (var key in results.Keys)
            {
                var filename = AppDomain.CurrentDomain.BaseDirectory + prefix + "_frame.h";
                System.IO.File.Delete(filename);

                var code = "";
                var frameTable = new List<string>();

                var images = SliceImage.ToImage(results[key], (int)doc.Size.Length, (int)doc.Size.Height, 128, 64, 0, 0);
                for (int i = 0; i < images.Count; i++)
                {
                    var bmp = images[i];
                    bmp.Save(AppDomain.CurrentDomain.BaseDirectory + "/" + prefix + "_" + i + ".bmp", ImageFormat.Bmp);

                    IImageMould im = new SSD1306();
                    code += "static unsigned char _" + prefix + "_frame_" + i + "[] = { " + im.GetMould(bmp) + " }; \n";
                    frameTable.Add("_" + prefix + "_frame_" + i);
                }

                code += "unsigned char* _" + prefix + "_frames_table[] = { " + string.Join(",", frameTable.ToArray()) + " };";
                System.IO.File.AppendAllText(filename, code);

                prefix++;
            }

            Console.WriteLine("TestSlicer finish");
        }

        /// <summary>
        /// 测试转为2D图形
        /// </summary>
        static void TestTo2D()
        {
            //var p = SliceImage.To2D(new Plane(0,1,0,8),new Vector3(-1,8,1));
            //Console.WriteLine(p);
        }
    }
}