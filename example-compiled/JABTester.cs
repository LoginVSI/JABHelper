// TARGET:"%USERPROFILE%\Desktop\example-compiled\MainFlow.lnk"
// START_IN:""

using LoginPI.Engine.ScriptBase;
using System;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;

public class JABTester : ScriptBase 
{
    private void Execute() 
	
	{
		// Load native DLL
		Custom.LoadDll(@"%USERPROFILE%\Desktop\example-compiled\JavaUiCoords.dll");

        // Ensure JAB is enabled - use full path if needed (eg., multiple JREs)
        ShellExecute("jabswitch /enable");
        
        Wait(2);

		START(mainWindowTitle:"MainFlow", timeout:120, processName:"java", forceKillOnExit:false);
		 Wait(2);
		
int idxOOBS = FindDuplicateJABControlByOccurrence("push button", "Open Orders By Store", moveMouse: true, click: true, occurrence: 1);

if (idxOOBS >= 0) {

    StartTimer("OOBS_Menu");
} else {
    Log("OOBS button not found. Cancel timer.");
    Log(idxOOBS);
    CancelTimer("OOBS_Menu");
}


// Find transaction details button and send click
int idxTDButton = FindJABControl("push button", "Transaction Detail", moveMouse: true, click: true);
if (idxTDButton >= 0) {
    StopTimer("OOBS_Menu");
    Log("TD Button found - start timer and click"); 
    StartTimer("TD_Load");
    }
else {
    Log("TD button not found - cancel timer."); 
    CancelTimer("TD_Load");
     }

// Find back button and send click
int idxBack = FindJABControl("push button", "Back", moveMouse: true, click: true);
if (idxBack >= 0) {
    StopTimer("TD_Load");
    }
else {
    Log("Back button not found!");
    
     }
     
// Find close button and send click
int idxCloseButton = FindJABControl("push button", "CLOSE", moveMouse: true, click: true);
if (idxCloseButton >=0) {
    Log("Close button found");
    }



// dump controls currently seen
Wait(5);
Log("Dumping controls:");
Custom.DumpAllControls();

// Uncomment environment.exit and comment STOP to keep app open 
// Environment.Exit(0);

STOP();

    }
    
    
static (int X, int Y) GetControlCenter(string role, int idx)
{
    int x = Custom.GetControlCenterX(role, idx);
    int y = Custom.GetControlCenterY(role, idx);
    return (x, y);
       
}

// FindJABControl - a function that finds a Java‑Access‑Bridge control by role+name, and optionally move/click on it.

int FindJABControl(
    string role,
    string nameSubstring,
    bool moveMouse        = false,
    bool click            = false,
    bool doubleClick      = false,
    int timeoutSeconds    = 120,
    int pollIntervalMs    = 500
) {
    var sw = Stopwatch.StartNew();
    int idx;
    // 1) wait for control to exist
    while (sw.Elapsed.TotalSeconds < timeoutSeconds) {
        idx = Custom.FindControlIndexByName(role, nameSubstring);
        if (idx >= 0) goto foundIndex;
        Thread.Sleep(pollIntervalMs);
    }
    throw new TimeoutException(
        $"Timed out after {timeoutSeconds}s waiting for '{role}' containing '{nameSubstring}'.");

    foundIndex:
    idx = Custom.FindControlIndexByName(role, nameSubstring);

    // if no further action requested, just return
    if (!moveMouse && !click && !doubleClick)
        return idx;

    // 2) wait for its center‐coords
    sw.Restart();
    int cx, cy;
    while (true) {
        try {
            cx = Custom.GetControlCenterX(role, idx);
            cy = Custom.GetControlCenterY(role, idx);
            break;
        }
        catch {
            if (sw.Elapsed.TotalSeconds >= timeoutSeconds)
                throw new TimeoutException(
                  $"Timed out after {timeoutSeconds}s getting center for '{role}'@{idx}");
            Thread.Sleep(pollIntervalMs);
        }
    }

    // 3) move
    MouseMove(cx, cy, continueOnError: false);

    // 4) optional click
    if (click) {
        MouseDown();
        MouseUp();
    }

    // 5) optional double‑click
    if (doubleClick) {
        DoubleClick(cx, cy, continueOnError: false);
    }

    return idx;
}


// In the rare occurrence that duplicate names are found with the same role, this function can be used to point to the specific index of controls returned. Start with 0.
int FindDuplicateJABControlByOccurrence(
    string role,
    string nameSubstring,
    int occurrence      = 0,    // 0 = first, 1 = second, etc.
    bool moveMouse      = false,
    bool click          = false,
    bool doubleClick    = false,
    int timeoutSeconds  = 120,
    int pollIntervalMs  = 500
)
{

    var sw = Stopwatch.StartNew();
    IntPtr p = IntPtr.Zero;

    // 1) Poll until the DLL returns a non‑zero pointer (i.e. match found)
    while (sw.Elapsed.TotalSeconds < timeoutSeconds)
    {
        p = Custom.GetControlCenterByNameAndOccurrence(
              role, nameSubstring, occurrence);
        if (p != IntPtr.Zero) 
            break;
        Thread.Sleep(pollIntervalMs);
    }
    if (p == IntPtr.Zero)
        throw new TimeoutException(
          $"Timed out after {timeoutSeconds}s waiting for {occurrence+1}‑th '{role}' containing '{nameSubstring}'.");

    // 2) Marshal back the "X,Y"
    string coords = Marshal.PtrToStringUni(p) ?? "";
    Custom.FreeString(p);
    if (string.IsNullOrEmpty(coords))
        throw new Exception("DLL returned empty coords for control.");

    var parts = coords.Split(',');
    int cx = int.Parse(parts[0]), cy = int.Parse(parts[1]);

    // 3) Move & click/double‑click if requested
    if (moveMouse)    MouseMove(cx, cy, continueOnError: false);
    if (click)        { MouseDown(); MouseUp(); }
    if (doubleClick)  DoubleClick(cx, cy, continueOnError: false);

    // 4) Return the logical occurrence
    return occurrence;
}


    private static class Custom
{
    // (1) loader for the DLL - LoadDll("…\\JavaUiCoords.dll")
    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibrary(string lpLibFileName);

    public static void LoadDll(string dllPath)
    {
        var full = Environment.ExpandEnvironmentVariables(dllPath);
        var handle = LoadLibrary(full);
        if (handle == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error(),
                $"Failed to load DLL \"{full}\"");
    }

    // (2) the DLL exports/entry points
    
    // Get the X coords for center of control from role and index
    [DllImport("JavaUiCoords.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetControlCenterX(
        [MarshalAs(UnmanagedType.LPWStr)] string role,
        int index);

    // Get the Y coords for center of control from role and index
    [DllImport("JavaUiCoords.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int GetControlCenterY(
        [MarshalAs(UnmanagedType.LPWStr)] string role, int index);
        
    // Get X,Y coords for center of control from role and index
    [DllImport("JavaUiCoords.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern IntPtr GetControlCenter(
        [MarshalAs(UnmanagedType.LPWStr)] string role, int index);

    // Release the memory from string returns
    [DllImport("JavaUiCoords.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern void FreeString(IntPtr ptr);
    
    // This dumps all controls to the console/log - useful at the end of the script to see all visible control names and roles
      [DllImport("JavaUiCoords.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void DumpAllControls();
    
    // We use this to search by role and name to get the index # 
    [DllImport("JavaUiCoords.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern int FindControlIndexByName(string role, string nameSub);
    
    // Use this one if there's multiple controls with the same name presented - used in the FindDuplicateJABControlByOccurrence function
    [DllImport("JavaUiCoords.dll",CallingConvention=CallingConvention.Cdecl,CharSet=CharSet.Unicode)]
    public static extern IntPtr GetControlCenterByNameAndOccurrence(string role, string nameSub, int occurrence);

    
}
    
    }