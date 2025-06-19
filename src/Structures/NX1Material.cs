using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct NX1Material
    {
        public MaterialInfo info;
        public MaterialStateBits stateBitsEntry;
        public byte TextureCount;
        public byte ConstantCount;
        public byte StateBitsCount;
        public byte StateFlags;
        public byte CameraRegion;
        public byte LayerCount;
        public uint TechniqueSet;
        public uint TextureTable;
        public uint ConstantTable;
        public uint StateBitsTable;
        public uint SubMaterials;
    };
}
