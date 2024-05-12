using System;
using System.Collections;
using CMS21Together.ClientSide.Handle;
using CMS21Together.Shared.Data;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.GarageInteraction
{
    [HarmonyPatch]
    public static class ModTireChangerLogic
    {
        public static bool listenToTC = true;
        
        [HarmonyPatch(typeof(TireChangerLogic), "SetGroupOnTireChanger", new Type[]{ typeof(GroupItem), typeof(bool), typeof(bool)})]
        [HarmonyPostfix]
        public static void TireChangerFix(GroupItem groupItem, bool instant, bool connect, TireChangerLogic __instance)
        {
            if(!Client.Instance.isConnected) return;
            
            if (groupItem.ItemList.Count == 0) return;

            if (listenToTC)
            {
                MelonLogger.Msg($"Tire Changer Triggered! : {instant} , {connect}");
                ClientSend.TireChanger(new ModGroupItem(groupItem), instant, connect);

            }
        }

        [HarmonyPatch(typeof(PieMenuController), "_GetOnClick_b__72_61")]
        [HarmonyPostfix]
        public static void TireRemoveActionFix(TireChangerLogic __instance)
        {
            MelonLogger.Msg($"Tire On Changer removed!");
            ClientSend.TireChanger(new ModGroupItem(), false, false, true);
        }
    }
}