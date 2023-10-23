using System.Collections.Generic;
using CMS21MP.ClientSide.DataHandle;
using CMS21MP.SharedData;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace CMS21MP.ClientSide.Data
{
    [HarmonyPatch]
    public static class GarageInteraction
    {

        #region Lifter


        public static bool needToTrigger = false;

        [HarmonyPatch(typeof(CarLifter), "Action")]
        [HarmonyPostfix]
        public static void LifterFix(int actionType, CarLifter __instance)
        {
            int PostLiftVal = (int)__instance.currentState + actionType;
            if(!needToTrigger)
                ClientSend.SendLifterNewPos(actionType, PostLiftVal, __instance.connectedCarLoader.gameObject.name[10]);
            else
                needToTrigger = false;
            
            MelonLogger.Msg("Sended New lifter pos to : " + __instance.connectedCarLoader.gameObject.name[10] + 
                            " action: " + actionType + " pos: " + PostLiftVal);
        }
        #endregion
    }
}