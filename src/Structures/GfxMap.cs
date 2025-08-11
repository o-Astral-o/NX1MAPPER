using System.Runtime.InteropServices;
using C2M;

namespace NX1GAMER.Structures;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct GfxMap
{
	[FieldOffset(0x0)]
	public uint Name;
	[FieldOffset(0x4)]
	public uint BaseName;
	[FieldOffset(0x8)]
	public uint PlaneCount;
	[FieldOffset(0xC)]
	public uint NodeCount;
	[FieldOffset(0x10)]
	public uint SurfaceCount;
	[FieldOffset(0x14)]
	public uint SkyCount;
	[FieldOffset(0x18)]
	public uint Skies;
	
	[FieldOffset(0x20)]
	public uint LastSunPrimaryLightIndex;
	[FieldOffset(0x24)]
	public uint PrimaryLightCount;
	[FieldOffset(0x28)]
	public uint SortKeyLitDecal;
	[FieldOffset(0x2C)]
	public uint SortKeyEffectDecal;
	[FieldOffset(0x30)]
	public uint SortKeyEffectAuto;
	[FieldOffset(0x34)]
	public uint SortKeyDistortion;
        
	[FieldOffset(0x38)]
	public uint CellCount;
	[FieldOffset(0x3C)]
	public uint Planes;
	[FieldOffset(0x40)]
	public uint Nodes;
	[FieldOffset(0x44)]
	public uint SceneEntCellBits;
        
	[FieldOffset(0x48)]
	public uint AabbTreeCounts;
	[FieldOffset(0x4C)]
	public uint AabbTrees;
	[FieldOffset(0x50)]
	public uint Cells;
        
	// GfxWorldDraw
	[FieldOffset(0x54)]
	public uint ReflectionProbeCount;
	[FieldOffset(0x58)]
	public uint ReflectionProbes;
	[FieldOffset(0x5C)]
	public uint ReflectionProbeOrigins;
	[FieldOffset(0x60)]
	public uint ReflectionProbeTextures;
	[FieldOffset(0x64)]
	public uint LightmapCount;
	[FieldOffset(0x68)]
	public uint Lightmaps;
	[FieldOffset(0x6C)]
	public uint LightmapPrimaryTextures;
	[FieldOffset(0x70)]
	public uint LightmapSecondaryTextures;

	[FieldOffset(0x8C)]
	public uint VertexCount;
	[FieldOffset(0x90)]
	public uint Vertices;
	
	[FieldOffset(0xB4)]
	public uint VertexLayerDataSize;
	[FieldOffset(0xB8)]
	public uint VertexLayerData;
	
	[FieldOffset(0xDC)]
	public uint VertexLayerDataSize0;
	[FieldOffset(0xE0)]
	public uint VertexLayerData0;

	[FieldOffset(0xFC)]
	public uint Indices;
	[FieldOffset(0x100)]
	public uint IndexCount;
	
	// GfxWorldDpvsStatic
	[FieldOffset(0x270)]
	public uint SModelCount;
	[FieldOffset(0x274)]
	public uint StaticSurfaceCountNoDecal;
	[FieldOffset(0x278)]
	public uint LitOpaqueSurfsBegin;
	[FieldOffset(0x27C)]
	public uint LitOpaqueSurfsEnd;
	[FieldOffset(0x280)]
	public uint LitTransSurfsBegin;
	[FieldOffset(0x284)]
	public uint LitTransSurfsEnd;
	[FieldOffset(0x288)]
	public uint ShadowCasterSurfsBegin;
	[FieldOffset(0x28C)]
	public uint ShadowCasterSurfsEnd;
	[FieldOffset(0x290)]
	public uint EmissiveSurfsBegin;
	[FieldOffset(0x294)]
	public uint EmissiveSurfsEnd;
	[FieldOffset(0x298)]
	public uint SModelVisDataCount;
	[FieldOffset(0x29C)]
	public uint SurfaceVisDataCount;
        
	[FieldOffset(0x2A0)]
	public fixed uint SModelVisDataPtr[3];
	[FieldOffset(0x2AC)]
	public fixed uint SurfaceVisDataPtr[3];
        
	[FieldOffset(0x2B8)]
	public uint SortedSurfIndex;
	[FieldOffset(0x2BC)]
	public uint SModelInsts;
	[FieldOffset(0x2C0)]
	public uint Surfaces;
	[FieldOffset(0x2C4)]
	public uint SurfacesBounds;
	[FieldOffset(0x2C8)]
	public uint SModelDrawInsts;
	[FieldOffset(0x2CC)]
	public uint SurfaceMaterials;
	[FieldOffset(0x2D0)]
	public uint SurfaceCastsSunShadow;
	[FieldOffset(0x2D4)]
	public uint UsageCount;
}

public struct GfxSky
{
	public int SkySurfCount { get; set; }
	public uint SkyStartSurfs { get; set; }
	public uint SkyImage { get; set; }
	public byte SkySamplerState { get; set; }
};

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct GfxSurface
{
	public uint VertexLayerDataOffset;
	public uint VertexIndex;
	public ushort VertexCount;
	public ushort FaceCount;
	public uint FaceIndex;
	public uint MaterialPointer;
	public uint Padding;
};

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0x30)]
public struct GfxStaticModel
{
	public float X { get; set; }
	public float Y { get; set; }
	public float Z { get; set; }
	
	public uint MatrixX { get; set; }
	public uint MatrixY { get; set; }
	public uint MatrixZ { get; set; }
	
	public float ModelScale { get; set; }
	
	public uint ModelPointer { get; set; }
	public ushort CullDist { get; set; }
	public ushort LightingHandle { get; set; }
	public byte ReflectionProbeIndex { get; set; }
	public byte PrimaryLightIndex { get; set; }
	public byte Flags { get; set; }
	public byte FirstMtlSkinIndex { get; set; }
	public GfxColor GroundLighting { get; set; }
	public uint MpCamoPaletteData { get; set; }
}
