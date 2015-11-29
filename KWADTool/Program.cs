using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using KWADTool.KwadFormat;
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
                        options.Output = Path.GetFileName(options.Input + ".d");
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
                        case ExportType.Anims:
                            ExtractTextures(kwad, options.Output);
                            ExtractAnims(kwad, options.Output);
                            break;

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

        private static void ExtractAnims(KWAD kwad, string outputPath)
        {
            var animBundleList = (from aliasInfo in kwad.GetAliasInfoList()
                                 where kwad.GetResourceInfoList()[(int)aliasInfo.ResourceIdx].GetType().SequenceEqual(KLEIAnimation.KLEI_TYPE)
                                  select new
                                  {
                                      name = Path.GetFileNameWithoutExtension(aliasInfo.AliasPath.GetString()),
                                      path = Path.GetDirectoryName(aliasInfo.AliasPath.GetString()),
                                      animDef = kwad.GetResourceAt<KLEIAnimation>((int)aliasInfo.ResourceIdx),
                                      animBld = kwad.GetResourceByAlias<KLEIBuild>(Path.ChangeExtension(aliasInfo.AliasPath.GetString(), ".abld"))
                                      //namedTextureList = (from innerAliasInfo in kwad.GetAliasInfoList()
                                      //      where kwad.GetResourceInfoList()[(int) innerAliasInfo.ResourceIdx].GetType().SequenceEqual(KLEITexture.KLEI_TYPE) &&
                                      //            // ReSharper disable once PossibleNullReferenceException
                                      //            Path.GetDirectoryName(innerAliasInfo.AliasPath.GetString()).Equals(
                                      //            Path.GetDirectoryName(Path.Combine(Path.ChangeExtension(aliasInfo.AliasPath.GetString(), ".anim"), "dummy")),
                                      //            //GetDirectoryName has a side effect of changing separator character.
                                      //            //That is why it is used on a second path to effect both.
                                      //            StringComparison.InvariantCultureIgnoreCase)
                                      //      select new
                                      //      {
                                      //          name = Path.GetFileName(innerAliasInfo.AliasPath.GetString()), 
                                      //          texture = kwad.GetResourceAt<KLEITexture>((int) innerAliasInfo.ResourceIdx)
                                      //      }).ToList()
                                  }).ToList();

            Console.WriteLine("Extracting {0} animation bundles...", animBundleList.Count);

            foreach (var animBundle in animBundleList)
            {
                var tempOutputPath = Path.Combine(Path.GetTempPath(), animBundle.name);
                if (!string.IsNullOrWhiteSpace(tempOutputPath))
                {
                    Directory.CreateDirectory(tempOutputPath);
                }
                
                if (animBundle.animBld != null)
                {
                    ExtractAnimationBuild(kwad, animBundle.animBld, tempOutputPath, outputPath);
                }
            }
        }

        private static void ExtractAnimationBuild(KWAD kwad, KLEIBuild animBld, string outputPath, string extractionOutputPath)
        {
            if (animBld == null)
            {
                throw new ArgumentNullException("animBld");
            }

            Console.WriteLine("\tAnimation build \"{0}\":", animBld.Name.GetString());

            XmlDocument buildXml = new XmlDocument();
            XmlElement buildElement = buildXml.CreateElement("Build");
            buildElement.SetAttribute("name", animBld.Name.GetString());
            buildXml.AppendChild(buildElement);

            foreach (var symbol in animBld.GetSymbols())
            {
                Console.WriteLine("\t\tSymbol \"{0}\":", symbol.GetNameString());

                XmlElement symbolElement = buildXml.CreateElement("Symbol");
                symbolElement.SetAttribute("name", symbol.GetNameString());

                uint frameNum = 0;
                for (var frameIdx = (int) symbol.FrameIdx; frameIdx < symbol.FrameIdx+symbol.FrameCount; frameIdx++)
                {
                    Console.WriteLine("\t\t\tFrame {0}", frameIdx);

                    var frame = animBld.GetSymbolFrames()[frameIdx];

                    //Some frames have no references to the actual model.
                    if (frame.ModelResourceIdx == uint.MaxValue)
                    {
                        Console.WriteLine("\t\t\tSkipping Frame {0}: Model reference not found", frameIdx);
                        frameNum++;
                        continue;
                    }

                    XmlElement symbolFrameElement = buildXml.CreateElement("Frame");

                    symbolFrameElement.SetAttribute("framenum", frameNum.ToString());
                    symbolFrameElement.SetAttribute("duration", "1");

                    var model = kwad.GetResourceAt<KLEIModel>((int) frame.ModelResourceIdx);

                    var leftOffset = 0;
                    var topOffset = 0;

                    //Some meshes have 0 vertex count
                    if (model.Mesh.VertexCount > 1)
                    {
                        leftOffset = (int)model.Mesh.GetVertices()[0].X;
                        topOffset = (int)model.Mesh.GetVertices()[0].Y;                        
                    }

                    var imageRelativePath = kwad.GetAliasInfoList().First(aliasInfo => aliasInfo.ResourceIdx == model.TextureResourceIdx).AliasPath.GetString();
                    var imageFullPath = Path.GetFullPath(Path.Combine(extractionOutputPath, imageRelativePath.TrimStart(@"\/".ToCharArray())));
                    var baseframeImageName = Path.GetFileNameWithoutExtension(imageRelativePath);
                    var frameImageName = baseframeImageName;

                    int frameImageWidth;
                    int frameImageHeight;

                    using (var textureImage = Image.FromFile(imageFullPath))
                    {
                        frameImageWidth = ((textureImage.Width % 2) == 0 ? textureImage.Width : (textureImage.Width + 1)) + leftOffset * 2;
                        frameImageHeight = ((textureImage.Height % 2) == 0 ? textureImage.Height : (textureImage.Height + 1)) + topOffset * 2;
                        
                        using (var bitmap = new Bitmap(frameImageWidth, frameImageHeight, PixelFormat.Format32bppArgb))
                        {
                            bitmap.MakeTransparent();
                            using (var graphics = Graphics.FromImage(bitmap))
                            {
                                graphics.DrawImage(textureImage, leftOffset, topOffset);
                            }

                            int imagePostfix = 1;
                            do
                            {
                                var frameImagePath = Path.GetFullPath(Path.Combine(outputPath, frameImageName + ".png"));
                            
                                if (File.Exists(frameImagePath))
                                {
                                    using (var img = Image.FromFile(frameImagePath))
                                    {
                                        if (img.Width == frameImageWidth && img.Height == frameImageHeight)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            frameImageName = baseframeImageName + "-" + imagePostfix;
                                            imagePostfix++;
                                        }
                                    }

                                }
                                else
                                {
                                    bitmap.Save(frameImagePath, ImageFormat.Png);
                                    break;
                                }
                            } while (true);
                        }
                    }

                    symbolFrameElement.SetAttribute("image", frameImageName);
                    symbolFrameElement.SetAttribute("w", frameImageWidth.ToString());
                    symbolFrameElement.SetAttribute("h", frameImageHeight.ToString());

                    symbolFrameElement.SetAttribute("x", (frame.Affine3D.GetTx() + (float)frameImageWidth / 2).ToString(CultureInfo.InvariantCulture));
                    symbolFrameElement.SetAttribute("y", (frame.Affine3D.GetTy() + (float)frameImageHeight / 2).ToString(CultureInfo.InvariantCulture));

                    symbolElement.AppendChild(symbolFrameElement);
                    frameNum++;
                }

                buildElement.AppendChild(symbolElement);
            }

            buildXml.Save(Path.Combine(outputPath, "build.xml"));

            var groupedSymbolFrames = animBld.GetSymbolFrames().GroupBy(symbolFrame => symbolFrame.ModelResourceIdx);
            foreach (var groupedSymbolFrame in groupedSymbolFrames)
            {
                //groupedSymbolFrame.
            }
        }

        private static void ExtractBlobs(KWAD kwad, string outputPath)
        {
            var namedBlobList = (from aliasInfo in kwad.GetAliasInfoList()
                                 where kwad.GetResourceInfoList()[(int) aliasInfo.ResourceIdx].GetType().SequenceEqual(KLEIBlob.KLEI_TYPE)
                                 select new {name = aliasInfo.AliasPath.GetString(), blob = kwad.GetResourceAt<KLEIBlob>((int) aliasInfo.ResourceIdx)}).ToList();

            Console.WriteLine("Extracting {0} blobs...", namedBlobList.Count);

            foreach (var namedBlob in namedBlobList)
            {
                Console.WriteLine(namedBlob.name);

                var path = Path.GetFullPath(Path.Combine(outputPath, namedBlob.name.TrimStart(@"\/".ToCharArray())));

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
                                    where kwad.GetResourceInfoList()[(int) aliasInfo.ResourceIdx].GetType().SequenceEqual(KLEITexture.KLEI_TYPE)
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

                            var path = Path.GetFullPath(Path.Combine(outputPath, namedTexture.name.TrimStart(@"\/".ToCharArray())));

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