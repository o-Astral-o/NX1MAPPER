using System.Runtime.InteropServices;

namespace NX1GAMER.Structures;

[StructLayout(LayoutKind.Explicit)]
public struct ComMap
{
	[FieldOffset(0x0)]
	public uint FileName;
	[FieldOffset(0x4)]
	public uint IsInUse;
	[FieldOffset(0x8)]
	public uint PrimaryLightCount;
	[FieldOffset(0xC)]
	public uint PrimaryLights;
}

[StructLayout(LayoutKind.Explicit, Size = 0x48)]
public struct ComPrimaryLight
{
	[FieldOffset(0x0)]
	public byte Type;
	[FieldOffset(0x1)]
	public byte CanUseShadowMap;
	[FieldOffset(0x2)]
	public byte Exponent;
	[FieldOffset(0x3)]
	public byte LutID;
	[FieldOffset(0x4)]
	public byte SpotRotation;
	
	[FieldOffset(0x8)]
	public System.Numerics.Vector3 Color;
	[FieldOffset(0x14)]
	public System.Numerics.Vector3 Dir;
	[FieldOffset(0x20)]
	public System.Numerics.Vector3 Origin;
	[FieldOffset(0x2C)]
	public float Radius;
	[FieldOffset(0x30)]
	public float CosHalfFovOuter;
	[FieldOffset(0x34)]
	public float CosHalfFovInner;
	[FieldOffset(0x38)]
	public float CosHalfFovExpanded;
	[FieldOffset(0x3C)]
	public float RotationLimit;
	[FieldOffset(0x40)]
	public uint TranslationLimit;
	[FieldOffset(0x44)]
	public uint Name;
	
};