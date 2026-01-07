using System.Collections;
using CMS21Together.ClientSide.Data.Handle;
using HarmonyLib;
using Il2Cpp;
using Il2CppCMS.UI.Windows;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class WheelBalancerLogic
{
    public static bool listen = true;


    [HarmonyPatch(typeof(Il2Cpp.WheelBalancerLogic), nameof(Il2Cpp.WheelBalancerLogic.SetGroupOnWheelBalancer))]
    [HarmonyPrefix]
    public static void SetGroupOnWheelBalancerHook(GroupItem groupItem, bool instant, Il2Cpp.WheelBalancerLogic __instance)
    {
        if(!Client.Instance.isConnected || !listen) {listen = true; return;}
        if(groupItem == null || groupItem.ItemList.Count == 0) return;

        ClientSend.SetWheelBalancerPacket(groupItem);
    }
    
    [HarmonyPatch(typeof(WheelBalanceWindow), nameof(WheelBalanceWindow.StartMiniGame))]
    [HarmonyPrefix]
    public static bool StartMiniGameHook(WheelBalanceWindow __instance)
    {
        if(!Client.Instance.isConnected || !listen) {listen = true; return true;}
                
        MelonCoroutines.Start(BalanceWheel(__instance));
        __instance.CancelAction();
        return false;
    }

    private static IEnumerator BalanceWheel(WheelBalanceWindow instance)
    {
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
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            GameData.Instance.wheelBalancer.balanceCanceled = false;
        }

        ClientSend.WheelBalancePacket(GameData.Instance.wheelBalancer.groupOnWheelBalancer);
    }
    
    [HarmonyPatch(typeof(PieMenuController), "_GetOnClick_b__72_64")]
    [HarmonyPrefix]
    public static void TireRemoveActionHook()
    {
        if(!Client.Instance.isConnected || !listen) {listen = true; return;}
        
        ClientSend.WheelRemovePacket();
    }
}