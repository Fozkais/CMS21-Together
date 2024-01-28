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
    public static class ModLifterLogic
    {
        public static bool listenToLifter = true;

        [HarmonyPatch(typeof(CarLifter), "Action")]
        [HarmonyPostfix]
        public static void LifterFix(int actionType, CarLifter __instance)
        {
            if(!listenToLifter) return;

            int postLiftValue = (int)__instance.currentState + actionType;
            int carLoaderID = __instance.connectedCarLoader.gameObject.name[10] - '0';
            ModLifterState state = GetStateFromValue(postLiftValue);
            ClientSend.SendLifter(state, carLoaderID);
            MelonLogger.Msg("Sending lifter info!");
        }

        public static IEnumerator ResetLifterListen()
        {
            listenToLifter = false;
            yield return new WaitForSeconds(0.5f);
            listenToLifter = true;
        }


        public static ModLifterState GetStateFromValue(int value)
        {
            ModLifterState state = ModLifterState.ground;
            switch (value)
            {
                case 0:
                    state = ModLifterState.ground;
                    break;
                case 1:
                    state = ModLifterState.low;
                    break;
                case 2:
                    state = ModLifterState.high;
                    break;
            }

            return state;
        }
    }
}