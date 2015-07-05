using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace KWADTool.Kwad
{
    public class KLEIResource
    {
        public static readonly byte[] SIGNATURE_KLEI_BLOB_4 = Encoding.ASCII.GetBytes("BLOB"); //WARNING: mutable
        public static readonly byte[] SIGNATURE_KLEI_SURFACE_4 = Encoding.ASCII.GetBytes("SRF1");
        public static readonly byte[] SIGNATURE_KLEI_TEXTURE_4 = Encoding.ASCII.GetBytes("TEX1");

        private class ByteArrayComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] left, byte[] right)
            {
                if (left == null || right == null) 
                    return left == right;
                return left.SequenceEqual(right);
            }

            public int GetHashCode(byte[] key)
            {
                if (key == null)
                    throw new ArgumentNullException("key");
                return key.Sum(b => b);
            }
        }

        public static readonly ReadOnlyDictionary<byte[], Type> RESOURCE_TYPES;

        static KLEIResource()
        {
            Dictionary<byte[], Type> resTypes = new Dictionary<byte[], Type>(new ByteArrayComparer())
            {
                {SIGNATURE_KLEI_BLOB_4, typeof (KLEIBlob)},
                {SIGNATURE_KLEI_SURFACE_4, typeof (KLEISurface)},
                {SIGNATURE_KLEI_TEXTURE_4, typeof (KLEITexture)}
            };

            RESOURCE_TYPES = new ReadOnlyDictionary<byte[], Type>(resTypes);
        }

        public static KLEIResource From(byte[] type, BinaryReader reader)
        {
            Type resType;
            if (!RESOURCE_TYPES.TryGetValue(type, out resType)) return null;
            return (KLEIResource) Activator.CreateInstance(resType, reader);
        }
    }
}