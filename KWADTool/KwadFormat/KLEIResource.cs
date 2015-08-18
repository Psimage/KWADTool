using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace KWADTool.KwadFormat
{
    public abstract class KLEIResource
    {
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
            Dictionary<byte[], Type> resTypes = new Dictionary<byte[], Type>(new ByteArrayComparer());

            var kleiResourceTypes = from type in Assembly.GetAssembly(typeof (KLEIResource)).GetTypes()
                                    where type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof (KLEIResource))
                                    select type;

            foreach (var type in kleiResourceTypes)
            {
                var fieldInfo = type.GetField("KLEI_TYPE", BindingFlags.Public | BindingFlags.Static);
                if (fieldInfo == null)
                {
                    throw new Exception("Required field \"KLEI_TYPE\" not found in " + type.FullName + " class");
                }
                resTypes.Add((byte[]) fieldInfo.GetValue(null), type);
            }

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