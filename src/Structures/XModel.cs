using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER.Structures
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct XModel
    {
        public uint Name;
        public byte NumBones;
        public byte NumRootBones;
        public byte Numsurfs;
        public float Scale;
        public uint NoScalePartBits0;
        public uint NoScalePartBits1;
        public uint NoScalePartBits2;
        public uint NoScalePartBits3;
        public uint NoScalePartBits4;
        public uint NoScalePartBits5;
        public uint BoneNames;
        public uint ParentList;
        public uint Quats;
        public uint Trans;
        public uint PartClassification;
        public uint BaseMat;
        public uint MaterialHandles;
        public XModelLodInfoArray LodInfo;
        public byte MaxLoadedLod;
        public byte NumLods;
        public byte CollLod;
        public byte Flags;
        public uint CollSurfs;
        public int NumCollSurfs;
        public int Contents;
        public uint BoneInfo;
        public float Radius;
        public Bounds Bounds;
        public uint InvHighMipRadius;
        public int MemUsage;
        public uint PhysPreset;
        public uint PhysCollmap;
    };
}
