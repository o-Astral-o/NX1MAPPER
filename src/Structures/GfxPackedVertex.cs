using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
}
