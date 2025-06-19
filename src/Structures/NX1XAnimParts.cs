using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct XAnimParts
    {
        public BigEndian<uint> Name;
        public BigEndian<ushort> DataByteCount;
        public BigEndian<ushort> DataShortCount;
        public BigEndian<ushort> DataIntCount;
        public BigEndian<ushort> RandomDataByteCount;
        public BigEndian<ushort> RandomDataIntCount;
        public BigEndian<ushort> Numframes;
        public byte Flags;
        public NX1XAnimPartsBoneCounts BoneCounts;
        public byte NotifyCount;
        public byte AssetType;
        public byte Pad;
        public BigEndian<int> RandomDataShortCount;
        public BigEndian<int> IndexCount;
        public BigEndian<float> Framerate;
        public BigEndian<float> Frequency;
        public BigEndian<uint> Names;
        public BigEndian<uint> DataByte;
        public BigEndian<uint> DataShort;
        public BigEndian<uint> DataInt;
        public BigEndian<uint> RandomDataShort;
        public BigEndian<uint> RandomDataByte;
        public BigEndian<uint> RandomDataInt;
        public BigEndian<uint> Indices;
        public BigEndian<uint> Notify;
        public BigEndian<uint> DeltaPart;
    };
}
