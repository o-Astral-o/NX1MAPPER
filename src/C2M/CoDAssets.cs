namespace C2M;

public enum SettingDataType : byte
{
    Color,
    UInt,
    UInt1,
    UInt2,
    UInt3,
    UInt4,
    Float,
    Vector2,
    Vector3,
    Vector4,
    Boolean,
    Unknown,
}

public enum CoDVersion : byte
{
    NX1 = 6
}

public enum BlendingMode : byte
{
    VertexSimple,
    VertexComplex,
    Multiply,
    Add,
    Translucent
}

public enum InstanceType : byte
{
    Static,
    DynamicEnt
}

public class CoDMap
{
    public string Name { get; set; }
    public CoDVersion Version { get; set; }
    
    public List<CoDMesh> Objects = new();
    public List<CoDLight> Lights = new();
    public Dictionary<string, CoDMaterial> Materials = new();
    public List<CoDModelInstance> ModelInstances = new();

    public string SkyBoxInfo = "";

    public string MapEnts { get; set; }

    public CoDMap(string name, CoDVersion version)
    {
        Name = name;
        Version = version;
    }
}

public class CoDMesh
{
    public string Name { get; set; }

    public ushort FaceCount { get; set; }

    public List<CoDSurface> Surfaces = new();
    public List<CoDMeshLOD> LODs = new();

    public float LODDistance { get; set; }
    
    public List<Vector3D> Vertices = new();
    public List<Vector3D> Normals = new();
    public List<List<Vector2D>> UVs = new();
    public List<GfxColor> Colors = new();

    public CoDMesh(string name)
    {
        Name = name;
    }

    public void WriteBin(BinaryWriter writer)
    {
        // Write header
        writer.WriteString(Name);
        writer.Write((uint)Vertices.Count);
        writer.Write((uint)Surfaces.Count);
        writer.Write((uint)FaceCount);
        writer.Write((uint)LODs.Count);
        writer.Write((float)LODDistance);
        // Write vertices
        foreach (var vertex in Vertices)
        {
            writer.Write((float)vertex.X);
            writer.Write((float)vertex.Y);
            writer.Write((float)vertex.Z);
        }
        // Write normals
        foreach (var normal in Normals)
        {
            writer.Write((float)normal.X);
            writer.Write((float)normal.Y);
            writer.Write((float)normal.Z);
        }
        // Write Layer UVs
        foreach (var uvSets in UVs)
        {
            // Write count of layers
            writer.Write((uint)uvSets.Count);
            foreach (var uv in uvSets)
            {
                writer.Write((float)uv.X);
                writer.Write((float)uv.Y);
            }
        }
        // Write Colors
        foreach (var color in Colors)
        {
            writer.Write((byte)color.R);
            writer.Write((byte)color.G);
            writer.Write((byte)color.B);
            writer.Write((byte)color.A);
        }
        // Write Surfaces
        foreach (var surface in Surfaces)
        {
            // Write surface name
            writer.WriteString(surface.Name);
            // Write material count for surface
            writer.Write((byte)surface.UVCount);
            writer.Write((byte)surface.MaterialIndices.Count);
            foreach (var materialIndex in surface.MaterialIndices)
                writer.Write((ushort)materialIndex);
            // Write face count
            writer.Write((uint)surface.Faces.Count);
            foreach (var face in surface.Faces)
                // Write face index
                foreach (var index in face.vertexIndices)
                    writer.Write((uint)index);

        }
        //Write LODs
        foreach (var LOD in LODs)
        {
            LOD.WriteBin(writer);
        }
    }
}

public class CoDMeshLOD
{
    public ushort FaceCount { get; set; }

    public float LODDistance { get; set; }

    /// <summary>
    /// Surface List
    /// </summary>
    public List<CoDSurface> Surfaces = new();

    /// <summary>
    /// Vertex Positions
    /// </summary>
    public List<Vector3D> Vertices = new();

    /// <summary>
    /// Vertex Normals
    /// </summary>
    public List<Vector3D> Normals = new();

    /// <summary>
    /// Vertex UV/Texture Coordinates
    /// </summary>
    public List<List<Vector2D>> UVs = new();

    /// <summary>
    /// Vertex Colors
    /// </summary>
    public List<GfxColor> Colors = new();

    public void WriteBin(BinaryWriter writer)
    {
        // Write header
        writer.Write((uint)Vertices.Count);
        writer.Write((uint)Surfaces.Count);
        writer.Write((uint)FaceCount);
        writer.Write((float)LODDistance);
        // Write vertices
        foreach (var vertex in Vertices)
        {
            writer.Write((float)vertex.X);
            writer.Write((float)vertex.Y);
            writer.Write((float)vertex.Z);
        }
        // Write normals
        foreach (var normal in Normals)
        {
            writer.Write((float)normal.X);
            writer.Write((float)normal.Y);
            writer.Write((float)normal.Z);
        }
        // Write Layer UVs
        foreach (var uvSets in UVs)
        {
            // Write count of layers
            writer.Write((uint)uvSets.Count);
            foreach (var uv in uvSets)
            {
                writer.Write((float)uv.X);
                writer.Write((float)uv.Y);
            }
        }
        // Write Colors
        foreach (var color in Colors)
        {
            writer.Write((byte)color.R);
            writer.Write((byte)color.G);
            writer.Write((byte)color.B);
            writer.Write((byte)color.A);
        }
        // Write Surfaces
        foreach (var surface in Surfaces)
        {
            // Write surface name
            writer.WriteString(surface.Name);
            // Write material count for surface
            writer.Write((byte)surface.UVCount);
            writer.Write((byte)surface.MaterialIndices.Count);
            foreach (var materialIndex in surface.MaterialIndices)
                writer.Write((ushort)materialIndex);
            // Write face count
            writer.Write((uint)surface.Faces.Count);
            foreach (var face in surface.Faces)
                // Write face index
                foreach (var index in face.vertexIndices)
                    writer.Write((uint)index);

        }
    }
}

public class CoDSurface
{
    public List<ushort> MaterialIndices { get; set; }
    public string Name { get; set; }
    
    public List<Face> Faces = new();

    public byte UVCount { get; set; }

    public class SurfMaterial
    {
        public string Name { get; set; }
        public List<ushort> Indices { get; set; }

        public SurfMaterial(string name)
        {
            Name = name;
            Indices = new List<ushort>();
        }
    }

    public class Face
    {
        public uint[] vertexIndices = new uint[3];
    }

    public CoDSurface(string name)
    {
        Name = name;
        MaterialIndices = new List<ushort>();
    }
}

public class CoDConstant
{
    public string Name { get; set; }
    public uint Hash { get; set; }

    public SettingDataType DataType { get; set; }

    public Vector4D Value { get; set; }

    public CoDConstant(string name, uint hash, Vector4D value)
    {
        Name = name;
        Hash = hash;
        DataType = SettingDataType.Vector4;
        Value = value;
    }
}

public class CoDMaterial
{
    public ushort MaterialIndex { get; set; }
    public string Name { get; set; }
    public ushort SortKey { get; set; }
    public string TechSet { get; set; }
    public string SurfType { get; set; }

    public BlendingMode Blending = BlendingMode.VertexSimple;

    /// <summary>
    /// Material textures
    /// </summary>
    public Dictionary<string, CoDTexture> Textures = new();

    public Dictionary<string, CoDConstant> Constants = new();

    public CoDMaterial(string name, ushort sortkey, string techset)
    {
        Name = name;
        SortKey = sortkey;
        TechSet = techset;
        SurfType = "SURF_TYPE_DEFAULT";
    }
}

public class CoDTexture
{
    public string Name { get; set; }
    public string Type { get; set; }

    public CoDTexture(string name, string type)
    {
        Name = name;
        Type = type;
    }
}

public class CoDModelInstance
{
    public string Name { get; set; }
    public Vector3D Position { get; set; }
    public Vector3D RotationDegrees { get; set; }

    public Vector4D RotationQuat { get; set; }
    public Vector3D ModelScale { get; set; }

    public byte RotationMode { get; set; }
    
    public InstanceType Type { get; set; }

    // degrees rotation, float scale
    public CoDModelInstance (string name, Vector3D position, Vector3D rotation, float modelscale, InstanceType type)
    {
        Name = name;
        Position = position;
        RotationDegrees = rotation;
        RotationQuat = new Vector4D();
        ModelScale = new Vector3D(modelscale, modelscale, modelscale);
        RotationMode = 1;
        Type = type;
    }
}

public enum LightType : byte
{
    Sun,
    Spot,
    Point,
    Box,
}

public class CoDLight
{
    public LightType Type { get; set; }
    public Vector3D Origin { get; set; }
    public Vector3D Direction { get; set; }
    public Vector4D Color { get; set; }
    public Vector3D Angles { get; set; }
    public float Radius { get; set; }
    public float CosHalfFovOuter { get; set; }
    public float CosHalfFovInner { get; set; }
    public float DAttenuation { get; set; }
    public CoDLight(LightType type, Vector3D origin, Vector3D direction, Vector4D color, Vector3D angles, float radius, float coshalffovouter, float coshalffovinner, float dattenuation)
    {
        Type = type;
        Origin = origin;
        Direction = direction;
        Color = color;
        Angles = angles;
        Radius = radius;
        CosHalfFovOuter = coshalffovouter;
        CosHalfFovInner = coshalffovinner;
        DAttenuation = dattenuation;
    }
}

public struct GfxColor
{
    public byte A { get; set; }
    public byte B { get; set; }
    public byte G { get; set; }
    public byte R { get; set; }

    public GfxColor(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

}