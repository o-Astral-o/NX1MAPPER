using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct NX1XAnimNotifyInfo
    {
        public BigEndian<ushort> Name;
        public BigEndian<float> Time;
    };
}
