using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct XModelLodInfo
    {
        public float Dist;
        public ushort Numsurfs;
        public ushort SurfIndex;
        public uint ModelSurfs;
        public XPartBits PartBits;
        public uint Surfs;
    };
}
