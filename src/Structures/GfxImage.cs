using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct GfxImage
    {
        public GfxTexture Texture;
        public uint Format;
        public byte MapType;
        public byte Semantic;
        public byte Category;
        public uint CardMemory;
        public ushort Width;
        public ushort Height;
        public ushort Depth;
        public byte LevelCount;
        public byte Cached;
        public uint Pixels;
        public GfxImageStreamDataArray Streams;
        public uint Name;
    };
}
