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
            //Compressed size of 0 means that Mipmap data is not compressed
            public uint CompressedSize { get; private set; }
            private readonly byte[] data;

            public byte[] GetData()
            {
                return (byte[]) data.Clone();
            }

            public Mipmap(BinaryReader reader)
            {
                Size = reader.ReadUInt32();
                Width = reader.ReadUInt32();
                Height = reader.ReadUInt32();
                CompressedSize = reader.ReadUInt32();
                data = reader.ReadBytes((int) CompressedSize); //WARNING: type downgrade
            }

            public Mipmap(byte[] data, uint size, uint width, uint height, uint compressedSize)
            {
                this.data = data;
                Size = size;
                Width = width;
                Height = height;
                CompressedSize = compressedSize;
            }

            /// <summary>
            /// Create Mipmap from KWAD Package v1
            /// </summary>
            public static Mipmap CreateFromKWAD1(BinaryReader reader)
            {
                var size = reader.ReadUInt32();
                var width = reader.ReadUInt32();
                var height = reader.ReadUInt32();
                var data = reader.ReadBytes((int)size);

                return new Mipmap(data, size, width, height, 0);
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

        public KLEISurface(byte[] signature, Mipmap[] mipmaps, uint structSize, uint openGLType, uint openGLStorageType, bool isDXTCompressed, uint mipmapCount, uint totalSizeOfAllMips)
        {
            this.signature = signature;
            this.mipmaps = mipmaps;
            StructSize = structSize;
            OpenGLType = openGLType;
            OpenGLStorageType = openGLStorageType;
            IsDXTCompressed = isDXTCompressed;
            MipmapCount = mipmapCount;
            TotalSizeOfAllMips = totalSizeOfAllMips;
        }

        /// <summary>
        /// Create Surface from KWAD Package v1
        /// </summary>
        public static KLEISurface CreateFromKWAD1(BinaryReader reader)
        {
            var signature = reader.ReadBytes(8);
            var structSize = reader.ReadUInt32();

            var openGLType = reader.ReadUInt32();
            var openGLStorageType = reader.ReadUInt32();

            var compressed = reader.ReadUInt32();
            var isDXTCompressed = compressed == 1;

            var mipmapCount = reader.ReadUInt32();
            var totalSizeOfAllMips = reader.ReadUInt32();

            var mipmaps = new Mipmap[mipmapCount];
            for (var i = 0; i < mipmapCount; i++)
            {
                mipmaps[i] = Mipmap.CreateFromKWAD1(reader);
            }

            return new KLEISurface(signature, mipmaps, structSize, openGLType, openGLStorageType, isDXTCompressed, mipmapCount, totalSizeOfAllMips);
        }
    }
}