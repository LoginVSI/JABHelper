Compiled DLL and Script Example for JavaUiCoords (JAB Helper)

Script Configuration

1) Point the TARGET to the necessary Java app starting point. This can be a shortcut or utilize java <classname>, such as java MainFlow as we have in our provided example Java app. Ensure the MainFlow.lnk shortcut provided points to the correct JRE location. The example was written for everything to be within the provided example-compiled directory, located in the user’s desktop directory. Adjust as needed. 
2) Notice we have a jabswitch /enable command within the script – this is a safety net to ensure JAB has been enabled. If the Java PATH is not correctly defined or you have multiple JREs, ensure you include the full path to the appropriate jabswitch.exe here. 
3) Configure the correct DLL location
4) Configure the START parameters as necessary
5) Script out the actions using FindDuplicateJABControlByOccurrence and FindJABControl – wrap timers as necessary as in the example. See “Helper Functions” below for usage. At the last page, you can also use Custom.DumpAllControls() to dump all of the controls/roles currently seen on the screen, which can be useful for building your next action.

Helper Functions

The script defines several helper methods to interact with Java UI controls. Here are the main ones utilized: 
- FindJABControl(string role, string nameSub, bool moveMouse = false, bool click = false, bool doubleClick = false): Locates a control by role and name, optionally moves the mouse to its center, and performs click or double-click actions based on parameter flags.
  
- FindDuplicateJABControlByOccurrence(string role, string nameSub, int occurrence, bool moveMouse = false, bool click = false, bool doubleClick = false): Locates a control by role, name, AND occurrence number. This was added in “v2,” along with its associated DLL entry point, to account for the fact that Java apps can technically have the same name/role on the same screen. Optionally moves the mouse to its center and performs click or double-click actions based on parameter flags. To showcase this helper, the first screen in the example Java app has two duplicate buttons, so we utilize this first in the script.
  
- Custom.DumpAllControls() to dump all of the roles/control names currently seen on the screen to the log/debug output, which can be useful at the end of the script for building your next action. 

Google Access Bridge Explorer

Included here in example-compiled, Java Access Bridge Explorer is a great tool for analyzing the control names/roles. It also validates that jabswitch /enable has appropriately been executed. It works very similarly to Application Xray – just drag the crosshairs to the appropriate Java control, and you have the exact role type and name that you can utilize for the helper functions above:

 ![image](https://github.com/user-attachments/assets/dc3ebfd5-3725-4de8-bfa2-1c705fdf7e03)


DLL Entry Points
The bottom of the script declares P/Invoke signatures for the native JavaUiCoords.dll:
- [DllImport("JavaUiCoords.dll", CallingConvention = CallingConvention.Cdecl)]
  public static extern void DumpAllControls();
- [DllImport("JavaUiCoords.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
  public static extern int FindControlIndexByName(string role, string nameSub);
- [DllImport("JavaUiCoords.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
  public static extern IntPtr GetControlCenterByNameAndOccurrence(string role, string nameSub, int occurrence);

  The JabApi and JabHelpers by jdog3 were foundational for this project - jdog3's JAB .NET sample can be found here: https://github.com/jdog3/JavaAccessBridge.Net-Sample/tree/master
