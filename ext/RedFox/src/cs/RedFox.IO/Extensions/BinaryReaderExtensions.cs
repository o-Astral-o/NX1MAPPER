using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;

namespace RedFox.IO.Extensions
{
    public static class BinaryReaderExtensions
    {
        public static ushort ReadUInt16BE(this BinaryReader binaryReader)
        {
            Span<byte> buf = stackalloc byte[Unsafe.SizeOf<ushort>()];
            binaryReader.BaseStream.ReadExactly(buf);
            return BinaryPrimitives.ReadUInt16BigEndian(buf);
        }
        public static short ReadInt16BE(this BinaryReader binaryReader)
        {
            Span<byte> buf = stackalloc byte[Unsafe.SizeOf<short>()];
            binaryReader.BaseStream.ReadExactly(buf);
            return BinaryPrimitives.ReadInt16BigEndian(buf);
        }
        public static uint ReadUInt32BE(this BinaryReader binaryReader)
        {
            Span<byte> buf = stackalloc byte[Unsafe.SizeOf<uint>()];
            binaryReader.BaseStream.ReadExactly(buf);
            return BinaryPrimitives.ReadUInt32BigEndian(buf);
        }
        public static int ReadInt32BE(this BinaryReader binaryReader)
        {
            Span<byte> buf = stackalloc byte[Unsafe.SizeOf<int>()];
            binaryReader.BaseStream.ReadExactly(buf);
            return BinaryPrimitives.ReadInt32BigEndian(buf);
        }
        public static ulong ReadUInt64BE(this BinaryReader binaryReader)
        {
            Span<byte> buf = stackalloc byte[Unsafe.SizeOf<ulong>()];
            binaryReader.BaseStream.ReadExactly(buf);
            return BinaryPrimitives.ReadUInt64BigEndian(buf);
        }
        public static long ReadInt64BE(this BinaryReader binaryReader)
        {
            Span<byte> buf = stackalloc byte[Unsafe.SizeOf<long>()];
            binaryReader.BaseStream.ReadExactly(buf);
            return BinaryPrimitives.ReadInt64BigEndian(buf);
        }
        public static float ReadSingleBE(this BinaryReader binaryReader)
        {
            Span<byte> buf = stackalloc byte[Unsafe.SizeOf<float>()];
            binaryReader.BaseStream.ReadExactly(buf);
            return BinaryPrimitives.ReadSingleBigEndian(buf);
        }
        public static double ReadDoubleBE(this BinaryReader binaryReader)
        {
            Span<byte> buf = stackalloc byte[Unsafe.SizeOf<double>()];
            binaryReader.BaseStream.ReadExactly(buf);
            return BinaryPrimitives.ReadDoubleBigEndian(buf);
        }

        public static long ReadInt40BE(this BinaryReader binaryReader)
        {
            Span<byte> buf = stackalloc byte[5];
            binaryReader.BaseStream.ReadExactly(buf);

            long r = 0;

            r |= (long)buf[4] << 0;
            r |= (long)buf[3] << 8;
            r |= (long)buf[2] << 16;
            r |= (long)buf[1] << 24;
            r |= (long)buf[0] << 32;

            return r;
        }
    }
}
