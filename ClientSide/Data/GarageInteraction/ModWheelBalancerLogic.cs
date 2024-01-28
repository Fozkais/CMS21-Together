using System.Collections;
using CMS21Together.ClientSide.Handle;
using CMS21Together.Shared.Data;
using HarmonyLib;
using Il2Cpp;
using Il2CppCMS.UI.Windows;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.GarageInteraction
{
    public static class ModWheelBalancerLogic
    {
         private static bool listentoWB = true;
        
            [HarmonyPatch(typeof(WheelBalancerLogic), "SetGroupOnWheelBalancer")]
            [HarmonyPrefix]
            public static void WheelBalancerFix(GroupItem groupItem, bool instant, WheelBalancerLogic __instance)
            {
                if (groupItem.ItemList.Count == 0) return;

                if (listentoWB)
                {
                   // MelonLogger.Msg($"Wheel Balance Triggered!");
                    instant = true;
                    ClientSend.WheelBalancer(ModWheelBalancerActionType.setGroup,new ModGroupItem(groupItem));

                }
            }
            
            [HarmonyPatch(typeof(WheelBalanceWindow), "StartMiniGame")]
            [HarmonyPrefix]
            public static void WheelBalancer2Fix(WheelBalanceWindow __instance)
            {
                    MelonCoroutines.Start(BalanceWheel(__instance));
            }
            public static IEnumerator BalanceWheel(WheelBalanceWindow __instance)
            {
                
                yield return new WaitForFixedUpdate();
                yield return new WaitForEndOfFrame();
                yield return new WaitForSeconds(0.1f);
                foreach (Item item in GameData.Instance.wheelBalancer.groupOnWheelBalancer.ItemList)
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
                    GameData.Instance.wheelBalancer.balanceCanceled = false;
                }

                ClientSend.WheelBalancer(ModWheelBalancerActionType.start, new ModGroupItem(GameData.Instance.wheelBalancer.groupOnWheelBalancer));
            }
            public static IEnumerator ResetWBListen()
            {
                listentoWB = false;
                yield return new WaitForSeconds(0.10f);
                listentoWB = true;
            }
            
            [HarmonyPatch(typeof(PieMenuController), "_GetOnClick_b__72_64")]
            [HarmonyPostfix]
            public static void WB_TireRemoveActionFix(TireChangerLogic __instance)
            {
                //MelonLogger.Msg($"Tire On Wheel Balancer removed!");
                ClientSend.WheelBalancer(ModWheelBalancerActionType.remove, null);
            }
    }
}