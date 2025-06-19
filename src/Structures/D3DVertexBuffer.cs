using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER.Structures
{
    public struct D3DVertexBuffer
    {
        public uint Common;
        public uint ReferenceCount;
        public uint Fence;
        public uint ReadFence;
        public uint Identifier;
        public uint BaseFlush;
        public uint Dword0;
        public uint Dword1;
    };
}
