using RuiJi.Slice.ConsoleTool;
using RuiJi.Slicer.Core;
using RuiJi.Slicer.Core.File;
using RuiJi.Slicer.Core.ImageMould;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            TestSlicer();
            ////TestTo2D();
            //Console.ReadLine();
            //ConsoleSliceTool.ConsoleSlicer();
        }

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

        static void TestSlicer()
        {
            //-0.03141076,0.9995066
            var doc = STLDocument.Open(AppDomain.CurrentDomain.BaseDirectory + @"/stl/bmwi8.stl");
            doc.MakeCenter();

            var results = Slicer.DoSlice(doc.Facets.ToArray(), new ArrayDefine[] {
                new ArrayDefine(new Plane(0, 1, 0, 0), ArrayType.Circle, 200,360)
                //new ArrayDefine(new Plane(-0.03141076f, 0.9995066f, 0, 8), ArrayType.Circle, 8)
            });

            var prefix = 0;
            IImageMould im = new LED6432P();

            foreach (var key in results.Keys)
            {
                var filename = AppDomain.CurrentDomain.BaseDirectory + prefix + "_frame.h";
                System.IO.File.Delete(filename);

                var code = "";
                var frameTable = new List<string>();

                var images = SliceImage.ToImage(results[key], doc.Size, 64, 32, 0, 0);
                for (int i = 0; i < images.Count; i++)
                {
                    var bmp = images[i];
                    bmp.Save(AppDomain.CurrentDomain.BaseDirectory + "/" + prefix + "_" + i + ".bmp", ImageFormat.Bmp);

                    code += im.GetFrameCode(prefix,i,bmp) ;
                    frameTable.Add("_" + prefix + "_frame_" + i);
                }

                code += im.GetFramesCode(prefix,frameTable);
                System.IO.File.AppendAllText(filename, code);

                prefix++;
            }

            Console.WriteLine("TestSlicer finish");
        }

        static void TestTo2D()
        {
            //var p = SliceImage.To2D(new Plane(0,1,0,8),new Vector3(-1,8,1));
            //Console.WriteLine(p);
        }
    }
}