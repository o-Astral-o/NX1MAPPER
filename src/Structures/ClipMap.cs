using System.Numerics;
using System.Runtime.InteropServices;

namespace NX1GAMER.Structures;

[StructLayout(LayoutKind.Explicit)]
public struct ClipMap
{
	[FieldOffset(0x0)]
	public uint Name;
	[FieldOffset(0x4)]
	public uint IsInUse;
	
	[FieldOffset(0xA0)]
	public ushort DynEntCount;
	[FieldOffset(0xB0)]
	public uint DynEntDefs;
}

[StructLayout(LayoutKind.Explicit, Size = 0x60)]
public struct DynEntity
{
	[FieldOffset(0x0)]
	public uint Type;
	[FieldOffset(0x4)]
	public Vector4 Rotation;
	[FieldOffset(0x14)]
	public Vector3 Position;
	[FieldOffset(0x20)]
	public uint XModel;
	
	[FieldOffset(0x2C)]
	public uint PhysicsPreset;

	[FieldOffset(0x5C)]
	public uint Contents;
}