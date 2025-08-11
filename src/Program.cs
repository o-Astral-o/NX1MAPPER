using NX1GAMER.Helpers;
using NX1GAMER.Structures;
using RedFox.IO;
using RedFox.IO.Extensions;
using Spectre.Console;
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace NX1GAMER
{
    internal partial class Program
    {
        static void Main(string[] args)
        {
            AnsiConsole.MarkupLine("[red]-----------------------------[/] ");
            AnsiConsole.MarkupLine("[red]         NX1MAPPER           [/] ");
            AnsiConsole.MarkupLine("[red]-----------------------------[/] ");

            var configFile = args[0];
            var dumpFile = args[1];
            var gameFolder = args[2];
            
            var memory = GameMemory.LoadFromXenia(dumpFile);
            var config = JsonSerializer.Deserialize(File.ReadAllText(configFile), GameConfigContext.Default.GameConfig) ?? throw new Exception("Invalid Config File");
            var instance = new GameInstance(memory, config, gameFolder).Initialize();

            foreach (var pool in instance.Pools)
            {
                if (pool.Name == "gfxMap")
                {
                    foreach (var asset in pool.Assets)
                    {
                        asset.Pool.Export(asset);
                    }
                }
            }
        }
    }
}
