using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER
{
    public abstract class AssetPool(GameInstance instance, string name, int index, int headerSize)
    {
        public GameInstance Instance { get; set; } = instance;

        public string Name { get; set; } = name;

        public int Index { get; set; } = index;

        public int HeaderSize { get; set; } = headerSize;

        public List<Asset> Assets { get; set; } = [];

        public abstract void LoadGeneric();

        public abstract void Export(Asset asset);
    }
}
