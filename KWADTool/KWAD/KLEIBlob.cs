using System.IO;

namespace KWADTool.Kwad
{
    public class KLEIBlob : KLEIResource
    {
        private readonly byte[] signature;

        public byte[] GetSignature()
        {
            return (byte[]) signature.Clone();
        }

        public uint DataSize { get; private set; }

        private readonly byte[] data;

        public byte[] GetData()
        {
            return (byte[]) data.Clone();
        }

        public KLEIBlob(BinaryReader reader)
        {
            signature = reader.ReadBytes(8);
            DataSize = reader.ReadUInt32();
            data = reader.ReadBytes((int) DataSize); //WARNING: Type downgrade
        }
    }
}