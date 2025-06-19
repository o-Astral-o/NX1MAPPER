using Husky;
using NX1GAMER.Helpers;
using NX1GAMER.Structures;
using RedFox.Graphics3D;
using RedFox.Graphics3D.Skeletal;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace NX1GAMER.AssetPools
{
    public class LoadedSoundPool(GameInstance instance, string name, int index, int headerSize) : AssetPool(instance, name, index, headerSize)
    {
        public override void Export(Asset asset)
        {

        }

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

                if (Instance.Memory.ReadNullTerminatedString(ptr) == "weapons/xm108/wpnfoley_xm108_hand_rest_01")
                {
                    var s = Instance.Memory.ReadStruct<LoadedSound>(poolAddress + i * HeaderSize);
                    File.WriteAllBytes("test.dat", Instance.Memory.ReadMemory(s.AudioData, s.AudioBytes));
                    Debugger.Break();
                }

                Assets.Add(new(Instance.Memory.ReadNullTerminatedString(ptr), Name, poolAddress + i * HeaderSize, this));
            }
        }
    }
}
