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


            public static bool Lifter_needToTrigger = true;

            [HarmonyPatch(typeof(CarLifter), "Action")]
            [HarmonyPostfix]
            public static void LifterFix(int actionType, CarLifter __instance)
            {
                if (Lifter_needToTrigger)
                {
                    int PostLiftVal = (int)__instance.currentState + actionType;
                    MelonLogger.Msg("PostLiftVal :" + PostLiftVal);
                    int convertedToInt = __instance.connectedCarLoader.gameObject.name[10] - '0';
                    MelonLogger.Msg("Sended New lifter pos to : " + convertedToInt + 
                                    " action: " + actionType + " pos: " + PostLiftVal);
                    ClientSend.SendLifterNewPos(actionType, PostLiftVal,  convertedToInt);
                }
            }
            public static IEnumerator LifterPauseUpdating()
            {
                Lifter_needToTrigger = false;
                yield return new WaitForSeconds(0.10f);
                Lifter_needToTrigger = true;
            }
        #endregion
        
        #region TireChanger

        private static bool TC_needToTrigger = true;
            
        [HarmonyPatch(typeof(TireChangerLogic), "SetGroupOnTireChanger")]
        [HarmonyPostfix]
        public static void TireChangerFix(GroupItem groupItem, bool instant, bool connect, TireChangerLogic __instance)
        {
            if (groupItem.ItemList.Count == 0) return;

            if (TC_needToTrigger)
            {
                MelonLogger.Msg($"Tire Changer Triggered! : {instant} , {connect}");
                ClientSend.SendTireChange(new ModGroupItem(groupItem), instant,connect);
                
            }
        }
        
        public static IEnumerator TC_PauseUpdating()
        {
            TC_needToTrigger = false;
            yield return new WaitForSeconds(0.10f);
            TC_needToTrigger = true;
        }

        [HarmonyPatch(typeof(PieMenuController), "_GetOnClick_b__72_61")]
        [HarmonyPostfix]
        public static void TireRemoveActionFix(TireChangerLogic __instance)
        {
                MelonLogger.Msg($"Tire On Changer removed!");
                ClientSend.SendTireChange_ResetAction();
        }
        
        
        #endregion

    }
}