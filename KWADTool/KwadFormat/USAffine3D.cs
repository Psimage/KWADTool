using System.IO;

namespace KWADTool.KwadFormat
{
    // ReSharper disable once InconsistentNaming
    public class USAffine3D
    {
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

        protected bool Equals(USAffine3D other)
        {
            return C1R1.Equals(other.C1R1) && C1R2.Equals(other.C1R2) && C1R3.Equals(other.C1R3) &&
                   C2R1.Equals(other.C2R1) && C2R2.Equals(other.C2R2) && C2R3.Equals(other.C2R3) &&
                   C3R1.Equals(other.C3R1) && C3R2.Equals(other.C3R2) && C3R3.Equals(other.C3R3) &&
                   C4R1.Equals(other.C4R1) && C4R2.Equals(other.C4R2) && C4R3.Equals(other.C4R3);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (obj.GetType() != GetType()) return false;
            return Equals((USAffine3D) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = C1R1.GetHashCode();
                hashCode = (hashCode * 397) ^ C1R2.GetHashCode();
                hashCode = (hashCode * 397) ^ C1R3.GetHashCode();
                hashCode = (hashCode * 397) ^ C2R1.GetHashCode();
                hashCode = (hashCode * 397) ^ C2R2.GetHashCode();
                hashCode = (hashCode * 397) ^ C2R3.GetHashCode();
                hashCode = (hashCode * 397) ^ C3R1.GetHashCode();
                hashCode = (hashCode * 397) ^ C3R2.GetHashCode();
                hashCode = (hashCode * 397) ^ C3R3.GetHashCode();
                hashCode = (hashCode * 397) ^ C4R1.GetHashCode();
                hashCode = (hashCode * 397) ^ C4R2.GetHashCode();
                hashCode = (hashCode * 397) ^ C4R3.GetHashCode();
                return hashCode;
            }
        }
    }
}