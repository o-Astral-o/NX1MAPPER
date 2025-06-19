using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER.Structures
{
    [InlineArray(12)]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct NX1XAnimPartsBoneCounts
    {
        public byte Data;
    }
}
