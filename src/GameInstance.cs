using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NX1GAMER.AssetPools;
using RedFox.Graphics3D.Translation;
using Spectre.Console;

namespace NX1GAMER
{
    public class GameInstance
    {
        public GameMemory Memory { get; set; }

        public List<AssetPool> Pools { get; set; } = [];

        public GameConfig Config { get; set; }

        public string GameFolder { get; set; }

        public Graphics3DTranslatorFactory TranslatorFactory { get; set; }

        public GameInstance(GameMemory memory, GameConfig config, string gameFolder)
        {
            Memory = memory;
            Config = config;
            GameFolder = gameFolder;
            TranslatorFactory = new Graphics3DTranslatorFactory().WithDefaultTranslators();

            Pools.Add(new XModelPool(this, "xmodel", 4, 288));
            Pools.Add(new NX1XAnimPool(this, "xanim", 2, 88));
            // Pools.Add(new ImagePool(this, "image", 8, 112));
            // Pools.Add(new ImagePool(this, "loadedsounds", 12, 100));
        }

        public GameInstance Initialize()
        {
            Pools.ForEach(x =>
            {
                AnsiConsole.MarkupLine($"the poolo: [green]{x.Name}[/] is loading lmao");
                x.LoadGeneric();
                AnsiConsole.MarkupLine($"ficj she loaded [green]{x.Assets.Count}[/] thingos");
            });
            return this;
        }

        public long GetPoolAddress(int pool) => Memory.ReadUInt32(Config.GetAddress("PoolsAddress") + pool * 4);

        public int GetPoolSize(int pool) => Memory.ReadInt32(Config.GetAddress("PoolSizesAddress") + pool * 4);

        public string GetString(int str) => Memory.ReadNullTerminatedString(Memory.ReadUInt32(Config.GetAddress("StringTableAddress")) + str * (Config.IsMP ? 12 : 16) + 4);

        public int GetImageIndex(long address) => (int)((address - GetPoolAddress(8)) / 112);
    }
}
