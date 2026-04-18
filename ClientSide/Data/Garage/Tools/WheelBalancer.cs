using System.Collections;
using CMS.UI.Windows;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data.Vanilla.GarageTool;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class WheelBalancer
{
	public static bool listen = true;
	private static bool balanceWatchRunning;
	
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
        [HarmonyPostfix]
        public static void WheelBalancer2Fix(WheelBalanceWindow __instance)
        {
            if(!Client.Instance.isConnected) return;
            
            MelonCoroutines.Start(WatchBalanceResult());
        }

        private static IEnumerator WatchBalanceResult()
        {
            if (balanceWatchRunning) yield break;

            balanceWatchRunning = true;
            yield return new WaitForEndOfFrame();

            GroupItem watchedGroup = GameData.Instance?.wheelBalancer?.groupOnWheelBalancer;
            if (watchedGroup == null)
            {
                balanceWatchRunning = false;
                yield break;
            }

            long watchedUid = watchedGroup.UID;
            float timeoutAt = Time.time + 120f;
            while (Client.Instance.isConnected && Time.time < timeoutAt)
            {
                GroupItem currentGroup = GameData.Instance?.wheelBalancer?.groupOnWheelBalancer;
                if (currentGroup == null || currentGroup.UID != watchedUid)
                    break;

                if (IsBalanced(currentGroup))
                {
                    ClientSend.SendWheelBalancer((int)ModWheelBalancerActionType.start, currentGroup);
                    break;
                }

                yield return new WaitForSeconds(0.25f);
            }

            balanceWatchRunning = false;
        }

        private static bool IsBalanced(GroupItem groupItem)
        {
            if (groupItem?.ItemList == null) return false;

            foreach (Item item in groupItem.ItemList)
            {
                if (item != null && item.WheelData.IsBalanced)
                    return true;
            }

            return false;
        }
        
        [HarmonyPatch(typeof(PieMenuController), "_GetOnClick_b__72_64")]
        [HarmonyPostfix]
        public static void WB_TireRemoveActionFix()
        {
            if(!Client.Instance.isConnected) return;
            
            ClientSend.SendWheelBalancer(2);
        }
}
