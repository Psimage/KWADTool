using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using KWADTool.Kwad;
using ManagedSquish;

namespace KWADTool
{
    public static class Program
    {
        private static int Main(string[] args)
        {
            var options = new CommandLineOptions();
            if (CommandLine.Parser.Default.ParseArgumentsStrict(args, options))
            {
                try
                {
                    if (options.Output == null)
                    {
                        options.Output = Path.GetFileName(options.Input);
                    }

                    KWAD kwad;
                    try
                    {
                        using (BinaryReader reader = new BinaryReader(File.Open(options.Input, FileMode.Open)))
                        {
                            kwad = new KWAD(reader);
                        }
                    }
                    catch (UnexpectedSignatureException)
                    {
                        Console.WriteLine("Input is not a KWAD file: Unexpected signature");
                        return 1;
                    }
                    catch (FileNotFoundException e)
                    {
                        Console.WriteLine("File not found: " + e.FileName);
                        return 1;
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine(e);
                        return 1;
                    }


                    switch (options.ExportType)
                    {
                        case ExportType.Textures:
                            ExtractTextures(kwad, options.Output);
                            break;

                        case ExportType.Blobs:
                            ExtractBlobs(kwad, options.Output);
                            break;

                        case ExportType.All:
                            ExtractTextures(kwad, options.Output);
                            ExtractBlobs(kwad, options.Output);
                            break;

                        default:
                            throw new NotImplementedException("Unhandled ExportType: " + options.ExportType);
                    }

                    Console.WriteLine("Work complete!");
                } 
                // ReSharper disable once CatchAllClause
                catch (Exception e)
                {
                    Console.WriteLine("Extraction failed: " + e);
                    return 1;
                }
            }

            return 0;
        }

        private static void ExtractBlobs(KWAD kwad, string outputPath)
        {
            var namedBlobList = (from aliasInfo in kwad.GetAliasInfoList()
                                 where kwad.GetResourceInfoList()[(int) aliasInfo.ResourceIdx].GetType().SequenceEqual(KLEIResource.SIGNATURE_KLEI_BLOB_4)
                                 select new {name = aliasInfo.AliasPath.GetString(), blob = kwad.GetResourceAt<KLEIBlob>((int) aliasInfo.ResourceIdx)}).ToList();

            Console.WriteLine("Extracting {0} blobs...", namedBlobList.Count);

            foreach (var namedBlob in namedBlobList)
            {
                Console.WriteLine(namedBlob.name);

                var path = Path.GetFullPath(Path.Combine(outputPath, namedBlob.name));

                var parentDirectory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(parentDirectory))
                {
                    Directory.CreateDirectory(parentDirectory);
                }

                using (var fileStream = File.Open(path, FileMode.Create))
                {
                    var blobData = namedBlob.blob.GetData();
                    fileStream.Write(blobData, 0, blobData.Length);
                }
            }
        }

        private static void ExtractTextures(KWAD kwad, string outputPath)
        {
            var namedTextureList = (from aliasInfo in kwad.GetAliasInfoList()
                                    where kwad.GetResourceInfoList()[(int) aliasInfo.ResourceIdx].GetType().SequenceEqual(KLEIResource.SIGNATURE_KLEI_TEXTURE_4)
                                    select new {name = aliasInfo.AliasPath.GetString(), texture = kwad.GetResourceAt<KLEITexture>((int) aliasInfo.ResourceIdx)}).ToList();

            Console.WriteLine("Extracting {0} textures...", namedTextureList.Count);

            var groupedNamedTexture = namedTextureList.GroupBy(namedTexture => namedTexture.texture.ParentSurfaceResourceIdx);
            foreach (var surfaceGroup in groupedNamedTexture)
            {
                var surfaceName = kwad.GetAliasInfoList()
                                      .First(aliasInfo => aliasInfo.ResourceIdx == surfaceGroup.Key)
                                      .AliasPath.GetString();

                Console.WriteLine(surfaceName + ":");

                var surface = kwad.GetResourceAt<KLEISurface>((int) surfaceGroup.Key);
                var mipmap = surface.GetMipmaps()[0];
                var imageData = DecompressMipmap(mipmap);

                if (surface.IsDXTCompressed)
                {
                    imageData = Squish.DecompressImage(imageData, (int) mipmap.Width, (int) mipmap.Height, SquishFlags.Dxt5);
                }

                RgbaToBgra(imageData);

                GCHandle pinnedImageData = GCHandle.Alloc(imageData, GCHandleType.Pinned);
                try
                {
                    IntPtr imageDataIntPtr = pinnedImageData.AddrOfPinnedObject();

                    using (Bitmap image = new Bitmap((int) mipmap.Width, (int) mipmap.Height, (int) mipmap.Width * 4, PixelFormat.Format32bppArgb, imageDataIntPtr))
                    {

                        foreach (var namedTexture in surfaceGroup)
                        {
                            var texture = namedTexture.texture;

                            Console.WriteLine("\t{0}", namedTexture.name);

                            var path = Path.GetFullPath(Path.Combine(outputPath, namedTexture.name));

                            var parentDirectory = Path.GetDirectoryName(path);
                            if (!string.IsNullOrWhiteSpace(parentDirectory))
                            {
                                Directory.CreateDirectory(parentDirectory);
                            }

                            using (var fileStream = File.Open(path, FileMode.Create))
                            {
                                // ReSharper disable CompareOfFloatsByEqualityOperator
                                if (texture.Affine2d.TranslateX == 0 && texture.Affine2d.TranslateY == 0 &&
                                    texture.Affine2d.C1R2 == 0 && texture.Affine2d.C2R1 == 0 &&
                                    texture.Affine2d.ScaleX == 1 && texture.Affine2d.ScaleY == 1)
                                {
                                    image.Save(fileStream, ImageFormat.Png);
                                }
                                else
                                {
                                    image.Clone(new RectangleF(texture.Affine2d.TranslateX * mipmap.Width, texture.Affine2d.TranslateY * mipmap.Height,
                                        texture.Width, texture.Height), image.PixelFormat).Save(fileStream, ImageFormat.Png);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    pinnedImageData.Free();                  
                }
            }
        }

        private static void RgbaToBgra(byte[] abgrData)
        {
            for (int i = 0; i < abgrData.Length; i += 4)
            {
                var tmp = abgrData[i];
                abgrData[i] = abgrData[i + 2];
                abgrData[i + 2] = tmp;
            }
        }

        private static byte[] DecompressMipmap(KLEISurface.Mipmap mipmap)
        {
            var compressedSurface = mipmap.GetCompressedData();
            byte[] decompressMipmap;
            using (var memoryStream = new MemoryStream((int) mipmap.Size))
            {
                using (var compressedSurfaceStream = new MemoryStream(compressedSurface, 2, compressedSurface.Length - 2))
                using (var decompressionStream = new DeflateStream(compressedSurfaceStream, CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(memoryStream);
                }

                decompressMipmap = memoryStream.ToArray();
            }
            return decompressMipmap;
        }
    }
}