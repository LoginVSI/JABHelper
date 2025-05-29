using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using JabBridge;
using AccessibleTreeItem = JabApiLib.JavaAccessBridge.JabHelpers.AccessibleTreeItem;

public static class ExportedMethods
{
    static void Init() => Bridge.Initialize();

    [UnmanagedCallersOnly(
        EntryPoint = "GetControlCenterX",
        CallConvs = new[] { typeof(CallConvCdecl) }
    )]
    public static int GetControlCenterX(IntPtr rolePtr, int index)
    {
        string role = Marshal.PtrToStringUni(rolePtr) ?? "";
        Init();
        return Bridge.GetControl(role, index).CenterX;
    }

    [UnmanagedCallersOnly(
        EntryPoint = "GetControlCenterY",
        CallConvs = new[] { typeof(CallConvCdecl) }
    )]
    public static int GetControlCenterY(IntPtr rolePtr, int index)
    {
        string role = Marshal.PtrToStringUni(rolePtr) ?? "";
        Init();
        return Bridge.GetControl(role, index).CenterY;
    }

    [UnmanagedCallersOnly(
        EntryPoint = "GetControlCenter",
        CallConvs = new[] { typeof(CallConvCdecl) }
    )]
    public static IntPtr GetControlCenter(IntPtr rolePtr, int index)
    {
        string role = Marshal.PtrToStringUni(rolePtr) ?? "";
        Init();
        var info = Bridge.GetControl(role, index);
        string s = $"{info.CenterX},{info.CenterY}";
        return Marshal.StringToHGlobalUni(s);
    }

    [UnmanagedCallersOnly(
        EntryPoint = "FreeString",
        CallConvs = new[] { typeof(CallConvCdecl) }
    )]
	
    public static void FreeString(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero)
            Marshal.FreeHGlobal(ptr);
    }
	
	[UnmanagedCallersOnly(
        EntryPoint = "DumpAllControls",
        CallConvs = new[] { typeof(CallConvCdecl) }
    )]
    public static void DumpAllControls()
    {
        Init();
        var nodes = Bridge.CollectAllNodes();
        foreach (var n in nodes)
        {
            Console.WriteLine(
                $"{n.role_en_US} [{n.name}] @ (x={n.x},y={n.y}) size({n.width}×{n.height})"
            );
        }
    }
	
	[UnmanagedCallersOnly(
        EntryPoint = "FindControlIndexByName",
        CallConvs = new[] { typeof(CallConvCdecl) }
    )]
    public static int FindControlIndexByName(
        IntPtr rolePtr,
        IntPtr nameSubPtr)
    {
        // 1) marshal the input strings
        string role    = Marshal.PtrToStringUni(rolePtr)    ?? string.Empty;
        string nameSub = Marshal.PtrToStringUni(nameSubPtr) ?? string.Empty;

        // 2) make sure JAB is initialized
        Init();

        // 3) collect *all* UI nodes and filter by role
        List<AccessibleTreeItem> list = Bridge
            .CollectAllNodes()                                     // List<AccessibleTreeItem>
            .Where(n => string.Equals(n.role_en_US, role, 
                          StringComparison.OrdinalIgnoreCase))
            .ToList();                                             // now a real List<>

        // 4) scan for the first whose .name contains your substring
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].name?.Contains(nameSub, StringComparison.OrdinalIgnoreCase) == true)
                return i;
        }

        // 5) not found
        return -1;
    }
	
	[UnmanagedCallersOnly(
        EntryPoint = "GetControlCenterByNameAndOccurrence",
        CallConvs = new[] { typeof(CallConvCdecl) }
    )]
    public static IntPtr GetControlCenterByNameAndOccurrence(
        IntPtr rolePtr,
        IntPtr nameSubPtr,
        int occurrence)
    {
        string role    = Marshal.PtrToStringUni(rolePtr)    ?? "";
        string nameSub = Marshal.PtrToStringUni(nameSubPtr) ?? "";
        Init();

        // 1) filter the full tree by role+name
        var matches = Bridge.CollectAllNodes()
            .Where(n =>
                string.Equals(n.role_en_US, role, StringComparison.OrdinalIgnoreCase) &&
                n.name.Contains(nameSub, StringComparison.OrdinalIgnoreCase)
            )
            .ToList();

        // 2) bounds check
        if (occurrence < 0 || occurrence >= matches.Count)
            return IntPtr.Zero;

        // 3) build "x,y"
        var info = matches[occurrence];
        string s = $"{info.x + info.width/2},{info.y + info.height/2}";
        return Marshal.StringToHGlobalUni(s);
    }

	
}
