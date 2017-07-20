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

namespace RuiJi.Slice.ConsoleTool
{
    public class ConsoleSliceTool
    {
        /// <summary>
        /// 控制台切片工具
        /// </summary>
        public static void ConsoleSlicer()
        {
            Console.WriteLine("----------------------切片工具----------------------");
            while (true)
            {
                var doc = ReadFile();
                var slices = Slice(doc);
                SaveSlices(slices, doc.Size);
                Console.WriteLine("请问是否继续？（y/n）");
                var confirm = Console.ReadLine();
                if (confirm.ToLower() == "n")
                    break;
            }

        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <returns></returns>
        static STLDocument ReadFile()
        {
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
            Console.WriteLine("正在读取文件请稍后...");
            var doc = STLDocument.Open(path);
            doc.MakeCenter();
            return doc;
        }

        /// <summary>
        /// 生成切片
        /// </summary>
        /// <param name="doc">文件</param>
        /// <returns>切片结果集</returns>
        static Dictionary<ArrayDefine, List<SlicedPlane>> Slice(STLDocument doc)
        {
            Console.WriteLine("请选择切片模式：");
            Console.WriteLine("1.快速切片\t2.自定义切片");
            string type = Console.ReadLine();
            var listdefine = new List<ArrayDefine>();
            while (true)
            {
                var plane = new Plane(0, 1, 0, 0);
                if (type == "2")
                {
                    Console.WriteLine("请输入旋转面法线的X分量");
                    int x = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine("请输入旋转面法线的Y分量");
                    int y = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine("请输入旋转面法线的Z分量");
                    int z = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine("请输入旋转面从原点沿其法线的距离");
                    int d = Convert.ToInt32(Console.ReadLine());
                    plane = new Plane(x, y, z, d);
                }

                Console.WriteLine("请输入切片数量");
                var count = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("请输入切片弧度(360为最大)");
                var angle = Convert.ToInt32(Console.ReadLine());
                var arrayDefine = new ArrayDefine(plane, ArrayType.Circle, count, angle);
                listdefine.Add(arrayDefine);
                if (type == "2")
                {
                    Console.WriteLine("请问是否继续录入下个切片定义？（y/n）");
                    var confirm = Console.ReadLine();
                    if (confirm.ToLower() == "n")
                    {
                        break;
                    }
                }
                else
                    break;
            }
            Console.WriteLine("正在切片，请稍等...");
            var results = Slicer.Core.Slicer.DoSlice(doc.Facets.ToArray(), listdefine.ToArray());
            Console.WriteLine("切片完成");
            return results;
        }

        /// <summary>
        /// 保存切片结果
        /// </summary>
        /// <param name="slices">切片结果集</param>
        /// <param name="size">3维图原始大小</param>
        static void SaveSlices(Dictionary<ArrayDefine, List<SlicedPlane>> slices, ModelSize size)
        {
            Console.WriteLine("请选择切片保存模式");
            Console.WriteLine("1.图片\t2.机器语言\t3.图片与机器语言");
            var saveType = Console.ReadLine();
            var prefix = 0;
            foreach (var key in slices.Keys)
            {

                Console.WriteLine("正在保存第" + (prefix + 1) + "组切片...");
                Console.WriteLine("请输入图片尺寸");
                Console.WriteLine("长：");
                int width = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("宽：");
                int height = Convert.ToInt32(Console.ReadLine());

                var filename = AppDomain.CurrentDomain.BaseDirectory + prefix + "_frame.h";
                System.IO.File.Delete(filename);

                var code = "";
                var frameTable = new List<string>();

                var images = SliceImage.ToImage(slices[key], size, width, height, 0, 0);
                for (int i = 0; i < images.Count; i++)
                {
                    var bmp = images[i];
                    if (saveType == "1" || saveType == "3")
                        bmp.Save(AppDomain.CurrentDomain.BaseDirectory + "/" + prefix + "_" + i + ".bmp", ImageFormat.Bmp);


                    if (saveType == "2" || saveType == "3")
                    {
                        IImageMould im = new SSD1306();
                        code += "static unsigned char _" + prefix + "_frame_" + i + "[] = { " + im.GetMould(bmp) + " }; \n";
                        frameTable.Add("_" + prefix + "_frame_" + i);
                    }
                }
                if (saveType == "2" || saveType == "3")
                {
                    code += "unsigned char* _" + prefix + "_frames_table[] = { " + string.Join(",", frameTable.ToArray()) + " };";
                    System.IO.File.AppendAllText(filename, code);
                }
                Console.WriteLine("第" + (prefix + 1) + "组切片保存完成");
                prefix++;
            }
            Console.WriteLine("所有切片保存完成。");
        }


    }
}
