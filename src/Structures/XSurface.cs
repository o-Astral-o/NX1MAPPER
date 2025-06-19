using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct XSurface
    {
        public byte TileMode;
        public byte Deformed;
        public ushort VertCount;
        public ushort TriCount;
        public uint TriIndices;
        public ushort VertCount0;
        public ushort VertCount1;
        public ushort VertCount2;
        public ushort VertCount3;
        public uint VertBlend;
        public uint TensionData;
        public uint Flags;
        public uint Verts0;
        D3DVertexBuffer Vb0;
        public uint VertListCount;
        public uint VertList;
        D3DVertexBuffer IndexBuffer;
        XPartBits PartBits;
    };
}
