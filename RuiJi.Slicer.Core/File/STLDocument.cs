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
    public class STLDocument
    {
        public IList<Facet> Facets { get; private set; }

        public string Name { get; set; }

        public STLSize Size { get; private set; }

        public Vector3 Center { get; private set; }

        public STLDocument()
        {
            Facets = new List<Facet>();
        }
        public static STLDocument Open(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            using (Stream stream = System.IO.File.OpenRead(path))
            {
                var doc = Read(stream);
                doc.CaclSize();

                return doc;
            }
        }

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

        public static bool IsText(Stream stream)
        {
            const string solid = "solid";

            byte[] buffer = new byte[5];
            string header = null;

            //Reset the stream to tbe beginning and read the first few bytes, then reset the stream to the beginning again.
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);
            stream.Seek(0, SeekOrigin.Begin);

            //Read the header as ASCII.
            header = Encoding.ASCII.GetString(buffer);

            return solid.Equals(header, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsBinary(Stream stream)
        {
            return !IsText(stream);
        }

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

        private static Facet FacetRead(StreamReader reader)
        {
            reader.ReadLine();
            reader.ReadLine();

            var vs = new List<Vector3>();

            for (int i = 0; i < 3; i++)
            {
                var v = reader.ReadLine().Replace("vertex ", "");
                var s = v.Split(new char[] { ' ' });
                vs.Add(new Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2])));
            }

            Facet facet = new Facet(vs[0],vs[1],vs[2]);

            reader.ReadLine();
            reader.ReadLine();

            return facet;
        }

        private static Facet FacetRead(BinaryReader reader)
        {
            Vector3Read(reader);
            Facet facet = new Facet(Vector3Read(reader), Vector3Read(reader), Vector3Read(reader));
            reader.ReadUInt16();

            return facet;
        }

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

        private void CaclSize()
        {
            var minX = Facets.Min(m => m.Vertices.Min(n => n.X));
            var maxX = Facets.Max(m => m.Vertices.Max(n => n.X));
            var minY = Facets.Min(m => m.Vertices.Min(n => n.Y));
            var maxY = Facets.Max(m => m.Vertices.Max(n => n.Y));
            var minZ = Facets.Min(m => m.Vertices.Min(n => n.Z));
            var maxZ = Facets.Max(m => m.Vertices.Max(n => n.Z));
            Size = new STLSize(maxX - minX, maxY - minY, maxZ - minZ);
            Center = new Vector3(Size.Length / 2 + minX, Size.Width / 2 + minY, Size.Height / 2 + minZ);
        }

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