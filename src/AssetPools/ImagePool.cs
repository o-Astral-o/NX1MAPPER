using Husky;
using NX1GAMER.Helpers;
using NX1GAMER.Structures;
using RedFox.Graphics3D;
using RedFox.Graphics3D.Skeletal;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace NX1GAMER.AssetPools
{
    public class ImagePool(GameInstance instance, string name, int index, int headerSize) : AssetPool(instance, name, index, headerSize)
    {
        public override void LoadGeneric()
        {
            var poolSize = Instance.GetPoolSize(Index);
            var poolAddress = Instance.GetPoolAddress(Index) + 12;
            var poolEndAddress = poolSize * HeaderSize + poolAddress;

            for (int i = 0; i < poolSize; i++)
            {
                var ptr = Instance.Memory.ReadUInt32(poolAddress + i * HeaderSize);

                if (ptr >= poolAddress && ptr < poolEndAddress)
                    continue;
                if (ptr == 0)
                    continue;

                Assets.Add(new(Instance.Memory.ReadNullTerminatedString(ptr), Name, poolAddress + i * HeaderSize, this));
            }
        }

        private static byte[] ConvertToLinearTexture(byte[] data, int _width, int _height, DirectXTexUtility.DXGIFormat format)
        {
            // https://github.com/x1nixmzeng/project-grabbed/blob/99a59b9f515a004a38df410b518c1ef5ab015fb3/src/img/converters.cpp#L108
            byte[] destData = new byte[data.Length];

            int blockSize;
            int texelPitch;

            switch (format)
            {
                case DirectXTexUtility.DXGIFormat.BC1UNORM:
                    blockSize = 4;
                    texelPitch = 8;
                    break;
                case DirectXTexUtility.DXGIFormat.BC2UNORM:
                case DirectXTexUtility.DXGIFormat.BC3UNORM:
                case DirectXTexUtility.DXGIFormat.BC5UNORM:
                    blockSize = 4;
                    texelPitch = 16;
                    break;
                //case TEXTURE_FORMAT_A8R8G8B8:
                //    blockSize = 1;
                //    texelPitch = 4;
                //    break;
                //case TEXTURE_FORMAT_X4R4G4B4:
                //    blockSize = 1;
                //    texelPitch = 2;
                //    break;
                //case TEXTURE_FORMAT_R5G6B5:
                //    blockSize = 1;
                //    texelPitch = 2;
                //    break;
                default:
                    throw new ArgumentOutOfRangeException("Bad texture type!");
            }

            int blockWidth = _width / blockSize;
            int blockHeight = _height / blockSize;

            for (int j = 0; j < blockHeight; j++)
            {
                for (int i = 0; i < blockWidth; i++)
                {
                    int blockOffset = j * blockWidth + i;

                    int x = XGAddress2DTiledX(blockOffset, blockWidth, texelPitch);
                    int y = XGAddress2DTiledY(blockOffset, blockWidth, texelPitch);

                    int srcOffset = j * blockWidth * texelPitch + i * texelPitch;
                    int destOffset = y * blockWidth * texelPitch + x * texelPitch;
                    //TODO: ConvertToLinearTexture apparently breaks on on textures with a height of 64...
                    if (destOffset >= destData.Length) continue;
                    Array.Copy(data, srcOffset, destData, destOffset, texelPitch);
                }
            }

            return destData;
        }

        private static int XGAddress2DTiledX(int Offset, int Width, int TexelPitch)
        {
            int AlignedWidth = (Width + 31) & ~31;

            int LogBpp = (TexelPitch >> 2) + ((TexelPitch >> 1) >> (TexelPitch >> 2));
            int OffsetB = Offset << LogBpp;
            int OffsetT = ((OffsetB & ~4095) >> 3) + ((OffsetB & 1792) >> 2) + (OffsetB & 63);
            int OffsetM = OffsetT >> (7 + LogBpp);

            int MacroX = ((OffsetM % (AlignedWidth >> 5)) << 2);
            int Tile = ((((OffsetT >> (5 + LogBpp)) & 2) + (OffsetB >> 6)) & 3);
            int Macro = (MacroX + Tile) << 3;
            int Micro = ((((OffsetT >> 1) & ~15) + (OffsetT & 15)) & ((TexelPitch << 3) - 1)) >> LogBpp;

            return Macro + Micro;
        }

        private static int XGAddress2DTiledY(int Offset, int Width, int TexelPitch)
        {
            int AlignedWidth = (Width + 31) & ~31;

            int LogBpp = (TexelPitch >> 2) + ((TexelPitch >> 1) >> (TexelPitch >> 2));
            int OffsetB = Offset << LogBpp;
            int OffsetT = ((OffsetB & ~4095) >> 3) + ((OffsetB & 1792) >> 2) + (OffsetB & 63);
            int OffsetM = OffsetT >> (7 + LogBpp);

            int MacroY = ((OffsetM / (AlignedWidth >> 5)) << 2);
            int Tile = ((OffsetT >> (6 + LogBpp)) & 1) + (((OffsetB & 2048) >> 10));
            int Macro = (MacroY + Tile) << 3;
            int Micro = ((((OffsetT & (((TexelPitch << 6) - 1) & ~31)) + ((OffsetT & 15) << 1)) >> (3 + LogBpp)) & ~1);

            return Macro + Micro + ((OffsetT & 16) >> 4);
        }

        public static byte[] Decompress(byte[] input, int decompressedSize, int dataOffset = 2)
        {
            using var stream = new MemoryStream(decompressedSize);
            using var compressed = new MemoryStream(input)
            {
                Position = dataOffset
            };
            using var deflater = new DeflateStream(compressed, CompressionMode.Decompress);
            deflater.CopyTo(stream);
            return stream.ToArray();
        }

        public static byte[] LoadImageData(GameInstance instance, GfxSubImageStream part)
        {
            var fileName = instance.Memory.ReadNullTerminatedString(part.File.AsBE() + (instance.Config.IsMP ? 8 : 4));
            var filePath = Path.Combine(instance.GameFolder, fileName);

            if (File.Exists(filePath + ".ffm"))
                filePath += ".ffm";
            if (File.Exists(filePath + ".ff"))
                filePath += ".ff";

            var fileSize = (int)(part.FileOffsetEnd.AsBE() - part.FileOffset.AsBE());
            var fileBuff = new byte[fileSize];
            using var stream = File.OpenRead(filePath);

            stream.Position = part.FileOffset.AsBE();
            stream.ReadExactly(fileBuff);

            return Decompress(fileBuff, 0);
        }

        public static DirectXTexUtility.DXGIFormat ConvertToDXGIFormat(uint format)
        {
            switch(format)
            {
                case 0x1A200152:
                    return DirectXTexUtility.DXGIFormat.BC1UNORM;
                case 0x1A200153:
                    return DirectXTexUtility.DXGIFormat.BC2UNORM;
                case 0x1A200154:
                    return DirectXTexUtility.DXGIFormat.BC3UNORM;
                case 0x1A200171:
                    return DirectXTexUtility.DXGIFormat.BC5UNORM;
                default:
                    return DirectXTexUtility.DXGIFormat.BC1UNORM;
            }
        }

        public override void Export(Asset asset)
        {
            ExportImage(Instance, asset.Address, Path.Combine("exported_files", "image"));
        }

        public static string ExportImage(GameInstance instance, long address, string folder)
        {
            Directory.CreateDirectory(folder);

            var imageHeader = instance.Memory.ReadStruct<GfxImage>(address);
            var imageIndex = instance.GetImageIndex(address);
            var streamInfo = instance.Memory.ReadStruct<GfxImageStream>(instance.Config.GetAddress("ImageStreamTableAddress") + imageIndex * 64);
            var imageName = instance.Memory.ReadNullTerminatedString(imageHeader.Name.AsBE());
            var fullPath = Path.Combine(folder, imageName + ".dds");
            var highestIdx = -1;

            if (File.Exists(fullPath))
                return fullPath;

            // Find the highest mip index
            for (int i = 3; i >= 0; i--)
            {
                if (imageHeader.Streams[i].Width != 0)
                {
                    highestIdx = i;
                    break;
                }
            }

            if (highestIdx == -1)
                return string.Empty;

            var data = LoadImageData(instance, streamInfo.Parts[highestIdx]);

            //Console.WriteLine(Instance.GetImageIndex(asset.Address));

            //uint fullSize = 0;
            //for (int i = 0; i < 4; i++)
            //{
            //    if (i == 0)
            //    {
            //        fullSize += imageHeader.Streams[i].PixelSize.AsBE() & 0x3FFFFFF;
            //    }
            //    else
            //    {
            //        fullSize += (imageHeader.Streams[i].PixelSize.AsBE() & 0x3FFFFFF) - (imageHeader.Streams[i - 1].PixelSize.AsBE() & 0x3FFFFFF);
            //    }
            //    Console.WriteLine(imageHeader.Streams[i].PixelSize.AsBE() & 0x3FFFFFF);
            //}

            //Console.WriteLine(fullSize);

            //Console.WriteLine();
            //Console.WriteLine();
            //Console.WriteLine();
            using var writer = new BinaryWriter(File.Create(fullPath));

            //Console.WriteLine(asset.Name);
            //Console.WriteLine(imageHeader.Format.AsBE().ToString("X"));
            //Console.WriteLine(imageHeader.Width.AsBE());
            //Console.WriteLine(imageHeader.Height.AsBE());
            //Console.WriteLine(imageHeader.CardMemory.AsBE());

            //DirectXTexUtility.ComputePitch(DirectXTexUtility.DXGIFormat.BC3UNORM, imageHeader.Width.AsBE(), imageHeader.Height.AsBE(), out var r1, out var s2, DirectXTexUtility.CPFLAGS.NONE);

            // Generate DDS Header
            DirectXTexUtility.GenerateDDSHeader(
                DirectXTexUtility.GenerateMataData(imageHeader.Streams[highestIdx].Width.AsBE(), imageHeader.Streams[highestIdx].Height.AsBE(), 1, ConvertToDXGIFormat(imageHeader.Format.AsBE()), false),
                DirectXTexUtility.DDSFlags.NONE,
                out var ddsHeader,
                out var dx10Header);
            // Encode DDS Header
            var encodedHeader = DirectXTexUtility.EncodeDDSHeader(ddsHeader, dx10Header);

            var newData = new byte[data.Length];
            var blocks = data.Length / 2;

            for (int i = 0; i < blocks; i++)
            {
                newData[i * 2 + 0] = data[i * 2 + 1];
                newData[i * 2 + 1] = data[i * 2 + 0];
            }

            //Console.WriteLine(blocks);

            var final = ConvertToLinearTexture(newData, imageHeader.Streams[highestIdx].Width.AsBE(), imageHeader.Streams[highestIdx].Height.AsBE(), ConvertToDXGIFormat(imageHeader.Format.AsBE()));

            //// Done
            writer.Write(encodedHeader);
            writer.Write(final);

            return fullPath;
        }
    }
}
