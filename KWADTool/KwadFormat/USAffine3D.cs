using System.IO;

namespace KWADTool.KwadFormat
{
    // ReSharper disable once InconsistentNaming
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
}