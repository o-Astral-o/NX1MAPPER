using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct XRigidVertList
    {
        public ushort BoneOffset;
        public ushort VertCount;
        public ushort TriOffset;
        public ushort TriCount;
        public uint CollisionTree;
    };
}
