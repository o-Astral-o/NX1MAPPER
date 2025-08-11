using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using C2M;

namespace NX1GAMER.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct GfxPackedVertex
    {
        public Vector3 Xyz;
        public float binormalSign;
        public byte ColorR;
        public byte ColorG;
        public byte ColorB;
        public byte ColorA;
        public Half UVU;
        public Half UVV;
        public uint Normal;
        public uint Tangent;
    };
    
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct QSGfxVertexBuffer
    {
        public Vector3 Xyz;
        public float binormalSign;
        public GfxColor Color;
        public float U;
        public float V;
        public float LightMapU;
        public float LightMapV;
        public uint Normal;
        public uint Tangent;
    };
}
