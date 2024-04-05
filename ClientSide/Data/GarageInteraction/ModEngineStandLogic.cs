using CMS21Together.ClientSide.Handle;
using HarmonyLib;
using Il2Cpp;

namespace CMS21Together.ClientSide.Data.GarageInteraction
{
    public static class ModEngineStandLogic
    {
        public static bool listenToEngineStandLogic;
        
        [HarmonyPatch(typeof(EngineStandLogic), "SetEngineStandAngle")]
        [HarmonyPrefix]
        public static void SetEngineAnglePatch(float newAngle)
        {
            if(listenToEngineStandLogic)
                ClientSend.SendEngineAngle(newAngle);
        }
    }
}