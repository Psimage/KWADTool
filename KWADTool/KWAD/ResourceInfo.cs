using System.IO;

namespace KWADTool.Kwad
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

        public ResourceInfo(uint slabIdx, uint size, uint offset, byte[] type)
        {
            SlabIdx = slabIdx;
            Size = size;
            Offset = offset;
            this.type = (byte[])type.Clone();
        }
    }
}
