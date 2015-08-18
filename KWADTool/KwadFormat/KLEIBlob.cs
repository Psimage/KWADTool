using System.IO;
using System.Text;

namespace KWADTool.KwadFormat
{
    public class KLEIBlob : KLEIResource
    {
        public static readonly byte[] KLEI_TYPE = Encoding.ASCII.GetBytes("BLOB"); //WARNING: mutable

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