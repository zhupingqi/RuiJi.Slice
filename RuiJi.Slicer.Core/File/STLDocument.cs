/*
This file is part of RuiJi.Slice: A library for slicing 3D model.
RuiJi.Slice is part of RuiJiHG: RuiJiHG is holographic projection.
see http://www.ruijihg.com/ for more infomation.

Copyright (C) 2017 Pingqi(416803633@qq.com)
Copyright (c) 2017, githublixiang(271800249@qq.com)

RuiJi.Slice is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

RuiJi.Slice is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with wiringPi.
If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Numerics;

namespace RuiJi.Slicer.Core.File
{
    /// <summary>
    ///  3D模型文件
    /// </summary>
    public class STLDocument
    {
        /// <summary>
        /// 面集合
        /// </summary>
        public IList<Facet> Facets { get; private set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 模型大小
        /// </summary>
        public ModelSize Size { get; private set; }

        /// <summary>
        /// 模型中心点
        /// </summary>
        public Vector3 Center { get; private set; }

        public STLDocument()
        {
            Facets = new List<Facet>();
        }

        /// <summary>
        /// 打开并生成文件
        /// </summary>
        /// <param name="path">文件绝对路径</param>
        /// <returns>模型文件</returns>
        public static STLDocument Open(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            using (Stream stream = System.IO.File.OpenRead(path))
            {
                var doc = Read(stream,true);
                doc.CaclSize();

                return doc;
            }
        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="stream">数据流</param>
        /// <param name="tryBinaryIfTextFailed">文本文档读取失败尝试二进制</param>
        /// <returns>模型文件</returns>
        public static STLDocument Read(Stream stream, bool tryBinaryIfTextFailed = false)
        {
            //Determine if the stream contains a text-based or binary-based <see cref="STLDocument"/>, and then read it.
            var isText = IsText(stream);
            STLDocument textStlDocument = null;
            if (isText)
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.ASCII, true, 1024, true))
                {
                    textStlDocument = Read(reader);
                }

                if (textStlDocument.Facets.Count > 0 || !tryBinaryIfTextFailed) return textStlDocument;
                stream.Seek(0, SeekOrigin.Begin);
            }

            //Try binary if zero Facets were read and tryBinaryIfTextFailed==true
            using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var binaryStlDocument = Read(reader);

                //return text reading result if binary reading also failed and tryBinaryIfTextFailed==true
                return (binaryStlDocument.Facets.Count > 0 || !isText) ? binaryStlDocument : textStlDocument;
            }
        }

        /// <summary>
        /// 验证文本文档
        /// </summary>
        /// <param name="stream">数据流</param>
        /// <returns>是否为文档数据</returns>
        public static bool IsText(Stream stream)
        {
            var fileLong = stream.Length;

            byte[] buffer = new byte[4];

            //Reset the stream to tbe beginning and read the first few bytes, then reset the stream to the beginning again.
            stream.Seek(0, SeekOrigin.Begin);
            stream.Seek(80, SeekOrigin.Current);
            stream.Read(buffer, 0, buffer.Length);
            stream.Seek(0, SeekOrigin.Begin);

            int num = BitConverter.ToInt32(buffer, 0);

            if ((num * 50 + 84) == fileLong)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证二进制
        /// </summary>
        /// <param name="stream">数据流</param>
        /// <returns>是否为二进制</returns>
        public static bool IsBinary(Stream stream)
        {
            return !IsText(stream);
        }

        /// <summary>
        /// 根据文本文档流读取文件
        /// </summary>
        /// <param name="reader">文本文档读取器</param>
        /// <returns>模型文件</returns>
        public static STLDocument Read(StreamReader reader)
        {
            const string regexSolid = @"solid\s+(?<Name>[^\r\n]+)?";

            if (reader == null)
                return null;

            //Read the header.
            string header = reader.ReadLine();
            Match headerMatch = Regex.Match(header, regexSolid);
            STLDocument stl = null;
            Facet currentFacet = null;

            //Check the header.
            if (!headerMatch.Success)
                throw new FormatException("Invalid STL header");

            //Create the STL and extract the name (optional).
            stl = new STLDocument()
            {
                Name = headerMatch.Groups["Name"].Value
            };

            //Read each facet until the end of the stream.
            while ((currentFacet = FacetRead(reader)) != null)
                stl.Facets.Add(currentFacet);

            return stl;
        }

        /// <summary>
        /// 根据二进制读取文件
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        /// <returns>模型文件</returns>
        public static STLDocument Read(BinaryReader reader)
        {
            if (reader == null)
                return null;

            byte[] buffer = new byte[80];
            STLDocument stl = new STLDocument();
            Facet currentFacet = null;

            //Read (and ignore) the header and number of triangles.
            buffer = reader.ReadBytes(80);
            reader.ReadBytes(4);

            //Read each facet until the end of the stream. Stop when the end of the stream is reached.
            while ((reader.BaseStream.Position != reader.BaseStream.Length) && (currentFacet = FacetRead(reader)) != null)
            {
                stl.Facets.Add(currentFacet);
                //Math.Max()
            }

            return stl;
        }

        /// <summary>
        /// 根据文本文档读取模型面
        /// </summary>
        /// <param name="reader">文本文档读取器</param>
        /// <returns>面</returns>
        private static Facet FacetRead(StreamReader reader)
        {
            var s1 = reader.ReadLine();
            var s2 = reader.ReadLine();

            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return null;

            var vs = new List<Vector3>();

            for (int i = 0; i < 3; i++)
            {
                var line = reader.ReadLine();
                var v = line.Replace("vertex ", "");
                var s = v.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                vs.Add(new Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2])));
            }

            Facet facet = new Facet(vs[0], vs[1], vs[2]);

            reader.ReadLine();
            reader.ReadLine();

            return facet;
        }

        /// <summary>
        /// 根据二进制读取模型面
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        /// <returns>面</returns>
        private static Facet FacetRead(BinaryReader reader)
        {
            Vector3Read(reader);
            Facet facet = new Facet(Vector3Read(reader), Vector3Read(reader), Vector3Read(reader));
            reader.ReadUInt16();

            return facet;
        }

        /// <summary>
        /// 根据二进制读取面的三维点
        /// </summary>
        /// <param name="reader">二进制读取器</param>
        /// <returns>三维向量</returns>
        public static Vector3 Vector3Read(BinaryReader reader)
        {
            const int floatSize = sizeof(float);
            const int vertexSize = (floatSize * 3);

            byte[] data = new byte[vertexSize];
            int bytesRead = reader.Read(data, 0, data.Length);

            return new Vector3()
            {
                X = BitConverter.ToSingle(data, 0),
                Y = BitConverter.ToSingle(data, floatSize),
                Z = BitConverter.ToSingle(data, (floatSize * 2))
            };
        }

        /// <summary>
        /// 计算模型大小及中心点
        /// </summary>
        private void CaclSize()
        {
            var minX = Facets.Min(m => m.Vertices.Min(n => n.X));
            var maxX = Facets.Max(m => m.Vertices.Max(n => n.X));
            var minY = Facets.Min(m => m.Vertices.Min(n => n.Y));
            var maxY = Facets.Max(m => m.Vertices.Max(n => n.Y));
            var minZ = Facets.Min(m => m.Vertices.Min(n => n.Z));
            var maxZ = Facets.Max(m => m.Vertices.Max(n => n.Z));
            Size = new ModelSize(maxX - minX, maxY - minY, maxZ - minZ);
            Center = new Vector3(Size.Length / 2 + minX, Size.Width / 2 + minY, Size.Height / 2 + minZ);
        }

        /// <summary>
        /// 按中心位置重新计算每个面的位置
        /// </summary>
        public void MakeCenter()
        {
            for (int i = 0; i < Facets.Count; i++)
            {
                var vs = new List<Vector3>();
                foreach (var v in Facets[i].Vertices)
                {
                    vs.Add(v - Center);
                }

                Facets[i] = new Facet(vs);
            }
        }
    }
}