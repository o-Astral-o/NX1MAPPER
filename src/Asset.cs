using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER
{
    public class Asset(string name, string type, long address, AssetPool pool)
    {
        public string Name { get; set; } = name;

        public string Type { get; set; } = type;

        public long Address { get; set; } = address;

        public AssetPool Pool { get; set; } = pool;
    }
}
