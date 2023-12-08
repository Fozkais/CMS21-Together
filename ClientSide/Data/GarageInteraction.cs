using System.Collections;
using System.Collections.Generic;
using CMS21Together.ClientSide.DataHandle;
using CMS21Together.CustomData;
using CMS21Together.SharedData;
using HarmonyLib;
using Il2Cpp;
using Il2CppCMS.UI.Windows;
using Il2CppSystem;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data
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
        public static void TC_TireRemoveActionFix(TireChangerLogic __instance)
        {
                MelonLogger.Msg($"Tire On Changer removed!");
                ClientSend.SendTireChange_ResetAction();
        }
        
        
        #endregion

        #region WheelBalancer

            private static bool WB_needToTrigger = true;
        
            [HarmonyPatch(typeof(WheelBalancerLogic), "SetGroupOnWheelBalancer")]
            [HarmonyPrefix]
            public static void WheelBalancerFix(GroupItem groupItem, bool instant, WheelBalancerLogic __instance)
            {
                if (groupItem.ItemList.Count == 0) return;

                if (WB_needToTrigger)
                {
                    MelonLogger.Msg($"Wheel Balance Triggered!");
                    instant = true;
                    ClientSend.SendWheelBalance(new ModGroupItem(groupItem));
                    
                }
            }
            
            [HarmonyPatch(typeof(WheelBalanceWindow), "StartMiniGame")]
            [HarmonyPrefix]
            public static void WheelBalancer2Fix(WheelBalanceWindow __instance)
            {
                MelonCoroutines.Start(WB_BalanceWheel(__instance));
            }
            public static IEnumerator WB_BalanceWheel(WheelBalanceWindow __instance)
            {
                yield return new WaitForFixedUpdate();
                yield return new WaitForEndOfFrame();
                yield return new WaitForSeconds(0.1f);
                foreach (Item item in GameData.wheelBalancer.groupOnWheelBalancer.ItemList)
                {
                    item.WheelData = new WheelData()
                    {
                        ET = item.WheelData.ET,
                        Profile = item.WheelData.Profile,
                        Width = item.WheelData.Width,
                        Size = item.WheelData.Size,
                        IsBalanced = true
                    };
                    __instance.CancelAction();
                    yield return new WaitForFixedUpdate();
                    yield return new WaitForEndOfFrame();
                    yield return new WaitForSeconds(0.1f);
                    GameData.wheelBalancer.balanceCanceled = false;
                }

                ClientSend.SendUpdatedWheelFromBalancer(new ModGroupItem(GameData.wheelBalancer.groupOnWheelBalancer));
            }
            public static IEnumerator WB_PauseUpdating()
            {
                WB_needToTrigger = false;
                yield return new WaitForSeconds(0.10f);
                WB_needToTrigger = true;
            }
            
            [HarmonyPatch(typeof(PieMenuController), "_GetOnClick_b__72_64")]
            [HarmonyPostfix]
            public static void WB_TireRemoveActionFix(TireChangerLogic __instance)
            {
                MelonLogger.Msg($"Tire On Wheel Balancer removed!");
                ClientSend.SendWheelBalance_ResetAction();
            }

        #endregion

        #region CarWash

        public static bool WashLogic_NeedToTrigeer;
        
        [HarmonyPatch(typeof(CarWashLogic), "__n__0")]
        [HarmonyPrefix]
        public static void CarWashFix(CarLoader carLoader,CarWashLogic __instance)
        {
            if (WashLogic_NeedToTrigeer)
            {
                ClientSend.SendCarWash(carLoader.gameObject.name[10]);
                WashLogic_NeedToTrigeer = true;
            }
        }
        

        #endregion

    }
}