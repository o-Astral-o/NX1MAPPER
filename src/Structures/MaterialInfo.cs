using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 32)]
    public struct MaterialInfo
    {
        public uint name;
        public ushort gameFlags;
        public byte sortKey;
        public byte textureAtlasRowCount;
        public byte textureAtlasColumnCount;
        public byte textureAtlasFrameBlend;
        public byte pad0;
        public uint drawSurf0;
        public uint drawSurf1;
        public uint surfaceTypeBits;
    };
}
