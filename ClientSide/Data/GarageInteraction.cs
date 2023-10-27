using System.Collections;
using System.Collections.Generic;
using CMS21MP.ClientSide.DataHandle;
using CMS21MP.CustomData;
using CMS21MP.SharedData;
using HarmonyLib;
using Il2Cpp;
using Il2CppSystem;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide.Data
{
    [HarmonyPatch]
    public static class GarageInteraction
    {

        #region Lifter


            public static bool needToTrigger = true;

            [HarmonyPatch(typeof(CarLifter), "Action")]
            [HarmonyPostfix]
            public static void LifterFix(int actionType, CarLifter __instance)
            {
                if (needToTrigger)
                {
                    int PostLiftVal = (int)__instance.currentState + actionType;
                    MelonLogger.Msg("PostLiftVal :" + PostLiftVal);
                    int convertedToInt = __instance.connectedCarLoader.gameObject.name[10] - '0';
                    MelonLogger.Msg("Sended New lifter pos to : " + convertedToInt + 
                                    " action: " + actionType + " pos: " + PostLiftVal);
                    ClientSend.SendLifterNewPos(actionType, PostLiftVal,  convertedToInt);
                }
            }
            public static IEnumerator PauseUpdating()
            {
                needToTrigger = false;
                yield return new WaitForSeconds(0.25f);
                needToTrigger = true;
            }
        #endregion
        
        #region TireChanger
        
            
        [HarmonyPatch(typeof(TireChangerLogic), "SetGroupOnTireChanger")]
        [HarmonyPostfix]
        public static void TireChangerFix(GroupItem groupItem, bool instant, bool connect, TireChangerLogic __instance)
        {
            if (groupItem == null) return;
            MelonLogger.Msg($"Tire Changer Triggered! : {instant} , {connect}");
            ClientSend.SendTireChange(new ModGroupItem(groupItem), instant,connect);
        }
        
        #endregion

    }
}