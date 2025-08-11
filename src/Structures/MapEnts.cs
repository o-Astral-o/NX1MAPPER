using System.Runtime.InteropServices;

namespace NX1GAMER.Structures;

[StructLayout(LayoutKind.Explicit)]
public struct MapEnts
{
	[FieldOffset(0x10)]
	public uint MapData;
}