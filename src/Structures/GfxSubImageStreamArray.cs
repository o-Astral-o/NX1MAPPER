using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER.Structures
{
    [InlineArray(4)]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct GfxSubImageStreamArray
    {
        public GfxSubImageStream Data;
    }
}
