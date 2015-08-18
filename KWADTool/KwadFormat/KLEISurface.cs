using System.IO;
using System.Text;

namespace KWADTool.KwadFormat
{
    public class KLEISurface : KLEIResource
    {
        public static readonly byte[] KLEI_TYPE = Encoding.ASCII.GetBytes("SRF1"); //WARNING: mutable

        public class Mipmap
        {
            public uint Size { get; private set; }
            public uint Width { get; private set; }
            public uint Height { get; private set; }
            public uint CompressedSize { get; private set; }
            private readonly byte[] compressedData;

            public byte[] GetCompressedData()
            {
                return (byte[]) compressedData.Clone();
            }

            public Mipmap(BinaryReader reader)
            {
                Size = reader.ReadUInt32();
                Width = reader.ReadUInt32();
                Height = reader.ReadUInt32();
                CompressedSize = reader.ReadUInt32();
                compressedData = reader.ReadBytes((int) CompressedSize); //WARNING: type downgrade
            }
        }

        private readonly byte[] signature;

        public byte[] GetSignature()
        {
            return (byte[]) signature.Clone();
        }

        public uint StructSize { get; private set; }

        public uint OpenGLType { get; private set; }
        public uint OpenGLStorageType { get; private set; }

        public bool IsDXTCompressed { get; private set; }

        public uint MipmapCount { get; private set; }
        public uint TotalSizeOfAllMips { get; private set; }

        private readonly Mipmap[] mipmaps;

        public Mipmap[] GetMipmaps()
        {
            return (Mipmap[]) mipmaps.Clone();
        }

        public KLEISurface(BinaryReader reader)
        {
            signature = reader.ReadBytes(8);
            StructSize = reader.ReadUInt32();

            OpenGLType = reader.ReadUInt32();
            OpenGLStorageType = reader.ReadUInt32();

            var compressed = reader.ReadUInt32();
            IsDXTCompressed = compressed == 1;

            MipmapCount = reader.ReadUInt32();
            TotalSizeOfAllMips = reader.ReadUInt32();

            mipmaps = new Mipmap[MipmapCount];
            for (var i = 0; i < MipmapCount; i++)
            {
                mipmaps[i] = new Mipmap(reader);
            }
        }
    }
}