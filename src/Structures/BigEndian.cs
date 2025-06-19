using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NX1GAMER.Structures
{
    [DebuggerDisplay("{ToLittleEndian()}")]
    public struct BigEndian<T> where T : unmanaged
    {
        public T Data;

        public T ToLittleEndian()
        {
            Span<byte> buffer = MemoryMarshal.Cast<T, byte>(stackalloc T[1]
            {
                Data
            });
            Span<byte> newBuffer = stackalloc byte[buffer.Length];

            for (int i = 0; i < buffer.Length; i++)
            {
                var n = buffer.Length - i - 1;
                newBuffer[n] = buffer[i];
            }

            return MemoryMarshal.Cast<byte, T>(newBuffer)[0];
        }

        public static implicit operator T(BigEndian<T> d) => d.ToLittleEndian();

        public override string ToString() => ToLittleEndian().ToString() ?? "null";
    }
}
