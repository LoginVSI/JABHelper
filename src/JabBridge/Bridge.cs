using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Linq;
using JabApiLib.JavaAccessBridge;
using AccessibleTreeItem = JabApiLib.JavaAccessBridge.JabHelpers.AccessibleTreeItem;

namespace JabBridge
{
    public class ControlInfo
    {
        public int X, Y, Width, Height, CenterX, CenterY;
    }

    public static class Bridge
    {
        // P/Invoke kernel32 LoadLibrary
        [DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Auto)]
        static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("kernel32.dll")]
        static extern uint GetLastError();

        // Ensure the native JAB client is loaded and handshaken
        static bool _initialized;
        public static void Initialize(int handshakeMs = 500)
        {
            if (_initialized) return;
            var h = LoadLibrary("windowsaccessbridge-64.dll");
            if (h == IntPtr.Zero)
                throw new Exception($"LoadLibrary failed (error {GetLastError()})");

            JabApi.Windows_run();

            // pump messages
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < handshakeMs)
            {
                while (User32.PeekMessage(out var m, IntPtr.Zero, 0, 0, User32.PM_REMOVE))
                {
                    User32.TranslateMessage(ref m);
                    User32.DispatchMessage(ref m);
                }
                Thread.Sleep(5);
            }

            _initialized = true;
        }

        // Flatten every Java window into one list
        public static List<AccessibleTreeItem> CollectAllNodes()
        {
            Initialize();
            var list = new List<AccessibleTreeItem>();
            User32.EnumWindows((hwnd, _) =>
            {
                var root = JabHelpers.GetComponentTree(hwnd, out int vmID);
                if (root != null) CollectNodes(root, list);
                return true;
            }, IntPtr.Zero);
            return list;
        }

        // Grab one control by role & index and return its coords
        public static ControlInfo GetControl(string role, int index)
        {
            var all = CollectAllNodes()
                .Where(n => string.Equals(n.role_en_US, role, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (index < 0 || index >= all.Count)
                throw new IndexOutOfRangeException($"No control at index {index} for role '{role}'");
            var n = all[index];
            int cx = n.x + n.width / 2;
            int cy = n.y + n.height / 2;
            return new ControlInfo {
                X = n.x, Y = n.y,
                Width = n.width, Height = n.height,
                CenterX = cx, CenterY = cy
            };
        }

        static void CollectNodes(AccessibleTreeItem node, List<AccessibleTreeItem> list)
        {
            list.Add(node);
            if (node.children != null)
                foreach (var c in node.children)
                    CollectNodes(c, list);
        }

        // P/Invoke user32 for message pump & window enumeration
        static class User32
        {
            public const uint PM_REMOVE = 0x0001;
            [StructLayout(LayoutKind.Sequential)] public struct POINT { public int x, y; }
            [StructLayout(LayoutKind.Sequential)]
            public struct MSG {
                public IntPtr hwnd; public uint message; public UIntPtr wParam; public IntPtr lParam;
                public uint time; public POINT pt;
            }

            public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

            [DllImport("user32.dll")]
            public static extern bool PeekMessage(
                out MSG lpMsg, IntPtr hWnd,
                uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

            [DllImport("user32.dll")]
            public static extern bool TranslateMessage([In] ref MSG lpMsg);

            [DllImport("user32.dll")]
            public static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

            [DllImport("user32.dll")]
            public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

            [DllImport("user32.dll", CharSet=CharSet.Auto)]
            public static extern int GetWindowText(
                IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        }
    }
}
