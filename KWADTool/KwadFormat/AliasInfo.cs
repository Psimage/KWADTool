using System.IO;

namespace KWADTool.KwadFormat
{
    public class AliasInfo
    {
        public EncodedString AliasPath { get; private set; }
        public uint ResourceIdx { get; private set; }

        public AliasInfo(BinaryReader reader)
        {
            AliasPath = new EncodedString(reader);
            ResourceIdx = reader.ReadUInt32();
        }
    }
}