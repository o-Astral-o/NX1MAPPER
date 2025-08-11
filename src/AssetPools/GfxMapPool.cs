using Husky;
using NX1GAMER.Helpers;
using NX1GAMER.Structures;
using RedFox.Graphics3D;
using RedFox.Graphics3D.Skeletal;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using C2M;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace NX1GAMER.AssetPools
{
    public class GfxMapPool(GameInstance instance, string name, int index, int headerSize) : AssetPool(instance, name, index, headerSize)
    {
        // Load WNI Package
        public static WraithNameIndex wniPackage = new WraithNameIndex();

        public static string skyImageName;
        public static Dictionary<string, uint> loadedModels = new Dictionary<string, uint>();
        public static List<string> mapModels = new List<string>();

        public override void LoadGeneric()
        {
            var poolSize = Instance.GetPoolSize(Index);
            var poolAddress = Instance.GetPoolAddress(Index);
            var poolEndAddress = poolSize * HeaderSize + poolAddress;

            for (int i = 0; i < poolSize; i++)
            {
                var ptr = Instance.Memory.ReadUInt32(poolAddress + 4 + i * HeaderSize);

                if (ptr >= poolAddress && ptr < poolEndAddress)
                    continue;
                if (ptr == 0)
                    continue;

                var name = Instance.Memory.ReadNullTerminatedString(ptr);

                Assets.Add(new(name, Name, poolAddress + i * HeaderSize, this));
            }
        }

        public override void Export(Asset asset)
        {
            // Read GFXMap
            var gfxMap = instance.Memory.ReadStruct<GfxMap>(asset.Address);

            // Create CoDMap
            var codMap = new CoDMap(asset.Name, CoDVersion.NX1);

            // Parse Sky
            var sky = instance.Memory.ReadStruct<GfxSky>(gfxMap.Skies.AsBE());
            var skyImage = instance.Memory.ReadStruct<GfxImage>(sky.SkyImage.AsBE());
            skyImageName = instance.Memory.ReadNullTerminatedString(skyImage.Name.AsBE());

            // Parse Info
            LoadModels(instance);
            ReadLights(instance, codMap);
            ReadMapGeometry(instance, gfxMap, codMap);
            ReadMapEnts(instance, codMap);
            ReadStaticModels(instance, gfxMap, codMap);
            ReadDynamicEntities(instance, codMap);
            
            // Build Output Folder
            var outputPath = Path.Combine("exported_maps", "future_warfare", asset.Name);
            Directory.CreateDirectory(outputPath);
            var fileName = Path.Combine(outputPath, asset.Name);
            
            // Save Files
            C2M_2.ExportC2M_2(fileName + ".c2m", codMap);
            File.WriteAllText(fileName + "_mapEnts.txt", codMap.MapEnts);
            string matJson = JToken.FromObject(codMap.Materials).ToString(Formatting.Indented);
            File.WriteAllText(fileName + "_mats.json", matJson);
            string modelJson = JToken.FromObject(codMap.ModelInstances).ToString(Formatting.Indented);
            File.WriteAllText(fileName + "_models.json", modelJson);
            string lightsJson = JToken.FromObject(codMap.Lights).ToString(Formatting.Indented);
            File.WriteAllText(fileName + "_lights.json", lightsJson);
        }

        public static void LoadModels(GameInstance instance)
        {
            var poolSize = instance.GetPoolSize((int)XAssetType.XMODEL);
            var poolAddress = instance.GetPoolAddress((int)XAssetType.XMODEL) + 16;
            var poolEndAddress = poolSize * 288 + poolAddress;

            for (int i = 0; i < poolSize; i++)
            {
                var ptr = instance.Memory.ReadUInt32(poolAddress + i * 288);

                if (ptr >= poolAddress && ptr < poolEndAddress)
                    continue;
                if (ptr == 0)
                    continue;

                loadedModels.Add(instance.Memory.ReadNullTerminatedString(ptr), (uint)(poolAddress + i * 288));
            }
        }

        public static void ReadMapEnts(GameInstance instance, CoDMap codMap)
        {
            var mapEnts = instance.Memory.ReadStruct<MapEnts>(instance.GetPoolAddress((int)XAssetType.MAP_ENTS));
            codMap.MapEnts = instance.Memory.ReadNullTerminatedString(mapEnts.MapData.AsBE());

            string[] entities = codMap.MapEnts.Split(new[] { "\n}\n{" }, StringSplitOptions.None);

            Regex reg = new Regex(@"""(.*?)""\s""(.*?)""");
            foreach (string entity in entities)
            {
                // Split lines in each entity
                string[] entityProperties = entity.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string modelName = "";

                // Loop through lines, and get necessary ones
                foreach (String line in entityProperties)
                {
                    MatchCollection matches = reg.Matches(line);
                    // Check if line is a model name
                    foreach (Match m in matches)
                    {
                        // Check if line is a model name
                        if (m.Groups[1].Value == "model")
                            // Get entity name
                            modelName = m.Groups[2].Value;
                    }
                }

                // Handle BrushModels
                if (modelName.StartsWith("*") || modelName.StartsWith("?") || modelName == "")
                    continue;

                // if model name found in xmodel pool, parse & add it.
                if (loadedModels.ContainsKey(modelName) && !mapModels.Contains(modelName))
                {
                    ReadXModel(instance, loadedModels[modelName], codMap);
                    mapModels.Add(modelName);
                }
            }
        }

        public static void ReadLights(GameInstance instance, CoDMap codMap)
        {
            var comMap = instance.Memory.ReadStruct<ComMap>(instance.GetPoolAddress((int)XAssetType.COMWORLD));

            var lightcount = comMap.PrimaryLightCount.AsBE();
            var lights = comMap.PrimaryLights.AsBE();

            for (int l = 0; l < lightcount; l++)
            {
                if (l == 0)
                    continue;

                var light = instance.Memory.ReadStruct<ComPrimaryLight>(lights + l * 0x48);

                // Grab Info
                var color = new Vector3D(light.Color.X.AsBE(), light.Color.Y.AsBE(), light.Color.Z.AsBE());
                var dir = new Vector3(light.Dir.X.AsBE(), light.Dir.Y.AsBE(), light.Dir.Z.AsBE());
                var origin = new Vector3(light.Origin.X.AsBE(), light.Origin.Y.AsBE(), light.Origin.Z.AsBE());
                var radius = light.Radius.AsBE();
                var cosHalfFovOuter = light.CosHalfFovOuter.AsBE();
                var cosHalfFovInner = light.CosHalfFovInner.AsBE();

                LightType type;
                switch (light.Type)
                {
                    case 1: type = LightType.Sun; break;
                    case 2: type = LightType.Spot; break;
                    case 3: type = LightType.Point; break;
                    default:
                        Console.WriteLine(String.Format("Unkown Light Type: {0}", light.Type));
                        type = LightType.Spot;
                        break;
                }

                // Add to CoDMap
                codMap.Lights.Add(new CoDLight(type,
                    new Vector3D(origin.X, origin.Y, origin.Z),
                    new Vector3D(dir.X, dir.Y, dir.Z),
                    new Vector4D(color.X, color.Y, color.Z, color.Max()),
                    new Vector3D(0, 0, 0),
                    0,
                    cosHalfFovOuter,
                    cosHalfFovInner,
                    radius
                ));
            }
        }

        public static void ReadMapGeometry(GameInstance instance, GfxMap gfxMap, CoDMap codMap)
        {
            // Grab Info
            var vertexCount = gfxMap.VertexCount.AsBE();
            var vertices = gfxMap.Vertices.AsBE();
            
            var layerdataSize = (int)gfxMap.VertexLayerDataSize.AsBE();
            var layerData = gfxMap.VertexLayerData.AsBE();
            
            var indexCount = gfxMap.IndexCount.AsBE();
            var indices = gfxMap.Indices.AsBE();
            
            var surfaceCount = gfxMap.SurfaceCount.AsBE();
            var surfaces = gfxMap.Surfaces.AsBE();

            var layersBuffer = instance.Memory.ReadMemory(layerData, layerdataSize);

            List<int> faceIndices = new List<int>();
            for (uint i = 0; i < indexCount; i++)
            {
                faceIndices.Add(instance.Memory.ReadUInt16(indices + i * 2));
            }

            // Trackers
            Dictionary<string, ushort> mapSurfaces = new Dictionary<string, ushort>();
            Dictionary<string, uint> surfVertexIndex = new Dictionary<string, uint>();
            ushort mapSurfaceTracker = 0;
            uint surfVertexTracker;

            // Map Global Vertex Indices To Relative Indices For This CoDMesh
            Dictionary<int, int> vertexIndices = new Dictionary<int, int>();
            // Track Previous Surface To Compare Vertex Info
            GfxSurface prevSurface = new GfxSurface();

            // Create New CoDMesh
            CoDMesh codMesh = new CoDMesh("mapGeometry");

            for (int s = 0; s < surfaceCount; s++)
            {
                // Read Surface
                var surface = instance.Memory.ReadStruct<GfxSurface>(surfaces + s * Unsafe.SizeOf<GfxSurface>());

                // Grab Info
                var vertStartIndex = (int)surface.VertexIndex.AsBE();
                var vertCount = surface.VertexCount.AsBE();
                var faceStartIndex = (int)surface.FaceIndex.AsBE();
                var faceCount = surface.FaceCount.AsBE();

                // Read Material
                var material = instance.Memory.ReadStruct<NX1Material>(surface.MaterialPointer.AsBE());
                var techset = instance.Memory.ReadStruct<TechniqueSet>(material.TechniqueSet.AsBE());
                var surfMat = ReadMaterial(instance, surface.MaterialPointer.AsBE(), codMap, false);
                var surfName = surfMat.Name;

                // Loop Vertices
                for (int v = vertStartIndex; v < vertStartIndex + vertCount; v++)
                {
                    // Some Surfaces Are Identical For Some Reason, So Skip Them To Save Time
                    if (prevSurface.VertexIndex.AsBE() == surface.VertexIndex.AsBE() && prevSurface.VertexCount.AsBE() == surface.VertexCount.AsBE())
                        continue;

                    // Make Sure We Haven't Already Added This Vertex
                    if (vertexIndices.ContainsKey(v))
                        continue;

                    // Track It To Avoid Duplicates
                    vertexIndices.Add(v, codMesh.Vertices.Count);

                    var packedVertex = instance.Memory.ReadStruct<QSGfxVertexBuffer>(vertices + v * Unsafe.SizeOf<QSGfxVertexBuffer>());

                    var position = new Vector3(packedVertex.Xyz.X.AsBE(), packedVertex.Xyz.Y.AsBE(), packedVertex.Xyz.Z.AsBE());
                    var normal = UnpackFloat3(packedVertex.Normal.AsBE());
                    var uv = new Vector2(packedVertex.U.AsBE(), 1 - packedVertex.V.AsBE());

                    codMesh.Vertices.Add(new(position.X, position.Y, position.Z));
                    codMesh.Normals.Add(new(normal.X, normal.Y, normal.Z));
                    codMesh.UVs.Add(new List<Vector2D> { new Vector2D(uv.X, uv.Y) });
                    codMesh.Colors.Add(packedVertex.Color);
                }

                // Check if surface already exists
                if (mapSurfaces.ContainsKey(surfName) == false)
                {
                    // Add it with index
                    mapSurfaces.Add(surfName, mapSurfaceTracker);
                    // Pass to next index
                    mapSurfaceTracker++;
                    // Create new CoDSurface
                    codMesh.Surfaces.Add(new CoDSurface(surfName));
                    // Create a vertex tracker for current surface (surface might appear again later)
                    surfVertexTracker = 0;
                    // Add it
                    surfVertexIndex.Add(surfName, surfVertexTracker);
                }

                // If exists, get the surf index from the surfaces dictionary & load it
                var codSurf = codMesh.Surfaces[mapSurfaces[surfName]];

                codSurf.UVCount = (byte)surfMat.Indices.Count;
                codSurf.MaterialIndices = surfMat.Indices;

                byte worldVertFormat = techset.WorldVertFormat;

                int layerPadding = 1;
                switch (worldVertFormat)
                {
                    // No padding between UVs
                    case 1:
                    case 3:
                    case 6:
                    case 9:
                    case 19: layerPadding = 0; break;
                    // 4 byte padding between Vertices
                    case 2:
                    case 4:
                    case 7:
                    case 11:
                    case 24:
                    case 39: layerPadding = 4; break;
                    // 8 byte padding between Vertices
                    case 5:
                    // case 7:
                    case 15:
                    case 12:
                    case 29:
                    case 45:
                    case 76: layerPadding = 8; break;
                    // 12 byte padding between Vertices
                    case 16:
                    case 34:
                    case 30:
                    case 51: layerPadding = 12; break;
                    // 16 byte padding between Vertices
                    case 10:
                    case 17:
                    case 35:
                    case 90:
                    case 57: layerPadding = 16; break;
                    // 20 byte padding between Vertices
                    case 36:
                    case 63:
                    case 97:
                    case 58:
                    case 83: layerPadding = 20; break;
                    // 24 byte padding between Vertices
                    case 64: layerPadding = 24; break;
                    // 28 byte padding between Vertices
                    case 99:
                    case 65: layerPadding = 28; break;
                }

                if (layerPadding == 1 && worldVertFormat != 0)
                    Console.WriteLine();

                int surfLayerUvTracker = 0;
                for (ushort i = 0; i < faceCount; i++)
                {
                    // Face Indices
                    var faceIndex1 = faceIndices[i * 3 + faceStartIndex] + vertStartIndex;
                    var faceIndex2 = faceIndices[i * 3 + faceStartIndex + 1] + vertStartIndex;
                    var faceIndex3 = faceIndices[i * 3 + faceStartIndex + 2] + vertStartIndex;
                    
                    var layerIndex1 = faceIndices[(i * 3 + faceStartIndex)];
                    var layerIndex2 = faceIndices[(i * 3 + faceStartIndex + 1)];
                    var layerIndex3 = faceIndices[(i * 3 + faceStartIndex + 2)];

                    // Validate unique points, and write to OBJ
                    if (faceIndex1 != faceIndex2 && faceIndex1 != faceIndex3 && faceIndex2 != faceIndex3)
                    {
                        // new Obj Face
                        var codFace = new CoDSurface.Face();

                        int layerIndex = 0;
                        int layerCount = codSurf.UVCount - 1;
                        if (codSurf.UVCount > 1)
                        {
                            for (int f = 0; f < 3; f++)
                            {
                                switch (f)
                                {
                                    case 0: surfLayerUvTracker = layerIndex1; layerIndex = vertexIndices[faceIndex1]; break;
                                    case 1: surfLayerUvTracker = layerIndex2; layerIndex = vertexIndices[faceIndex2]; break;
                                    case 2: surfLayerUvTracker = layerIndex3; layerIndex = vertexIndices[faceIndex3]; break;
                                }
                                
                                if (codMesh.UVs[layerIndex].Count <= 1)
                                {
                                    int layerOffset = (int)(surface.VertexLayerDataOffset.AsBE() + (layerCount * 8 + layerPadding) * surfLayerUvTracker);
                                    
                                    var vertexLayers = UnpackLayerUVs(layersBuffer, layerOffset, layerCount);
                                            
                                    codMesh.UVs[layerIndex].AddRange(vertexLayers);
                                }
                            }
                        }

                        // Add points
                        codFace.vertexIndices[2] = (uint)vertexIndices[faceIndex1];
                        codFace.vertexIndices[1] = (uint)vertexIndices[faceIndex2];
                        codFace.vertexIndices[0] = (uint)vertexIndices[faceIndex3];

                        // Add to OBJ
                        codSurf.Faces.Add(codFace);
                        codMesh.FaceCount++;
                        surfVertexIndex[surfName] += 3;
                    }
                }
            }

            // Add To CoDMap
            codMap.Objects.Add(codMesh);
        }

        public static void ReadStaticModels(GameInstance instance, GfxMap gfxMap, CoDMap codMap)
        {
            var staticModelCount = gfxMap.SModelCount.AsBE();
            var staticModels = gfxMap.SModelDrawInsts.AsBE();

            for (uint m = 0; m < staticModelCount; m++)
            {
                var staticModel = instance.Memory.ReadStruct<GfxStaticModel>(staticModels + m * 0x30);

                // Grab Info
                var origin = new Vector3D(staticModel.X.AsBE(), staticModel.Y.AsBE(), staticModel.Z.AsBE());
                var matrixX = UnpackFloat3(staticModel.MatrixX.AsBE());
                var matrixY = UnpackFloat3(staticModel.MatrixY.AsBE());
                var matrixZ = UnpackFloat3(staticModel.MatrixZ.AsBE());
                var scale = staticModel.ModelScale.AsBE();
                var modelPtr = staticModel.ModelPointer.AsBE();
                
                // Read XModel
                var modelHeader = instance.Memory.ReadStruct<XModel>(modelPtr);
                var modelName = Path.GetFileNameWithoutExtension(instance.Memory.ReadNullTerminatedString(modelHeader.Name.AsBE()));
                if (!mapModels.Contains(modelName))
                {
                    ReadXModel(instance, modelPtr, codMap);
                    mapModels.Add(modelName);
                }
                
                // New Matrix
                var matrix = new Rotation.Matrix();
                // Copy X Values
                matrix.Values[0]  = matrixX.X;
                matrix.Values[1]  = matrixX.Y;
                matrix.Values[2]  = matrixX.Z;
                // Copy Y Values
                matrix.Values[4]  = matrixY.X;
                matrix.Values[5]  = matrixY.Y;
                matrix.Values[6]  = matrixY.Z;
                // Copy Z Values
                matrix.Values[8]  = matrixZ.X;
                matrix.Values[9]  = matrixZ.Y;
                matrix.Values[10] = matrixZ.Z;
                
                // Convert to Euler
                var euler = matrix.ToEuler();

                // Add model instance
                codMap.ModelInstances.Add(new CoDModelInstance(modelName,
                    origin,
                    euler,
                    scale,
                    InstanceType.Static));

                Console.WriteLine();
            }
        }

        public static void ReadDynamicEntities(GameInstance instance, CoDMap codMap)
        {
            var clipMap = instance.Memory.ReadStruct<ClipMap>(instance.GetPoolAddress((int)XAssetType.CLIPMAP_MP));

            // Grab Info
            var dynEntCount = clipMap.DynEntCount.AsBE();
            var dynEnts = clipMap.DynEntDefs.AsBE();

            for (int d = 0; d < dynEntCount; d++)
            {
                var dynEnt = instance.Memory.ReadStruct<DynEntity>(dynEnts, d);

                // Grab Info
                var model = instance.Memory.ReadStruct<XModel>(dynEnt.XModel.AsBE());
                var modelName = instance.Memory.ReadNullTerminatedString(model.Name.AsBE());
                var origin = new Vector3D(dynEnt.Position.X.AsBE(), dynEnt.Position.Y.AsBE(), dynEnt.Position.Z.AsBE());
                var quat = new Vector4D(dynEnt.Rotation.X.AsBE(), dynEnt.Rotation.Y.AsBE(), dynEnt.Rotation.Z.AsBE(), dynEnt.Rotation.W.AsBE());
                var euler = Rotation.QuatToEul(quat);

                if (!mapModels.Contains(modelName))
                {
                    ReadXModel(instance, dynEnt.XModel.AsBE(), codMap);
                    mapModels.Add(modelName);
                }

                // Add model instance
                codMap.ModelInstances.Add(new CoDModelInstance(modelName,
                    origin,
                    euler,
                    1,
                    InstanceType.DynamicEnt));
            }
        }

        public static void ReadXModel(GameInstance instance, uint address, CoDMap codMap)
        {
            var modelHeader = instance.Memory.ReadStruct<XModel>(address);
            var modelName = Path.GetFileNameWithoutExtension(instance.Memory.ReadNullTerminatedString(modelHeader.Name.AsBE()));

            // First parse bone names
            var materials = modelHeader.MaterialHandles.AsBE();

            CoDMesh codModel = new CoDMesh(modelName);

            for (int lodIndex = 0; lodIndex < modelHeader.NumLods; lodIndex++)
            {
                // Create LOD
                var codLod = new CoDMeshLOD();

                // Model LOD
                var modelLod = modelHeader.LodInfo[lodIndex];

                // Load surface buffer
                var modelSurfs = instance.Memory.ReadStruct<XModelSurfs>(modelLod.ModelSurfs.AsBE());
                var xsurfaces = modelSurfs.Surfs.AsBE();

                uint surfVertCounter = 0;
                for (int s = 0; s < modelSurfs.Numsurfs.AsBE(); s++)
                {
                    var surface = instance.Memory.ReadStruct<XSurface>(xsurfaces + s * Unsafe.SizeOf<XSurface>());

                    var material = ReadMaterial(instance, instance.Memory.ReadUInt32(materials), codMap, false);
                    materials += 4;

                    // Create new CoDSurface
                    var codSurf = new CoDSurface(material.Name);

                    CoDSurface.SurfMaterial surfMat = new CoDSurface.SurfMaterial(material.Name);
                    surfMat.Indices.Add(codMap.Materials[material.Name].MaterialIndex);

                    codSurf.UVCount = (byte)surfMat.Indices.Count;
                    codSurf.MaterialIndices = surfMat.Indices;

                    var vertexData = surface.Verts0.AsBE();
                    var vertexCount = surface.VertCount.AsBE();
                    var faceData = surface.TriIndices.AsBE();
                    var faceCount = surface.TriCount.AsBE();

                    for (int v = 0; v < vertexCount; v++)
                    {
                        var packedVertex = instance.Memory.ReadStruct<GfxPackedVertex>(vertexData + v * Unsafe.SizeOf<GfxPackedVertex>());

                        var position = new Vector3(packedVertex.Xyz.X.AsBE(), packedVertex.Xyz.Y.AsBE(), packedVertex.Xyz.Z.AsBE());
                        var normal = UnpackFloat3(packedVertex.Normal.AsBE());
                        var uv = new Vector2((float)packedVertex.UVU.AsBE(), 1 - (float)packedVertex.UVV.AsBE());

                        if (lodIndex == 0)
                        {
                            codModel.Vertices.Add(new(position.X, position.Y, position.Z));
                            codModel.Normals.Add(new(normal.X, normal.Y, normal.Z));
                            codModel.UVs.Add(new List<Vector2D> { new Vector2D(uv.X, uv.Y) });
                            codModel.Colors.Add(new GfxColor(packedVertex.ColorR, packedVertex.ColorG, packedVertex.ColorB, packedVertex.ColorA));
                        }
                        else
                        {
                            codLod.Vertices.Add(new(position.X, position.Y, position.Z));
                            codLod.Normals.Add(new(normal.X, normal.Y, normal.Z));
                            codLod.UVs.Add(new List<Vector2D> { new Vector2D(uv.X, uv.Y) });
                            codLod.Colors.Add(new GfxColor(packedVertex.ColorR, packedVertex.ColorG, packedVertex.ColorB, packedVertex.ColorA));
                        }
                    }

                    for (int f = 0; f < faceCount; f++)
                    {
                        var f0 = instance.Memory.ReadUInt16(faceData + f * 6 + 0);
                        var f1 = instance.Memory.ReadUInt16(faceData + f * 6 + 2);
                        var f2 = instance.Memory.ReadUInt16(faceData + f * 6 + 4);

                        // Create face
                        var codFace = new CoDSurface.Face();
                        codFace.vertexIndices[2] = f0 + surfVertCounter;
                        codFace.vertexIndices[1] = f1 + surfVertCounter;
                        codFace.vertexIndices[0] = f2 + surfVertCounter;

                        if (lodIndex == 0)
                            codModel.FaceCount++;
                        else
                            codLod.FaceCount++;

                        // Add it
                        codSurf.Faces.Add(codFace);
                    }
                    surfVertCounter += vertexCount;

                    if (lodIndex == 0)
                        codModel.Surfaces.Add(codSurf);
                    else
                        codLod.Surfaces.Add(codSurf);
                }

                if (lodIndex != 0)
                    codModel.LODs.Add(codLod);
            }

            codMap.Objects.Add(codModel);
        }

        public unsafe static CoDSurface.SurfMaterial ReadMaterial(GameInstance instance, uint address, CoDMap codMap, bool isSky)
        {

            // Read Info
            var material = instance.Memory.ReadStruct<NX1Material>(address);
            var materialName = Path.GetFileNameWithoutExtension(instance.Memory.ReadNullTerminatedString(material.info.name.AsBE()));
            var techset = instance.Memory.ReadStruct<TechniqueSet>(material.TechniqueSet.AsBE());
            string techSetName = instance.Memory.ReadNullTerminatedString(techset.Name.AsBE());

            // Create Surface Material
            CoDSurface.SurfMaterial surfMat = new CoDSurface.SurfMaterial(materialName.Replace("*", ""));

            // Resolve Material Names
            Dictionary<string, CoDMaterial> matList = new Dictionary<string, CoDMaterial>();
            var matNames = (materialName.Contains("*")) ? materialName.Replace("*", "").Split('_').ToList() : new List<string>() { materialName };

            // Resolve Techset Names
            List<string> techSetNames = new List<string>(matNames.Count);
            for (int i = 0; i < matNames.Count; i++)
                techSetNames.Add(techSetName);
            techSetName = Regex.Replace(techSetName, @"^(?:l_sm_)?", "");
            var matches = Regex.Matches(techSetName, @"([A-Za-z])(\d)(?:[A-Za-z]\2)+").Cast<Match>().ToList();
            if (matches.Count > 0)
                techSetNames = matches.Select((x, i) =>
                {
                    var j = matches.FindIndex(i + 1, y => int.Parse(y.Groups[2].Value) == int.Parse(x.Groups[2].Value) + 1);
                    var end = j >= 0 ? matches[j].Index : techSetName.Length;
                    return techSetName.Substring(x.Index, end - x.Index).TrimEnd('_');
                }).ToList();


            // Create CoDMaterials
            for (int i = 0; i < matNames.Count; i++)
            {
                string matName = matNames[i];

                string matTechSet = techSetNames[i].Replace(Convert.ToString(i), "0");

                if (!matList.ContainsKey(matName))
                    matList.Add(matName, new CoDMaterial(matName, 0, matTechSet));
            }

            // Parse Images
            for (int t = 0; t < material.TextureCount; t++)
            {
                var imageDef = instance.Memory.ReadStruct<MaterialTextureDef>(material.TextureTable.AsBE(), t);
                var imageHeader = instance.Memory.ReadStruct<GfxImage>(imageDef.Image.AsBE());
                var imageName = instance.Memory.ReadNullTerminatedString(imageHeader.Name.AsBE());

                // Export Image
                ImagePool.ExportImage(instance, imageDef.Image.AsBE(), Path.Combine("exported_images", "future_warfare"));

                var imageHash = imageDef.Hash.AsBE();
                var imageSemantic = $"unk_semantic_{imageHash:X}";

                var materialIndex = 0;
                if (wniPackage.assetNames.ContainsKey(imageHash))
                {
                    imageSemantic = wniPackage.assetNames[imageHash];
                    materialIndex = (Convert.ToInt32(Char.GetNumericValue(imageSemantic.Last())));
                    imageSemantic = imageSemantic.Remove(imageSemantic.Length - 1);
                }

                var codMaterial = matList[matNames[materialIndex]];

                // Check for SkyBox mat
                if (skyImageName == imageName)
                    codMap.SkyBoxInfo = string.Join(":", materialName, codMaterial.TechSet, imageName);

                // Set blending mode
                if (codMaterial.TechSet.Contains("b0") && codMaterial.TechSet.Contains("v0"))
                    codMaterial.Blending = BlendingMode.VertexComplex;
                else if (codMaterial.TechSet.Contains("b0c0") && materialIndex != 0)
                    codMaterial.Blending = BlendingMode.VertexSimple;
                else if (codMaterial.TechSet.Contains("b0c0") && materialIndex == 0)
                    codMaterial.Blending = BlendingMode.Translucent;
                else if (codMaterial.TechSet.Contains("m0c0"))
                    codMaterial.Blending = BlendingMode.Multiply;

                if (!codMaterial.Textures.ContainsKey(imageSemantic))
                    codMaterial.Textures.Add(imageSemantic, new CoDTexture(imageName, imageSemantic));
            }

            // Parse Constants
            for (int c = 0; c < material.ConstantCount; c++)
            {
                // Grab Info
                var constant = instance.Memory.ReadStruct<MaterialConstantDef>(material.ConstantTable.AsBE(), c);
                var nameHash = constant.NameHash.AsBE();
                var value = new Vector4D(constant.Literal.X.AsBE(), constant.Literal.Y.AsBE(), constant.Literal.Z.AsBE(), constant.Literal.W.AsBE());

                string constantName = new string(constant.Name).TrimEnd('\0');
                
                var materialIndex = 0;
                if (wniPackage.assetNames.ContainsKey(nameHash))
                {
                    constantName = wniPackage.assetNames[nameHash];
                    materialIndex = Convert.ToInt32(Char.GetNumericValue(constantName.Last()));
                    constantName = constantName.Remove(constantName.Length - 1);
                }
                
                var codMaterial = matList[matNames[materialIndex]];
                
                if (!codMaterial.Constants.ContainsKey(constantName))
                    codMaterial.Constants.Add(constantName, new CoDConstant(constantName, nameHash, value));
            }

            // Add Individual Materials to CoDMap
            foreach (var codMaterial in matList.Values)
                if (!codMap.Materials.ContainsKey(codMaterial.Name))
                {
                    codMaterial.MaterialIndex = (ushort)codMap.Materials.Count();
                    codMap.Materials.Add(codMaterial.Name, codMaterial);
                }

            foreach (var name in matNames)
                surfMat.Indices.Add(codMap.Materials[name].MaterialIndex);

            return surfMat;
        }
        
        public unsafe static Vector3 UnpackFloat3(uint packedInt)
        {
            // Resulting values
            var builtX = new FloatToInt { Integer = ((packedInt & 0x3FF) - 2 * (packedInt & 0x200) + 0x40400000) };
            var builtY = new FloatToInt { Integer = ((((packedInt >> 10) & 0x3FF) - 2 * ((packedInt >> 10) & 0x200) + 0x40400000)) };
            var builtZ = new FloatToInt { Integer = ((((packedInt >> 20) & 0x3FF) - 2 * ((packedInt >> 20) & 0x200) + 0x40400000)) };

            // Return decoded vector
            return new Vector3(
                (builtX.Float - 3.0f) * 8208.0312f,
                (builtY.Float - 3.0f) * 8208.0312f,
                (builtZ.Float - 3.0f) * 8208.0312f);
        }

        public static List<Vector2D> UnpackLayerUVs(byte[] layersBuffer, int offset, int layerCount)
        {
            List<Vector2D> vertexUVs = new List<Vector2D>();
            byte[] vertexLayerBuffer = new byte[layerCount * 8];
        
            Buffer.BlockCopy(layersBuffer, offset, vertexLayerBuffer, 0, layerCount * 8);
            for (int i = 0; i < layerCount; i++)
            {
                var u = BitConverter.ToSingle(vertexLayerBuffer, i * 8).AsBE();
                var v = BitConverter.ToSingle(vertexLayerBuffer, i * 8 + 4).AsBE();

                var uv = new Vector2D(u, 1 - v);
                
                vertexUVs.Add(uv);
            }
            return vertexUVs;
        }
    }
}
