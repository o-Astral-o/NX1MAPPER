using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER.Helpers
{
    public static class NumberExtensions
    {
        public static uint AsBE(this uint val)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, val);
            return BinaryPrimitives.ReadUInt32BigEndian(buffer);
        }

        public static ushort AsBE(this ushort val)
        {
            Span<byte> buffer = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16LittleEndian(buffer, val);
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }

        public static float AsBE(this float val)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteSingleLittleEndian(buffer, val);
            return BinaryPrimitives.ReadSingleBigEndian(buffer);
        }

        public static Half AsBE(this Half val)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteHalfLittleEndian(buffer, val);
            return BinaryPrimitives.ReadHalfBigEndian(buffer);
        }

        public static byte[] AsBuffer(this uint val)
        {
            Span<byte> buffer = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(buffer, val);
            return buffer.ToArray();
        }
    }
}
