using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER
{
    public class GameMemoryHeap(string name, long startAddress, long endAddress, byte[] buffer)
    {
        public string Name { get; set; } = name;

        public long StartAddress { get; set; } = startAddress;
        public long EndAddress { get; set; } = endAddress;

        public byte[] Buffer { get; set; } = buffer;

        public MemoryStream Stream { get; } = new(buffer);

        public static GameMemoryHeap LoadMemoryHeap(BinaryReader reader, string name, long address, long size, int pageSize)
        {
            var heap = new GameMemoryHeap(name, address, address + size, new byte[size]);
            var pageCount = size / pageSize;
            var offset = 0;

            for (int i = 0; i < pageCount; i++)
            {
                var qword1 = reader.ReadUInt64();
                var state = qword1 >> 60;

                if ((state & 2) == 2)
                {
                    reader.Read(heap.Buffer, offset, pageSize);
                }

                offset += pageSize;
            }

            return heap;
        }
    }
}
