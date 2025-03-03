using System.Collections;
using CMS.UI.Windows;
using CMS21Together.ClientSide.Data.Handle;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class WheelBalancer
{
	public static bool listen = true;
	
        [HarmonyPrefix]
	    [HarmonyPatch(typeof(WheelBalancerLogic), "SetGroupOnWheelBalancer")]
        public static void WheelBalancerFix(GroupItem groupItem, bool instant, WheelBalancerLogic __instance)
        {
            if(!Client.Instance.isConnected) return;
            if(!listen) { listen = true; return;}
            
            if (groupItem != null && groupItem.ItemList.Count != 0)
            {
                ClientSend.SendWheelBalancer(0, groupItem);
            }
        }
        
        [HarmonyPatch(typeof(WheelBalanceWindow), nameof(WheelBalanceWindow.StartMiniGame))]
        [HarmonyPrefix]
        public static bool WheelBalancer2Fix(WheelBalanceWindow __instance)
        {
            if(!Client.Instance.isConnected) return true;
            
            MelonCoroutines.Start(BalanceWheel(__instance));
            return false;
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
                GameMode.m_instance.SetCurrentMode(gameMode.Garage);
            }
            ClientSend.SendWheelBalancer(1, GameData.Instance.wheelBalancer.groupOnWheelBalancer);
        }
        
        [HarmonyPatch(typeof(PieMenuController), "_GetOnClick_b__72_64")]
        [HarmonyPostfix]
        public static void WB_TireRemoveActionFix()
        {
            if(!Client.Instance.isConnected) return;
            
            ClientSend.SendWheelBalancer(2);
        }
}