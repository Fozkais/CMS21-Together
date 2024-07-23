using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data.Vanilla.Cars;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class ToolsMoveManager
{
    public static bool listenToMove = true;

    public static void Reset()
    {
        listenToMove = true;
    }
    
    [HarmonyPatch(typeof(Il2Cpp.ToolsMoveManager), nameof(Il2Cpp.ToolsMoveManager.MoveTo))]
    [HarmonyPrefix]
    public static void MoveToolPatch(IOSpecialType tool, CarPlace place, bool playSound)
    {
        if(!Client.Instance.isConnected || !listenToMove) { listenToMove = true; return;}

        ClientSend.ToolPositionPacket(tool, (ModCarPlace)place, playSound);
    }
        
    [HarmonyPatch(typeof(Il2Cpp.ToolsMoveManager), nameof(Il2Cpp.ToolsMoveManager.SetOnDefaultPosition))]
    [HarmonyPrefix]
    public static void ResetToolPosPatch(IOSpecialType tool)
    {
        if(!Client.Instance.isConnected || !listenToMove) { listenToMove = true; return;}
        
        ClientSend.ToolPositionPacket(tool, ModCarPlace.none);
    }
}