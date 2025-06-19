using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER
{
    public class GameMemory
    {
        public List<GameMemoryHeap> Heaps { get; set; } = [];

        public bool TryGetHeap(long address, [NotNullWhen(true)] out GameMemoryHeap? heap)
        {
            foreach (var potentialHeap in Heaps)
            {
                if (address >= potentialHeap.StartAddress && address < potentialHeap.EndAddress)
                {
                    heap = potentialHeap;
                    return true;
                }
            }

            heap = null;
            return false;
        }

        public byte ReadByte(long address)
        {
            if (!TryGetHeap(address, out var heap))
                throw new Exception();

            heap.Stream.Position = address - heap.StartAddress;

            return (byte)heap.Stream.ReadByte();
        }

        public int ReadInt32(long address)
        {
            if (!TryGetHeap(address, out var heap))
                throw new Exception();

            Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<uint>()];

            heap.Stream.Position = address - heap.StartAddress;
            heap.Stream.ReadExactly(buffer);

            return BinaryPrimitives.ReadInt32BigEndian(buffer);
        }

        public uint ReadUInt32(long address)
        {
            if (!TryGetHeap(address, out var heap))
                throw new Exception();

            Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<uint>()];

            heap.Stream.Position = address - heap.StartAddress;
            heap.Stream.ReadExactly(buffer);

            return BinaryPrimitives.ReadUInt32BigEndian(buffer);
        }

        public ushort ReadUInt16(long address)
        {
            if (!TryGetHeap(address, out var heap))
                throw new Exception();

            Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<ushort>()];

            heap.Stream.Position = address - heap.StartAddress;
            heap.Stream.ReadExactly(buffer);

            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }

        public Half ReadHalf(long address)
        {
            if (!TryGetHeap(address, out var heap))
                throw new Exception();

            Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Half>()];

            heap.Stream.Position = address - heap.StartAddress;
            heap.Stream.ReadExactly(buffer);

            return BinaryPrimitives.ReadHalfBigEndian(buffer);
        }

        public float ReadSingle(long address)
        {
            if (!TryGetHeap(address, out var heap))
                throw new Exception();

            Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<float>()];

            heap.Stream.Position = address - heap.StartAddress;
            heap.Stream.ReadExactly(buffer);

            return BinaryPrimitives.ReadSingleBigEndian(buffer);
        }

        public T ReadStruct<T>(long address) where T : unmanaged
        {
            if (!TryGetHeap(address, out var heap))
                throw new Exception();

            Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];

            heap.Stream.Position = address - heap.StartAddress;
            heap.Stream.ReadExactly(buffer);

            return MemoryMarshal.Cast<byte, T>(buffer)[0];
        }

        public T ReadStruct<T>(long address, int index) where T : unmanaged
        {
            if (!TryGetHeap(address, out var heap))
                throw new Exception();

            Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];

            heap.Stream.Position = address - heap.StartAddress + index * Unsafe.SizeOf<T>();
            heap.Stream.ReadExactly(buffer);

            return MemoryMarshal.Cast<byte, T>(buffer)[0];
        }

        public string ReadNullTerminatedString(long address)
        {
            if (!TryGetHeap(address, out var heap))
                throw new Exception();

            Span<byte> buffer = stackalloc byte[4096];

            heap.Stream.Position = address - heap.StartAddress;
            
            int bytesRead = heap.Stream.Read(buffer);
            var result = new StringBuilder(4096);

            for (int i = 0; i < bytesRead; i++)
            {
                if (buffer[i] == 0)
                    break;

                result.Append(Convert.ToChar(buffer[i]));
            }

            return result.ToString();
        }

        public byte[] ReadMemory(long address, int size)
        {
            if (address == 0)
                return Array.Empty<byte>();
            if (!TryGetHeap(address, out var heap))
                throw new Exception();

            var result = new byte[size];

            heap.Stream.Position = address - heap.StartAddress;
            heap.Stream.ReadExactly(result);

            return result;
        }

        public static GameMemory LoadFromXenia(string filePath)
        {
            var memory = new GameMemory();

            using var reader = new BinaryReader(File.OpenRead(filePath));

            memory.Heaps.Add(GameMemoryHeap.LoadMemoryHeap(reader, "v40000000", 0x40000000, 0x40000000 - 0x01000000,    64 * 1024));
            memory.Heaps.Add(GameMemoryHeap.LoadMemoryHeap(reader, "v80000000", 0x80000000, 0x10000000,                 64 * 1024));
            memory.Heaps.Add(GameMemoryHeap.LoadMemoryHeap(reader, "v90000000", 0x90000000, 0x10000000,                 4096));
            memory.Heaps.Add(GameMemoryHeap.LoadMemoryHeap(reader, "vA0000000", 0xA0000000, 0x20000000,                 64 * 1024));
            memory.Heaps.Add(GameMemoryHeap.LoadMemoryHeap(reader, "vC0000000", 0xC0000000, 0x20000000,                 16 * 1024 * 1024));
            memory.Heaps.Add(GameMemoryHeap.LoadMemoryHeap(reader, "vE0000000", 0xE0000000, 0x1FD00000,                 4096));

            return memory;
        }

        public short ReadInt16(uint address)
        {
            if (!TryGetHeap(address, out var heap))
                throw new Exception();

            Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Half>()];

            heap.Stream.Position = address - heap.StartAddress;
            heap.Stream.ReadExactly(buffer);

            return BinaryPrimitives.ReadInt16BigEndian(buffer);
        }
    }
}
