using RuiJi.Slicer.Core;
using RuiJi.Slicer.Core.File;
using System;
using System.Collections.Generic;
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
            //TestTo2D();
            Console.ReadLine();
        }



        static void TestArrayCreate()
        {
            var p = new Plane(0,1,0,1);
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
            var p = new Plane(0, 1, 0, 8);
            var d = new ArrayDefine(p, ArrayType.Circle, 8);
            var doc = STLDocument.Open(AppDomain.CurrentDomain.BaseDirectory + "bmwi8.stl");
            doc.MakeCenter();

            var results = Slicer.DoSlice(doc.Facets.ToArray(), new ArrayDefine[] {
                new ArrayDefine(new Plane(0, 1, 0, 8), ArrayType.Circle, 8)
                //new ArrayDefine(new Plane(-0.03141076f, 0.9995066f, 0, 8), ArrayType.Circle, 8)
            });

            var prefix = 0;
            foreach (var key in results.Keys)
            {
                SliceImage.ToImage(results[key], (int)doc.Size.Length, (int)doc.Size.Height, 128, 64, prefix.ToString());
                prefix++;
            }

            Console.WriteLine("TestSlicer finish");
        }

        static void TestTo2D()
        {
            var p = SliceImage.To2D(new Plane(0,1,0,8),new Vector3(-1,8,1));
            Console.WriteLine(p);
        }
    }
}