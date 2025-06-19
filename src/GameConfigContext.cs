using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NX1GAMER
{
    [JsonSerializable(typeof(GameConfig))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(int))]
    public partial class GameConfigContext : JsonSerializerContext
    {
    }
}
