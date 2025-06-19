using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NX1GAMER
{
    public class GameConfig
    {
        public Dictionary<string, string>? Addresses { get; set; }

        public bool IsMP { get; set; }

        public long GetAddress(string address)
        {
            if (Addresses is null)
                throw new NullReferenceException("Addresses is null.");
            if (!Addresses.TryGetValue(address, out var value))
                throw new KeyNotFoundException($"{address} does not exist in the config.");

            return Convert.ToInt64(value, 16);
        }
    }
}
