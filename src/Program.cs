using NX1GAMER.Helpers;
using NX1GAMER.Structures;
using RedFox.IO;
using RedFox.IO.Extensions;
using Spectre.Console;
using System;
using System.Buffers.Binary;
using System.Diagnostics;
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
            AnsiConsole.MarkupLine("[red] NX1GAMER - he do be gaming tho lmao[/] ");
            AnsiConsole.MarkupLine("[red]-----------------------------[/] ");

            var configFile = args[0];
            var dumpFile = args[1];
            var gameFolder = args[2];

            AnsiConsole.MarkupLine($"ramming the dump file: [green]{dumpFile}[/]...");
            var memory = GameMemory.LoadFromXenia(dumpFile);
            AnsiConsole.MarkupLine($"[green]{dumpFile}[/] do be loaded tho");
            AnsiConsole.MarkupLine($"trying out this ratio: [green]{configFile}[/]");
            var config = JsonSerializer.Deserialize(File.ReadAllText(configFile), GameConfigContext.Default.GameConfig) ?? throw new Exception("config is fucked");
            AnsiConsole.MarkupLine($"lmao it actually loaded: [green]{configFile}[/]");
            AnsiConsole.MarkupLine($"da game lives here laddo: [green]{gameFolder}[/]");
            AnsiConsole.MarkupLine($"we loading now boyo");
            var instance = new GameInstance(memory, config, gameFolder).Initialize();
            AnsiConsole.MarkupLine($"we're safe now, she loaded all pools lad");

            foreach (var pool in instance.Pools)
            {
                //if (pool.Name == "xanim")
                {
                    foreach (var asset in pool.Assets)
                    {
                        //if (asset.Name.Contains("civilian_run_hunched_a"))
                        {
                            AnsiConsole.MarkupLine($"exporting me lad: [green]{asset.Name}[/] from me boss: [blue]{asset.Type}[/] :D");
                            asset.Pool.Export(asset);
                            AnsiConsole.MarkupLine($"she fucking exported HAHA");
                        }
                    }
                }
            }
        }
    }
}
