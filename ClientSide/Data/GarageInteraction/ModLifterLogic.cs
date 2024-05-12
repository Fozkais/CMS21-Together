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

            int validAction = 0;

            if (actionType == 0) validAction = 1;
            if (actionType == 1) validAction = -1;

            int postLiftValue = (int)__instance.currentState + validAction;
            int carLoaderID = __instance.connectedCarLoader.gameObject.name[10] - '0';
            ModLifterState state = GetStateFromValue(postLiftValue);
            MelonLogger.Msg("ActionType: " + validAction);
            MelonLogger.Msg("New Lifter state: " + state + " Value: " + postLiftValue);
            ClientSend.SendLifter(state, carLoaderID);
            MelonLogger.Msg("Sending lifter info!");
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