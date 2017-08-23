using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KWADTool.KwadFormat
{
    public class KWAD
    {
        public static readonly byte[] SIGNATURE_KLEI_PACKAGE_1 = Encoding.ASCII.GetBytes("KLEIPKG1"); //WARNING: mutable
        public static readonly byte[] SIGNATURE_KLEI_PACKAGE_2 = Encoding.ASCII.GetBytes("KLEIPKG2"); //WARNING: mutable

        public static readonly int PACKAGE_VERSION_1 = 1;
        public static readonly int PACKAGE_VERSION_2 = 2;

        public byte[] Signature { get; private set; }
        public uint FileSize { get; private set; }
        public uint SlabCount { get; private set; }
        public int Version { get; private set; }

        private readonly List<ResourceInfo> resourceInfoList = new List<ResourceInfo>();

        public IReadOnlyList<ResourceInfo> GetResourceInfoList()
        {
            return resourceInfoList.AsReadOnly();
        }

        private readonly List<AliasInfo> aliasInfoList = new List<AliasInfo>();

        public IReadOnlyList<AliasInfo> GetAliasInfoList()
        {
            return aliasInfoList.AsReadOnly();
        }

        private readonly List<KLEIResource> resourceList;

        /// <exception cref="UnexpectedSignatureException">Signature doesn't match
        /// <see cref="SIGNATURE_KLEI_PACKAGE_1"/> or <see cref="SIGNATURE_KLEI_PACKAGE_2"/>. </exception>
        public KWAD(BinaryReader reader)
        {
            Signature = reader.ReadBytes(8);
            if (!Signature.SequenceEqual(SIGNATURE_KLEI_PACKAGE_2) && !Signature.SequenceEqual(SIGNATURE_KLEI_PACKAGE_1))
            {
                throw new UnexpectedSignatureException(String.Format("Expected {0} or {1} but got {2}",
                    BitConverter.ToString(SIGNATURE_KLEI_PACKAGE_1),
                    BitConverter.ToString(SIGNATURE_KLEI_PACKAGE_2),
                    BitConverter.ToString(Signature)));
            }

            Version = Signature.SequenceEqual(SIGNATURE_KLEI_PACKAGE_1) ? PACKAGE_VERSION_1 : PACKAGE_VERSION_2;

            FileSize = reader.ReadUInt32();
            SlabCount = reader.ReadUInt32();

            var resouceInfoListCount = reader.ReadUInt32();
            for (int i = 0; i < resouceInfoListCount; i++)
            {
                var resourceInfo = Version == PACKAGE_VERSION_1 ? ResourceInfo.CreateFromKWAD1(reader) : new ResourceInfo(reader);
                resourceInfoList.Add(resourceInfo);
            }

            var aliasInfoListCount = reader.ReadUInt32();
            for (int i = 0; i < aliasInfoListCount; i++)
            {
                aliasInfoList.Add(new AliasInfo(reader));
            }

            resourceList = resourceInfoList.Select(resInfo =>
            {
                reader.BaseStream.Seek(resInfo.Offset, SeekOrigin.Begin);

                try
                {
                    if (Version == PACKAGE_VERSION_1)
                    {
                        if (resInfo.GetType().SequenceEqual(KLEISurface.KLEI_TYPE))
                        {
                            return KLEISurface.CreateFromKWAD1(reader);
                        }
                        // Do not parse animation types since thay have a lot of differences with KWAD version 2
                        else if (resInfo.GetType().SequenceEqual(KLEIAnimation.KLEI_TYPE))
                        {
                            return null;
                        }
                    }

                    return KLEIResource.From(resInfo.GetType(), reader);
                }
                catch (Exception)
                {
                    Console.WriteLine("WARN: Failed to parse resource of type {0} at offset {1:X}",
                        Encoding.ASCII.GetString(resInfo.GetType()), resInfo.Offset);
                    return null;
                }
            }).ToList();
        }

        /// <returns>Can return NULL in cases where specified resource at index was not parsed or had parsing errors</returns>
        public T GetResourceAt<T>(int idx) where T : KLEIResource
        {
            return (T) resourceList[idx];
        }

        public T GetResourceByAlias<T>(string alias) where T : KLEIResource
        {
            var foundAlias = aliasInfoList.DefaultIfEmpty(null).FirstOrDefault(a => a.AliasPath.GetString().Equals(alias));
            if (foundAlias == null) return null;

            return GetResourceAt<T>((int) foundAlias.ResourceIdx);
        }
    }
}