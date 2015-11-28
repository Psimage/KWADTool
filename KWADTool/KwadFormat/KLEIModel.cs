using System.IO;
using System.Text;

namespace KWADTool.KwadFormat
{
    public class KLEIModel : KLEIResource
    {
        public static readonly byte[] KLEI_TYPE = Encoding.ASCII.GetBytes("MDL1"); //WARNING: mutable

        private readonly byte[] signature;

        public byte[] GetSignature()
        {
            return (byte[]) signature.Clone();
        }

        public uint TextureResourceIdx { get; private set; }

        public KLEIMesh Mesh { get; private set; }

        public KLEIModel(BinaryReader reader)
        {
            signature = reader.ReadBytes(8);
            TextureResourceIdx = reader.ReadUInt32();
            Mesh = new KLEIMesh(reader);
        }

        public class KLEIMesh
        {
            private readonly byte[] signature;

            public byte[] GetSignature()
            {
                return (byte[])signature.Clone();
            }

            public uint PolygonCount { get; private set; }
            public uint IndexCount { get; private set; }
            public uint VertexCount { get; private set; }

            private readonly int[] indecies;
            public int[] GetIndecies()
            {
                return (int[])indecies.Clone();
            }

            private readonly Vertex[] vertices;
            public int[] GetVertices()
            {
                return (int[])vertices.Clone();
            }

            public KLEIMesh(BinaryReader reader)
            {
                signature = reader.ReadBytes(8);
                PolygonCount = reader.ReadUInt32();
                IndexCount = reader.ReadUInt32();
                VertexCount = reader.ReadUInt32();

                indecies = new int[IndexCount];
                for (var i = 0; i < IndexCount; i++)
                {
                    indecies[i] = reader.ReadInt32();
                }

                vertices = new Vertex[VertexCount];
                for (var i = 0; i < VertexCount; i++)
                {
                    vertices[i] = new Vertex(reader);
                }
            }

            public class Vertex
            {
                public float X { get; private set; }
                public float Y { get; private set; }
                public float U { get; private set; }
                public float V { get; private set; }

                public Vertex(BinaryReader reader)
                {
                    X = reader.ReadSingle();
                    Y = reader.ReadSingle();
                    U = reader.ReadSingle();
                    V = reader.ReadSingle();
                }
            }
        }
    }
}
