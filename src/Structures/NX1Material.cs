using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
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
    
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct MaterialConstantDef
    {
        [FieldOffset(0x00)] 
        public uint NameHash;
        [FieldOffset(0x04)] 
        public fixed byte Name[12];
        [FieldOffset(0x10)] 
        public Vector4 Literal;

        public string GetNameFragment()
        {
            fixed (byte* p = Name)
            {
                var span = new ReadOnlySpan<byte>(p, 12);
                int len = span.IndexOf((byte)0);
                if (len < 0) len = 12;
                return Encoding.ASCII.GetString(span.Slice(0, len));
            }
        }
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TechniqueSet
    {
        public uint Name { get; set; }
        public byte WorldVertFormat { get; set; }
    }
}
