using System;
using System.Collections.Generic;
using System.Linq;
using JabBridge;
using JabApiLib.JavaAccessBridge;
using AccessibleTreeItem = JabApiLib.JavaAccessBridge.JabHelpers.AccessibleTreeItem;

class Program
{
    static void Main(string[] args)
    {
        // 1) Initialize the bridge (loads native DLL, handshakes)
        Bridge.Initialize();

        if (args.Length == 0)
        {
            // --- Full dump mode ---
            var nodes = Bridge.CollectAllNodes();
            Console.WriteLine($"Found {nodes.Count} UI elements:");
            foreach (var n in nodes)
            {
                Console.WriteLine(
                    $"{n.role_en_US} [{n.name}] (x={n.x},y={n.y},w={n.width},h={n.height})"
                );
            }
        }
        else if (args.Length == 2)
        {
            // --- Single-control mode: <role> <index> ---
            string role = args[0];
            if (!int.TryParse(args[1], out int index))
            {
                Console.Error.WriteLine($"Invalid index: {args[1]}");
                Environment.Exit(1);
            }

            var info = Bridge.GetControl(role, index);
            // print only center X,Y
            Console.WriteLine($"{info.CenterX},{info.CenterY}");
        }
        else
        {
            Console.Error.WriteLine("Usage:");
            Console.Error.WriteLine("  JabDumpConsole.exe            # dump full list");
            Console.Error.WriteLine("  JabDumpConsole.exe <role> <index>  # get center coords");
            Environment.Exit(1);
        }
    }
}
