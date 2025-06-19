using Husky;
using NX1GAMER.Helpers;
using NX1GAMER.Structures;
using RedFox.Graphics3D;
using RedFox.Graphics3D.Skeletal;
using RedFox.IO;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace NX1GAMER.AssetPools
{
    public class NX1XAnimPool(GameInstance instance, string name, int index, int headerSize) : AssetPool(instance, name, index, headerSize)
    {
        public override void LoadGeneric()
        {
            var poolSize = Instance.GetPoolSize(Index);
            var poolAddress = Instance.GetPoolAddress(Index) + 12;
            var poolEndAddress = poolSize * HeaderSize + poolAddress;

            for (int i = 0; i < poolSize; i++)
            {
                var ptr = Instance.Memory.ReadUInt32(poolAddress + i * HeaderSize);

                if (ptr >= poolAddress && ptr < poolEndAddress)
                    continue;
                if (ptr == 0)
                    continue;

                var namePtr = Instance.Memory.ReadUInt32(poolAddress + i * HeaderSize);

                Assets.Add(new(Instance.Memory.ReadNullTerminatedString(namePtr), Name, poolAddress + i * HeaderSize, this));
            }
        }

        public override void Export(Asset asset)
        {
            var folder = Path.Combine("exported_files", "xanim");
            var xanimHeader = Instance.Memory.ReadStruct<XAnimParts>(asset.Address);
            var newAnim = new SkeletonAnimation(asset.Name)
            {
                TransformType = TransformType.Absolute,
                Framerate = 30,
            };
            uint xanimNames = xanimHeader.Names;
            uint xanimNotify = xanimHeader.Notify;

            uint dataShort = xanimHeader.DataShort;
            uint randomDataShort = xanimHeader.RandomDataShort;
            uint dataByte = xanimHeader.DataByte;

            //Console.WriteLine(xanimHeader.Indices);

            //Console.WriteLine($"PART_TYPE_NO_QUAT = {xanimHeader.BoneCounts[0]}");
            //Console.WriteLine($"PART_TYPE_SIMPLE_QUAT = {xanimHeader.BoneCounts[1]}");
            //Console.WriteLine($"PART_TYPE_NORMAL_QUAT = {xanimHeader.BoneCounts[2]}");
            //Console.WriteLine($"PART_TYPE_PRECISION_QUAT = {xanimHeader.BoneCounts[3]}");
            //Console.WriteLine($"PART_TYPE_SIMPLE_QUAT_NO_SIZE = {xanimHeader.BoneCounts[4]}");
            //Console.WriteLine($"PART_TYPE_NORMAL_QUAT_NO_SIZE = {xanimHeader.BoneCounts[5]}");
            //Console.WriteLine($"PART_TYPE_PRECISION_QUAT_NO_SIZE = {xanimHeader.BoneCounts[6]}");
            //Console.WriteLine($"PART_TYPE_SMALL_TRANS = {xanimHeader.BoneCounts[7]}");
            //Console.WriteLine($"PART_TYPE_TRANS = {xanimHeader.BoneCounts[8]}");
            //Console.WriteLine($"PART_TYPE_TRANS_NO_SIZE = {xanimHeader.BoneCounts[9]}");
            //Console.WriteLine($"PART_TYPE_NO_TRANS = {xanimHeader.BoneCounts[10]}");
            //Console.WriteLine($"PART_TYPE_ALL = {xanimHeader.BoneCounts[11]}");
            //Console.WriteLine($"Numframes = {xanimHeader.Numframes}");
            //Console.WriteLine($"IndexCount = {xanimHeader.IndexCount}");
            //Console.WriteLine($"Indices = {xanimHeader.Indices}");


            using var rdbReader = new BinaryReader(new MemoryStream(Instance.Memory.ReadMemory(xanimHeader.RandomDataByte, xanimHeader.RandomDataByteCount)));
            using var rdsReader = new BinaryReader(new MemoryStream(Instance.Memory.ReadMemory(xanimHeader.RandomDataShort, xanimHeader.RandomDataShortCount * 2)));
            using var rdiReader = new BinaryReader(new MemoryStream(Instance.Memory.ReadMemory(xanimHeader.RandomDataInt, xanimHeader.RandomDataIntCount * 4)));
            using var dbReader = new BinaryReader(new MemoryStream(Instance.Memory.ReadMemory(xanimHeader.DataByte, xanimHeader.DataByteCount)));
            using var dsReader = new BinaryReader(new MemoryStream(Instance.Memory.ReadMemory(xanimHeader.DataShort, xanimHeader.DataShortCount * 2)));
            using var diReader = new BinaryReader(new MemoryStream(Instance.Memory.ReadMemory(xanimHeader.DataInt, xanimHeader.DataIntCount * 4)));
            using var iReader = new BinaryReader(new MemoryStream(Instance.Memory.ReadMemory(xanimHeader.Indices, xanimHeader.Numframes >= 0x100 ? xanimHeader.IndexCount * 2 : xanimHeader.IndexCount)));

            //File.WriteAllBytes("rdb.dat", ((MemoryStream)rdbReader.BaseStream).ToArray());
            //File.WriteAllBytes("rds.dat", ((MemoryStream)rdsReader.BaseStream).ToArray());
            //File.WriteAllBytes("rdi.dat", ((MemoryStream)rdiReader.BaseStream).ToArray());
            //File.WriteAllBytes("db.dat", ((MemoryStream)dbReader.BaseStream).ToArray());
            //File.WriteAllBytes("ds.dat", ((MemoryStream)dsReader.BaseStream).ToArray());
            //File.WriteAllBytes("di.dat", ((MemoryStream)diReader.BaseStream).ToArray());
            //File.WriteAllBytes("i.dat", ((MemoryStream)iReader.BaseStream).ToArray());

            //Console.WriteLine(xanimHeader.BoneCounts[11]);

            //Console.WriteLine(xanimHeader.BoneCounts[4]);
            //Console.WriteLine(xanimHeader.BoneCounts[5]);
            //Console.WriteLine(xanimHeader.BoneCounts[6]);

            var targets = new SkeletonAnimationTarget[xanimHeader.BoneCounts[11]];

            for (int i = 0; i < xanimHeader.BoneCounts[11]; i++)
            {
                newAnim.Targets.Add(new SkeletonAnimationTarget(Instance.GetString(Instance.Memory.ReadUInt16(xanimNames + i * 2))));

                if (newAnim.Targets[i].BoneName == "tag_weapon_right")
                    newAnim.Targets[i].ChildTransformType = TransformType.Relative;
            }

            for (int i = 0; i < xanimHeader.NotifyCount; i++)
            {
                var notify = Instance.Memory.ReadStruct<NX1XAnimNotifyInfo>(xanimNotify, i);

                newAnim.CreateAction(Instance.GetString(notify.Name)).KeyFrames.Add(new AnimationKeyFrame<float, Action<Graphics3DScene>?>((int)(notify.Time * xanimHeader.Numframes), null));
            }

            // PART_TYPE_NO_QUAT
            int animPartIndex = 0;
            int size = xanimHeader.BoneCounts[0];

            //Console.WriteLine($"PART_TYPE_NO_QUAT = {xanimHeader.BoneCounts[0]}");


            while (animPartIndex < size)
            {
                newAnim.Targets[animPartIndex].AddRotationFrame(0, Quaternion.Identity);
                animPartIndex++;
            }

            // PART_TYPE_SIMPLE_QUAT
            size += xanimHeader.BoneCounts[1];

            while (animPartIndex < size)
            {
                int tableSize = dsReader.ReadStruct<BigEndian<ushort>>();

                if (tableSize >= 0x40 && xanimHeader.Numframes >= 0x100)
                    dsReader.BaseStream.Position += 2 * ((tableSize - 1) >> 8) + 4;

                for (int i = 0; i < (tableSize + 1); i++)
                {
                    var frame = 0;

                    if (xanimHeader.Numframes >= 0x100)
                    {
                        frame = tableSize >= 0x40 ? iReader.ReadStruct<BigEndian<ushort>>() : dsReader.ReadStruct<BigEndian<ushort>>();
                    }
                    else
                    {
                        frame = dbReader.ReadByte();
                    }

                    var x = rdsReader.ReadUInt16();
                }

                animPartIndex++;
            }

            // PART_TYPE_NORMAL_QUAT
            size += xanimHeader.BoneCounts[2];

            while (animPartIndex < size)
            {
                int tableSize = dsReader.ReadStruct<BigEndian<ushort>>();

                if (tableSize >= 0x40 && xanimHeader.Numframes >= 0x100)
                    dsReader.BaseStream.Position += 2 * ((tableSize - 1) >> 8) + 4;

                for (int i = 0; i < (tableSize + 1); i++)
                {
                    var frame = 0;

                    if (xanimHeader.Numframes >= 0x100)
                    {
                        frame = tableSize >= 0x40 ? iReader.ReadStruct<BigEndian<ushort>>() : dsReader.ReadStruct<BigEndian<ushort>>();
                    }
                    else
                    {
                        frame = dbReader.ReadByte();
                    }

                    var packed = rdiReader.ReadStruct<BigEndian<uint>>();

                    var x = (BitConverter.UInt32BitsToSingle(((packed >> 0x00) & 0x1FF) - 2 * ((packed >> 0x00) & 0x100) + 0x40400000) - 3.0f) * 16448.252f;
                    var y = (BitConverter.UInt32BitsToSingle(((packed >> 0x09) & 0x3FF) - 2 * ((packed >> 0x09) & 0x200) + 0x40400000) - 3.0f) * 8208.0312f;
                    var z = (BitConverter.UInt32BitsToSingle(((packed >> 0x13) & 0x3FF) - 2 * ((packed >> 0x13) & 0x200) + 0x40400000) - 3.0f) * 8208.0312f;

                    var sign = (packed & 0x80000000) != 0 ? -1.0f : 1.0f;
                    var idx = 3 - ((packed >> 29) & 0x3);

                    var w = sign / MathF.Sqrt((float)((float)((float)(x * x) + (float)(y * y)) + (float)(z * z)) + 1.0f);

                    x *= w;
                    y *= w;
                    z *= w;

                    var quat = idx switch
                    {
                        0 => new(w, x, y, z),
                        1 => new(z, w, x, y),
                        2 => new(y, z, w, x),
                        3 => new(x, y, z, w),
                        _ => Quaternion.Identity,
                    };

                    newAnim.Targets[animPartIndex].AddRotationFrame(frame, quat);
                }

                animPartIndex++;
            }

            // PART_TYPE_PRECISION_QUAT
            size += xanimHeader.BoneCounts[3];

            while (animPartIndex < size)
            {
                int tableSize = dsReader.ReadStruct<BigEndian<ushort>>();

                if (tableSize >= 0x40 && xanimHeader.Numframes >= 0x100)
                    dsReader.BaseStream.Position += 2 * ((tableSize - 1) >> 8) + 4;

                for (int i = 0; i < (tableSize + 1); i++)
                {
                    var frame = 0;

                    if (xanimHeader.Numframes >= 0x100)
                    {
                        frame = tableSize >= 0x40 ? iReader.ReadStruct<BigEndian<ushort>>() : dsReader.ReadStruct<BigEndian<ushort>>();
                    }
                    else
                    {
                        frame = dbReader.ReadByte();
                    }

                    var i0 = (long)rdsReader.ReadStruct<BigEndian<ushort>>();
                    var i1 = (long)rdsReader.ReadStruct<BigEndian<ushort>>();
                    var i2 = (long)rdsReader.ReadStruct<BigEndian<ushort>>();
                    var i3 = (i0 << 32) | (i1 << 16) | (i2 << 00); // easier to create 1 big 64bit int

                    var ux = (int)((i3 >> 00) & 0x7FFF);
                    var uy = (int)((i3 >> 15) & 0x7FFF);
                    var uz = (int)((i3 >> 30) & 0x7FFF);

                    var x = (BitConverter.Int32BitsToSingle(ux - 2 * (ux & 0x4000) + 0x40400000) - 3.0f) * 256.015625f;
                    var y = (BitConverter.Int32BitsToSingle(uy - 2 * (uy & 0x4000) + 0x40400000) - 3.0f) * 256.015625f;
                    var z = (BitConverter.Int32BitsToSingle(uz - 2 * (uz & 0x4000) + 0x40400000) - 3.0f) * 256.015625f;

                    var sign = ((i3 >> 47) & 1) == 0 ? -1.0f : 1.0f;
                    var idx = 3 - ((i3 >> 45) & 0x3);

                    var w = sign / MathF.Sqrt((float)((float)((float)(x * x) + (float)(y * y)) + (float)(z * z)) + 1.0f);

                    x *= w;
                    y *= w;
                    z *= w;

                    var quat = idx switch
                    {
                        0 => new(w, x, y, z),
                        1 => new(z, w, x, y),
                        2 => new(y, z, w, x),
                        3 => new(x, y, z, w),
                        _ => Quaternion.Identity,
                    };


                    newAnim.Targets[animPartIndex].AddRotationFrame(frame, quat);

                }

                animPartIndex++;
            }

            // PART_TYPE_SIMPLE_QUAT_NO_SIZE
            size += xanimHeader.BoneCounts[4];

            while (animPartIndex < size)
            {
                var packed = dsReader.ReadStruct<BigEndian<ushort>>();

                var x = 0.0f;
                var y = 0.0f;
                var z = (BitConverter.Int32BitsToSingle(((packed >> 0x00) & 0x3FFF) - 2 * ((packed >> 0x00) & 0x4000) + 0x40400000) - 3.0f) * 512.0625f;

                var idx = 1 - ((packed >> 14) & 0x3);
                var sign = ((packed >> 15) & 1) != 0 ? -1.0f : 1.0f;

                var w = sign / MathF.Sqrt((float)((float)((float)(x * x) + (float)(y * y)) + (float)(z * z)) + 1.0f);

                x *= w;
                y *= w;
                z *= w;

                var quat = idx switch
                {
                    0 => new(0, 0, w, z),
                    1 => new(0, 0, z, w),
                    _ => Quaternion.Identity,
                };

                newAnim.Targets[animPartIndex].AddRotationFrame(0, quat);

                animPartIndex++;
            }

            // PART_TYPE_NORMAL_QUAT_NO_SIZE
            size += xanimHeader.BoneCounts[5];

            while (animPartIndex < size)
            {
                var packed = diReader.ReadStruct<BigEndian<uint>>();

                var x = (BitConverter.UInt32BitsToSingle(((packed >> 0x00) & 0x1FF) - 2 * ((packed >> 0x00) & 0x100) + 0x40400000) - 3.0f) * 16448.252f;
                var y = (BitConverter.UInt32BitsToSingle(((packed >> 0x09) & 0x3FF) - 2 * ((packed >> 0x09) & 0x200) + 0x40400000) - 3.0f) * 8208.0312f;
                var z = (BitConverter.UInt32BitsToSingle(((packed >> 0x13) & 0x3FF) - 2 * ((packed >> 0x13) & 0x200) + 0x40400000) - 3.0f) * 8208.0312f;

                var sign = (packed & 0x80000000) != 0 ? -1.0f : 1.0f;
                var idx = 3 - ((packed >> 29) & 0x3);

                var w = sign / MathF.Sqrt((float)((float)((float)(x * x) + (float)(y * y)) + (float)(z * z)) + 1.0f);

                x *= w;
                y *= w;
                z *= w;

                var quat = idx switch
                {
                    0 => new(w, x, y, z),
                    1 => new(z, w, x, y),
                    2 => new(y, z, w, x),
                    3 => new(x, y, z, w),
                    _ => Quaternion.Identity,
                };

                newAnim.Targets[animPartIndex].AddRotationFrame(0, quat);
                animPartIndex++;
            }

            // PART_TYPE_TRANS
            size += xanimHeader.BoneCounts[6];

            while (animPartIndex < size)
            {
                var i0 = (long)dsReader.ReadStruct<BigEndian<ushort>>();
                var i1 = (long)dsReader.ReadStruct<BigEndian<ushort>>();
                var i2 = (long)dsReader.ReadStruct<BigEndian<ushort>>();
                var i3 = (i0 << 32) | (i1 << 16) | (i2 << 00); // easier to create 1 big 64bit int

                var ux = (int)((i3 >> 00) & 0x7FFF);
                var uy = (int)((i3 >> 15) & 0x7FFF);
                var uz = (int)((i3 >> 30) & 0x7FFF);

                var x = (BitConverter.Int32BitsToSingle(ux - 2 * (ux & 0x4000) + 0x40400000) - 3.0f) * 256.015625f;
                var y = (BitConverter.Int32BitsToSingle(uy - 2 * (uy & 0x4000) + 0x40400000) - 3.0f) * 256.015625f;
                var z = (BitConverter.Int32BitsToSingle(uz - 2 * (uz & 0x4000) + 0x40400000) - 3.0f) * 256.015625f;

                var sign = ((i3 >> 47) & 1) == 0 ? -1.0f : 1.0f;
                var idx = 3 - ((i3 >> 45) & 0x3);

                var w = sign / MathF.Sqrt((float)((float)((float)(x * x) + (float)(y * y)) + (float)(z * z)) + 1.0f);

                x *= w;
                y *= w;
                z *= w;

                var quat = idx switch
                {
                    0 => new(w, x, y, z),
                    1 => new(z, w, x, y),
                    2 => new(y, z, w, x),
                    3 => new(x, y, z, w),
                    _ => Quaternion.Identity,
                };


                newAnim.Targets[animPartIndex].AddRotationFrame(0, quat);

                animPartIndex++;
            }

            // PART_TYPE_SMALL_TRANS
            animPartIndex = 0;
            size = xanimHeader.BoneCounts[7];

            while (animPartIndex < size)
            {
                var bone = dbReader.ReadByte();

                var minVecX = diReader.ReadStruct<BigEndian<float>>();
                var minVecY = diReader.ReadStruct<BigEndian<float>>();
                var minVecZ = diReader.ReadStruct<BigEndian<float>>();

                var sizeVecX = diReader.ReadStruct<BigEndian<float>>();
                var sizeVecY = diReader.ReadStruct<BigEndian<float>>();
                var sizeVecZ = diReader.ReadStruct<BigEndian<float>>();

                int tableSize = dsReader.ReadStruct<BigEndian<ushort>>();

                if (tableSize >= 0x40 && xanimHeader.Numframes >= 0x100)
                    dsReader.BaseStream.Position += 2 * ((tableSize - 1) >> 8) + 4;

                for (int i = 0; i < (tableSize + 1); i++)
                {
                    var frame = 0;

                    if (xanimHeader.Numframes >= 0x100)
                    {
                        frame = tableSize >= 0x40 ? iReader.ReadStruct<BigEndian<ushort>>() : dsReader.ReadStruct<BigEndian<ushort>>();
                    }
                    else
                    {
                        frame = dbReader.ReadByte();
                    }

                    var x = (sizeVecX * rdbReader.ReadByte()) + minVecX;
                    var y = (sizeVecY * rdbReader.ReadByte()) + minVecY;
                    var z = (sizeVecZ * rdbReader.ReadByte()) + minVecZ;

                    newAnim.Targets[bone].AddTranslationFrame(frame, new Vector3(x, y, z) * 2.54f);
                }

                animPartIndex++;
            }

            // PART_TYPE_TRANS
            animPartIndex = 0;
            size = xanimHeader.BoneCounts[8];

            while (animPartIndex < size)
            {
                var bone = dbReader.ReadByte();

                var minVecX = diReader.ReadStruct<BigEndian<float>>();
                var minVecY = diReader.ReadStruct<BigEndian<float>>();
                var minVecZ = diReader.ReadStruct<BigEndian<float>>();

                var sizeVecX = diReader.ReadStruct<BigEndian<float>>();
                var sizeVecY = diReader.ReadStruct<BigEndian<float>>();
                var sizeVecZ = diReader.ReadStruct<BigEndian<float>>();

                int tableSize = dsReader.ReadStruct<BigEndian<ushort>>();

                if (tableSize >= 0x40 && xanimHeader.Numframes >= 0x100)
                    dsReader.BaseStream.Position += 2 * ((tableSize - 1) >> 8) + 4;

                for (int i = 0; i < (tableSize + 1); i++)
                {
                    var frame = 0;

                    if (xanimHeader.Numframes >= 0x100)
                    {
                        frame = tableSize >= 0x40 ? iReader.ReadStruct<BigEndian<ushort>>() : dsReader.ReadStruct<BigEndian<ushort>>();
                    }
                    else
                    {
                        frame = dbReader.ReadByte();
                    }

                    var x = (sizeVecX * rdsReader.ReadStruct<BigEndian<ushort>>()) + minVecX;
                    var y = (sizeVecY * rdsReader.ReadStruct<BigEndian<ushort>>()) + minVecY;
                    var z = (sizeVecZ * rdsReader.ReadStruct<BigEndian<ushort>>()) + minVecZ;

                    newAnim.Targets[bone].AddTranslationFrame(frame, new Vector3(x, y, z) * 2.54f);
                }

                animPartIndex++;
            }

            // PART_TYPE_TRANS_NO_SIZE
            animPartIndex = 0;
            size = xanimHeader.BoneCounts[9];

            while (animPartIndex < size)
            {
                var bone = dbReader.ReadByte();

                var x = diReader.ReadStruct<BigEndian<float>>();
                var y = diReader.ReadStruct<BigEndian<float>>();
                var z = diReader.ReadStruct<BigEndian<float>>();

                newAnim.Targets[bone].AddTranslationFrame(0, new Vector3(x, y, z) * 2.54f);

                animPartIndex++;
            }

            // PART_TYPE_NO_TRANS
            animPartIndex = 0;
            size = xanimHeader.BoneCounts[10];

            while (animPartIndex < size)
            {
                var bone = dbReader.ReadByte();

                animPartIndex++;
            }

            //Console.WriteLine($"{diReader.BaseStream.Position} / {diReader.BaseStream.Length}");
            //Console.WriteLine($"{dbReader.BaseStream.Position} / {dbReader.BaseStream.Length}");
            //Console.WriteLine($"{dsReader.BaseStream.Position} / {dsReader.BaseStream.Length}");
            //Console.WriteLine($"{rdiReader.BaseStream.Position} / {rdiReader.BaseStream.Length}");
            //Console.WriteLine($"{rdbReader.BaseStream.Position} / {rdbReader.BaseStream.Length}");
            //Console.WriteLine($"{rdsReader.BaseStream.Position} / {rdsReader.BaseStream.Length}");

            //Console.WriteLine();

            Directory.CreateDirectory(folder);

            Instance.TranslatorFactory.Save(Path.Combine(folder, asset.Name + ".seanim"), newAnim);

            //// PART_TYPE_SMALL_TRANS
            //animPartIndex = 0;
            //size = xanimHeader.BoneCounts[7];

            //while (animPartIndex < size)
            //{
            //    var x = dsReader.ReadUInt16();
            //    var y = dsReader.ReadUInt16();
            //    var z = dsReader.ReadUInt16();
            //    animPartIndex++;
            //}
        }
    }
}