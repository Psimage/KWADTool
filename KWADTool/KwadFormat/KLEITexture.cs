using System.IO;
using System.Text;

namespace KWADTool.KwadFormat
{
    public class KLEITexture : KLEIResource
    {
        public static readonly byte[] KLEI_TYPE = Encoding.ASCII.GetBytes("TEX1"); //WARNING: mutable

        public class Affine2D
        {
            public float ScaleX { get; private set; }
            public float C2R1 { get; private set; }
            public float C1R2 { get; private set; }
            public float ScaleY { get; private set; }
            public float TranslateX { get; private set; }
            public float TranslateY { get; private set; }

            public Affine2D(BinaryReader reader)
            {
                ScaleX = reader.ReadSingle();
                C2R1 = reader.ReadSingle();
                C1R2 = reader.ReadSingle();
                ScaleY = reader.ReadSingle();
                TranslateX = reader.ReadSingle();
                TranslateY = reader.ReadSingle();
            }
        }

        private readonly byte[] signature;

        public byte[] GetSignature()
        {
            return (byte[]) signature.Clone();
        }

        public uint StructSize { get; private set; }

        public uint ParentSurfaceResourceIdx { get; private set; }

        public uint Width { get; private set; }
        public uint Height { get; private set; }

        // ReSharper disable once InconsistentNaming
        public Affine2D Affine2d { get; private set; }

        public KLEITexture(BinaryReader reader)
        {
            signature = reader.ReadBytes(8);
            StructSize = reader.ReadUInt32();

            ParentSurfaceResourceIdx = reader.ReadUInt32();

            Width = reader.ReadUInt32();
            Height = reader.ReadUInt32();

            Affine2d = new Affine2D(reader);
        }
    }
}