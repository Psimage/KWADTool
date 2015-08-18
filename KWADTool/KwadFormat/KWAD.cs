using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KWADTool.KwadFormat
{
    public class KWAD
    {
        public static readonly byte[] SIGNATURE_KLEI_PACKAGE = Encoding.ASCII.GetBytes("KLEIPKG2"); //WARNING: mutable

        public byte[] Signature { get; private set; }
        public uint FileSize { get; private set; }
        public uint SlabCount { get; private set; }

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

        /// <exception cref="UnexpectedSignatureException">Signature is not equal to <see cref="SIGNATURE_KLEI_PACKAGE"/>. </exception>
        public KWAD(BinaryReader reader)
        {
            Signature = reader.ReadBytes(8);
            if (!Signature.SequenceEqual(SIGNATURE_KLEI_PACKAGE))
            {
                throw new UnexpectedSignatureException(String.Format("Expected {0} but got {1}", 
                    BitConverter.ToString(SIGNATURE_KLEI_PACKAGE), BitConverter.ToString(Signature)));
            }

            FileSize = reader.ReadUInt32();
            SlabCount = reader.ReadUInt32();

            var resouceInfoListCount = reader.ReadUInt32();
            for (int i = 0; i < resouceInfoListCount; i++)
            {
                resourceInfoList.Add(new ResourceInfo(reader));
            }

            var aliasInfoListCount = reader.ReadUInt32();
            for (int i = 0; i < aliasInfoListCount; i++)
            {
                aliasInfoList.Add(new AliasInfo(reader));
            }

            resourceList = resourceInfoList.Select(resInfo =>
            {
                reader.BaseStream.Seek(resInfo.Offset, SeekOrigin.Begin);
                return KLEIResource.From(resInfo.GetType(), reader);
            }).ToList();
        }

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