using System.IO;

namespace KWADTool.KwadFormat
{
    public class ResourceInfo
    {
        public uint SlabIdx { get; private set; }
        public uint Size { get; private set; }
        public uint Offset { get; private set; }
        private readonly byte[] type;
        public new byte[] GetType()
        {
            return (byte[])type.Clone();
        }

        public ResourceInfo(BinaryReader reader)
        {
            SlabIdx = reader.ReadUInt32();
            Size = reader.ReadUInt32();
            Offset = reader.ReadUInt32();
            type = reader.ReadBytes(4);
        }

        public ResourceInfo(byte[] type, uint slabIdx, uint size, uint offset)
        {
            SlabIdx = slabIdx;
            Size = size;
            Offset = offset;
            this.type = (byte[]) type.Clone();
        }

        /// <summary>
        /// Create ResourceInfo from KWAD Package v1
        /// </summary>
        public static ResourceInfo CreateFromKWAD1(BinaryReader reader)
        {
            var slabIdx = reader.ReadUInt32();
            var size = reader.ReadUInt32();
            var offset = reader.ReadUInt32();

            var oldPos = reader.BaseStream.Position;
            // Resource signature has prefix "KLEI" (KLEITEX1, KLEISRF1, ...) and is 8 bytes long.
            // "type" field of ResourceInfo is only 4 bytes long.
            // That is why we only use last 4 bytes of Resource Signature.
            reader.BaseStream.Seek(offset + 4, SeekOrigin.Begin);
            var type = reader.ReadBytes(4);
            reader.BaseStream.Seek(oldPos, SeekOrigin.Begin);

            return new ResourceInfo(type, slabIdx, size, offset);
        }
    }
}
