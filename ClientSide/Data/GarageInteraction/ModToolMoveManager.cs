using CMS21Together.ClientSide.Handle;
using CMS21Together.Shared.Data;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace CMS21Together.ClientSide.Data.GarageInteraction
{
    [HarmonyPatch]
    public static class ModToolMoveManager
    {
        
        public static bool listenToMove = true;
        
        [HarmonyPatch(typeof(ToolsMoveManager), nameof(ToolsMoveManager.MoveTo))]
        [HarmonyPrefix]
        public static void MoveToolPatch(IOSpecialType tool, CarPlace place, bool playSound)
        {
            if(!Client.Instance.isConnected) return;
            if (listenToMove)
            {
                MelonLogger.Msg($"Moving {tool} to : {place}.");
                ClientSend.SendToolPosition(tool, (ModCarPlace)place, playSound);
            }
        }
        
        [HarmonyPatch(typeof(ToolsMoveManager), nameof(ToolsMoveManager.SetOnDefaultPosition))]
        [HarmonyPrefix]
        public static void ResetToolPosPatch(IOSpecialType tool)
        {
            if(!Client.Instance.isConnected) return;
            if (listenToMove)
            {
                MelonLogger.Msg($"Moving {tool} to : default.");
                ClientSend.SendToolPosition(tool, ModCarPlace.none);
            }
        }
    }
}