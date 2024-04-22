using CMS21Together.ClientSide.Handle;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace CMS21Together.ClientSide.Data.GarageInteraction
{
    [HarmonyPatch]
    public static class ModEngineStandLogic
    {
        public static bool listenToEngineStandLogic;
        
        [HarmonyPatch(typeof(EngineStandLogic), "IncreaseEngineStandAngle")]
        [HarmonyPrefix]
        public static void SetEngineAnglePatch(float val)
        {
            MelonLogger.Msg("SetEngineStandAngle called.");
            if(listenToEngineStandLogic)
                ClientSend.SendEngineAngle(val);
        }


        [HarmonyPatch(typeof(EngineStandLogic), "SetGroupOnEngineStand")]
        [HarmonyPrefix]
        public static void SetEngineGroupPatch(GroupItem groupItem, bool withFade=true, EngineStandLogic __instance=null)
        {
            MelonLogger.Msg("SetGroupOnEngineStand called.");
        }
        
        [HarmonyPatch(typeof(EngineStandLogic), "SetEngineOnEngineStand")]
        [HarmonyPrefix]
        public static void SetEnginePatch(Item engine, EngineStandLogic __instance=null)
        {
            MelonLogger.Msg("SetEngineOnEngineStand called.");
        }
        
    }
}