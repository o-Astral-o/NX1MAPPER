using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MaterialTextureDef
    {
        public uint Hash;
        public byte NameStart;
        public byte NameEnd;
        public byte SamplerState;
        public byte Semantic;
        public uint Image;
        public uint Override;
    };
}
