using System.IO;
using System.Text;

namespace KWADTool.KwadFormat
{
    public class KLEIBuild : KLEIResource
    {
        public static readonly byte[] KLEI_TYPE = Encoding.ASCII.GetBytes("BLD1"); //WARNING: mutable

        private readonly byte[] signature;
        public byte[] GetSignature()
        {
            return (byte[])signature.Clone();
        }

        public uint StructSize { get; private set; }

        public EncodedString Name { get; private set; }

        public uint SymbolsCount { get; private set; }
        private readonly Symbol[] symbols;
        public Symbol[] GetSymbols()
        {
            return (Symbol[])symbols.Clone();
        }

        public uint SymbolFrameCount { get; private set; }
        private readonly SymbolFrame[] symbolFrames;
        public SymbolFrame[] GetSymbolFrames()
        {
            return (SymbolFrame[])symbolFrames.Clone();
        }

        public KLEIBuild(BinaryReader reader)
        {
            signature = reader.ReadBytes(8);
            StructSize = reader.ReadUInt32();

            Name = new EncodedString(reader);

            SymbolsCount = reader.ReadUInt32();
            symbols = new Symbol[SymbolsCount];
            for (var i = 0; i < SymbolsCount; i++)
            {
                symbols[i] = new Symbol(reader);
            }

            SymbolFrameCount = reader.ReadUInt32();
            symbolFrames = new SymbolFrame[SymbolFrameCount];
            for (var i = 0; i < SymbolFrameCount; i++)
            {
                symbolFrames[i] = new SymbolFrame(reader);
            }
        }

        public class Symbol
        {
            public uint Hash { get; private set; }
            private readonly char[] name;

            public byte[] GetName()
            {
                return (byte[])name.Clone();
            }

            public string GetNameString()
            {
                return new string(name);
            }

            public uint FrameIdx { get; private set; }
            public uint FrameCount { get; private set; }

            public Symbol(BinaryReader reader)
            {
                Hash = reader.ReadUInt32();
                name = reader.ReadChars(20);
                FrameIdx = reader.ReadUInt32();
                FrameCount = reader.ReadUInt32();
            }
        }

        public class SymbolFrame
        {
            public uint ModelResourceIdx { get; private set; }
            public USAffine3D Affine3D { get; private set; }

            public SymbolFrame(BinaryReader reader)
            {
                ModelResourceIdx = reader.ReadUInt32();
                Affine3D = new USAffine3D(reader);
            }

            protected bool Equals(SymbolFrame other)
            {
                return ModelResourceIdx == other.ModelResourceIdx && Affine3D.Equals(other.Affine3D);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (obj.GetType() != this.GetType()) return false;
                return Equals((SymbolFrame) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((int) ModelResourceIdx * 397) ^ Affine3D.GetHashCode();
                }
            }
        }
    }
}
