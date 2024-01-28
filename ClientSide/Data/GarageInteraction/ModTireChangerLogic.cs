using System.Collections;
using CMS21Together.ClientSide.Handle;
using CMS21Together.Shared.Data;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.GarageInteraction
{
    public static class ModTireChangerLogic
    {
        private static bool listenToTC = true;
        
        [HarmonyPatch(typeof(ModTireChangerLogic), "SetGroupOnTireChanger")]
        [HarmonyPostfix]
        public static void TireChangerFix(GroupItem groupItem, bool instant, bool connect, TireChangerLogic __instance)
        {
            if (groupItem.ItemList.Count == 0) return;

            if (listenToTC)
            {
                MelonLogger.Msg($"Tire Changer Triggered! : {instant} , {connect}");
                ClientSend.TireChanger(new ModGroupItem(groupItem), instant, connect);

            }
        }
        public static IEnumerator ResetTCListen()
        {
            listenToTC = false;
            yield return new WaitForSeconds(0.10f);
            listenToTC = true;
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