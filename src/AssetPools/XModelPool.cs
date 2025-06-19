using Husky;
using NX1GAMER.Helpers;
using NX1GAMER.Structures;
using RedFox.Graphics3D;
using RedFox.Graphics3D.Skeletal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER.AssetPools
{
    public class XModelPool(GameInstance instance, string name, int index, int headerSize) : AssetPool(instance, name, index, headerSize)
    {
        /// <summary>
        /// Unpacks a Vertex Normal from: Bo2
        /// </summary>
        /// <param name="packedNormal">Packed 4 byte Vertex Normal</param>
        /// <returns>Resulting Vertex Normal</returns>
        public static Vector3 MethodC(uint packedNormal)
        {
            // Resulting values
            var builtX = new FloatToInt { Integer = (uint)((packedNormal & 0x3FF) - 2 * (packedNormal & 0x200) + 0x40400000) };
            var builtY = new FloatToInt { Integer = (uint)((((packedNormal >> 10) & 0x3FF) - 2 * ((packedNormal >> 10) & 0x200) + 0x40400000)) };
            var builtZ = new FloatToInt { Integer = (uint)((((packedNormal >> 20) & 0x3FF) - 2 * ((packedNormal >> 20) & 0x200) + 0x40400000)) };

            // Return decoded vector
            return new Vector3(
                (builtX.Float - 3.0f) * 8208.0312f,
                (builtY.Float - 3.0f) * 8208.0312f,
                (builtZ.Float - 3.0f) * 8208.0312f);
        }

        public override void LoadGeneric()
        {
            Console.WriteLine(Instance.Config.GetAddress("PoolSizesAddress").ToString("X"));

            var poolSize = Instance.GetPoolSize(Index);
            var poolAddress = Instance.GetPoolAddress(Index) + 16;
            var poolEndAddress = poolSize * HeaderSize + poolAddress;

            for (int i = 0; i < poolSize; i++)
            {
                var ptr = Instance.Memory.ReadUInt32(poolAddress + i * HeaderSize);

                if (ptr >= poolAddress && ptr < poolEndAddress)
                    continue;
                if (ptr == 0)
                    continue;

                Assets.Add(new(Instance.Memory.ReadNullTerminatedString(ptr), Name, poolAddress + i * HeaderSize, this));
            }
        }

        public override void Export(Asset asset)
        {
            var folder = Path.Combine("exported_files", "xmodel", asset.Name);
            var skeleton = new Skeleton(asset.Name + "_skeleton");
            var modelHeader = Instance.Memory.ReadStruct<XModel>(asset.Address);

            // First parse bone names
            var boneCount = modelHeader.NumBones;
            var boneNamesAddress = modelHeader.BoneNames.AsBE();
            var boneMatricesAddress = modelHeader.BaseMat.AsBE();
            var boneParentsAddress = modelHeader.ParentList.AsBE();
            var materials = modelHeader.MaterialHandles.AsBE();

            // First preparse bones with their names
            for (int b = 0; b < boneCount; b++)
            {
                skeleton.Bones.Add(new(Instance.GetString(Instance.Memory.ReadUInt16(boneNamesAddress + b * 2))));
            }

            // Read Translations/Rotatins
            for (int i = 0, k = 0; i < modelHeader.NumBones; i++)
            {
                var matrix = Instance.Memory.ReadStruct<DObjAnimMat>(boneMatricesAddress + i * 32);

                skeleton.Bones[i].BaseWorldTranslation = new Vector3(
                    matrix.Trans.X.AsBE() * 2.54f,
                    matrix.Trans.Y.AsBE() * 2.54f,
                    matrix.Trans.Z.AsBE() * 2.54f);
                skeleton.Bones[i].BaseWorldRotation = new Quaternion(
                    matrix.Quat.X.AsBE(),
                    matrix.Quat.Y.AsBE(),
                    matrix.Quat.Z.AsBE(),
                    matrix.Quat.W.AsBE());

                if (i >= modelHeader.NumRootBones)
                {
                    var parentIndex = i - Instance.Memory.ReadByte(boneParentsAddress + i - modelHeader.NumRootBones);
                    skeleton.Bones[i].Parent = skeleton.Bones[parentIndex];
                }
            }

            // Make some juicy transforms
            skeleton.GenerateLocalTransforms();

            Directory.CreateDirectory(folder);

            for (int l = 0; l < modelHeader.NumLods; l++)
            {
                var modelLod = modelHeader.LodInfo[l];
                var newModel = new Model($"{asset.Name}_lod{l}") { Skeleton = skeleton };
                var modelSurfs = Instance.Memory.ReadStruct<XModelSurfs>(modelLod.ModelSurfs.AsBE());
                var xsurfaces = modelSurfs.Surfs.AsBE();

                for (int s = 0; s < modelSurfs.Numsurfs.AsBE(); s++)
                {
                    var surface = Instance.Memory.ReadStruct<XSurface>(xsurfaces + s * Unsafe.SizeOf<XSurface>());
                    var materialName = Instance.Memory.ReadNullTerminatedString(Instance.Memory.ReadUInt32(Instance.Memory.ReadUInt32(materials)));
                    var vertexData = surface.Verts0.AsBE();
                    var vertexCount = surface.VertCount.AsBE();
                    var faceData = surface.TriIndices.AsBE();
                    var faceCount = surface.TriCount.AsBE();
                    var xrigidData = surface.VertList.AsBE();
                    var newMesh = new Mesh(vertexCount, faceCount, 1, 4, 0, MeshAttributeFlags.All);

                    var materialHeader = Instance.Memory.ReadStruct<NX1Material>(Instance.Memory.ReadUInt32(materials));
                    var material = new Material(Path.GetFileNameWithoutExtension(materialName));

                    for (int t = 0; t < materialHeader.TextureCount; t++)
                    {
                        var imageDef = Instance.Memory.ReadStruct<MaterialTextureDef>(materialHeader.TextureTable.AsBE(), t);
                        var imageHash = imageDef.Hash.AsBE();
                        var imagePath = ImagePool.ExportImage(Instance, imageDef.Image.AsBE(), Path.Combine(folder, "_images"));

                        switch(imageHash)
                        {
                            case 0xA0AB1041:
                                material.Textures["DiffuseMap"] = new(imagePath.Replace(folder, "").Replace("\\_images\\", "_images\\"), "DiffuseMap");
                                break;
                            case 0x59D30D0F:
                                material.Textures["NormalMap"] = new(imagePath.Replace(folder, "").Replace("\\_images\\", "_images\\"), "NormalMap");
                                break;
                            case 0x34ECCCB3:
                                material.Textures["SpecularMap"] = new(imagePath.Replace(folder, "").Replace("\\_images\\", "_images\\"), "SpecularMap");
                                break;
                        }
                    }

                    for (int v = 0; v < vertexCount; v++)
                    {
                        var packedVertex = Instance.Memory.ReadStruct<GfxPackedVertex>(vertexData + v * Unsafe.SizeOf<GfxPackedVertex>());
                        var normal = MethodC(packedVertex.Normal.AsBE());

                        newMesh.Positions.Add(new(packedVertex.Xyz.X.AsBE() * 2.54f, packedVertex.Xyz.Y.AsBE() * 2.54f, packedVertex.Xyz.Z.AsBE() * 2.54f));
                        newMesh.Normals.Add(normal);
                        newMesh.UVLayers.Add(new((float)packedVertex.UVU.AsBE(), (float)packedVertex.UVV.AsBE()));
                    }

                    for (int f = 0; f < faceCount; f++)
                    {
                        var f0 = Instance.Memory.ReadUInt16(faceData + f * 6 + 0);
                        var f1 = Instance.Memory.ReadUInt16(faceData + f * 6 + 2);
                        var f2 = Instance.Memory.ReadUInt16(faceData + f * 6 + 4);

                        newMesh.Faces.Add((f0, f2, f1));
                    }

                    var vertexIndex = 0;
                    var blendOffset = 0;

                    for (int v = 0; v < surface.VertListCount.AsBE(); v++)
                    {
                        var rigid = Instance.Memory.ReadStruct<XRigidVertList>(xrigidData, v);
                        var boneIndex = rigid.BoneOffset.AsBE() / 64;
                        var vertCount = rigid.VertCount.AsBE();

                        for (int w = 0; w < vertCount; w++)
                        {
                            newMesh.Influences.Add((boneIndex, 1.0f), vertexIndex++, 0);
                        }
                    }

                    var blend = surface.VertBlend.AsBE();
                    var count0 = surface.VertCount0.AsBE();
                    var count1 = surface.VertCount1.AsBE();
                    var count2 = surface.VertCount2.AsBE();
                    var count3 = surface.VertCount3.AsBE();

                    // 0
                    for (int w = 0; w < count0; w++)
                    {
                        var index0 = Instance.Memory.ReadUInt16(blend + blendOffset + 00) / 64;
                        newMesh.Influences.Add((index0, 1.0f), vertexIndex, 0);
                        blendOffset += 2;
                        vertexIndex++;
                    }
                    // 1
                    for (int w = 0; w < count1; w++)
                    {
                        var index0 = Instance.Memory.ReadUInt16(blend + blendOffset + 00) / 64;
                        var index1 = Instance.Memory.ReadUInt16(blend + blendOffset + 02) / 64;
                        var weight0 = Instance.Memory.ReadUInt16(blend + blendOffset + 04) / 65536.0f;
                        newMesh.Influences.Add((index0, 1.0f - weight0), vertexIndex);
                        newMesh.Influences.Add((index1, weight0), vertexIndex);
                        blendOffset += 6;
                        vertexIndex++;
                    }
                    // 2
                    for (int w = 0; w < count2; w++)
                    {
                        var index0 = Instance.Memory.ReadUInt16(blend + blendOffset + 00) / 64;
                        var index1 = Instance.Memory.ReadUInt16(blend + blendOffset + 02) / 64;
                        var weight0 = Instance.Memory.ReadUInt16(blend + blendOffset + 04) / 65536.0f;
                        var index2 = Instance.Memory.ReadUInt16(blend + blendOffset + 06) / 64;
                        var weight1 = Instance.Memory.ReadUInt16(blend + blendOffset + 08) / 65536.0f;
                        newMesh.Influences.Add((index0, 1.0f - (weight0 + weight1)), vertexIndex);
                        newMesh.Influences.Add((index1, weight0), vertexIndex);
                        newMesh.Influences.Add((index2, weight1), vertexIndex);
                        blendOffset += 10;
                        vertexIndex++;
                    }
                    // 3
                    for (int w = 0; w < count3; w++)
                    {
                        var index0 = Instance.Memory.ReadUInt16(blend + blendOffset + 00) / 64;
                        var index1 = Instance.Memory.ReadUInt16(blend + blendOffset + 02) / 64;
                        var index2 = Instance.Memory.ReadUInt16(blend + blendOffset + 06) / 64;
                        var index3 = Instance.Memory.ReadUInt16(blend + blendOffset + 10) / 64;
                        var weight0 = Instance.Memory.ReadUInt16(blend + blendOffset + 04) / 65536.0f;
                        var weight1 = Instance.Memory.ReadUInt16(blend + blendOffset + 08) / 65536.0f;
                        var weight2 = Instance.Memory.ReadUInt16(blend + blendOffset + 12) / 65536.0f;
                        newMesh.Influences.Add((index0, 1.0f - (weight0 + weight1 + weight2)), vertexIndex);
                        newMesh.Influences.Add((index1, weight0), vertexIndex);
                        newMesh.Influences.Add((index2, weight1), vertexIndex);
                        newMesh.Influences.Add((index3, weight2), vertexIndex);
                        blendOffset += 14;
                        vertexIndex++;
                    }

                    newMesh.Materials.Add(material);
                    newModel.Materials.Add(material);
                    newModel.Meshes.Add(newMesh);

                    materials += 4;
                }

                Instance.TranslatorFactory.Save(Path.Combine(folder, newModel.Name + ".semodel"), newModel);
            }
        }
    }
}
