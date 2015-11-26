using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public class USAffine3D
        {
            public float C1R1 { get; private set; }
            public float C1R2 { get; private set; }
            public float C1R3 { get; private set; }

            public float C2R1 { get; private set; }
            public float C2R2 { get; private set; }
            public float C2R3 { get; private set; }

            public float C3R1 { get; private set; }
            public float C3R2 { get; private set; }
            public float C3R3 { get; private set; }

            public float C4R1 { get; private set; }
            public float C4R2 { get; private set; }
            public float C4R3 { get; private set; }

            public float GetA()
            {
                return C1R1;
            }

            public float GetB()
            {
                return C1R2;
            }

            public float GetC()
            {
                return C2R1;
            }

            public float GetD()
            {
                return C2R2;
            }

            public float GetTx()
            {
                return C4R1;
            }

            public float GetTy()
            {
                return C4R2;
            }

            public USAffine3D(BinaryReader reader)
            {
                C1R1 = reader.ReadSingle();
                C1R2 = reader.ReadSingle();
                C1R3 = reader.ReadSingle();

                C2R1 = reader.ReadSingle();
                C2R2 = reader.ReadSingle();
                C2R3 = reader.ReadSingle();

                C3R1 = reader.ReadSingle();
                C3R2 = reader.ReadSingle();
                C3R3 = reader.ReadSingle();

                C4R1 = reader.ReadSingle();
                C4R2 = reader.ReadSingle();
                C4R3 = reader.ReadSingle();
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
        }
    }
}
