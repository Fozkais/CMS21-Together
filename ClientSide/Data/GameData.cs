using System.Collections;
using System.Collections.Generic;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide.Data
{
    public static class GameData
    {
        public static GameObject localPlayer;
        public static TireChangerLogic tireChanger;
        public static CarLoader[] carLoaders;
        public static Inventory localInventory;
        public static bool DataInitialzed;

        public static IEnumerator InitializeGameData() // TODO: reset data when changing scene
        {
            localPlayer = Object.FindObjectOfType<FPSInputController>().gameObject;
            tireChanger = Object.FindObjectOfType<TireChangerLogic>();
            
            carLoaders = new[] // TODO: Reorganise order to match other scene loader organisation
            {
                GameScript.Get().carOnScene[0],
                GameScript.Get().carOnScene[3],
                GameScript.Get().carOnScene[4],
                GameScript.Get().carOnScene[1],
                GameScript.Get().carOnScene[2]
            };
            if (SceneChecker.isInGarage())
            {
                localInventory = GameScript.Get().GetComponent<Inventory>();
            }
            
            yield return new WaitForSeconds(3);
            MelonLogger.Msg("Initialized Game Data Successfully!");
            DataInitialzed = true;
        }
    }

    public enum SceneNames
    {
        garage,
        Menu,
        Junkyard,
        Auto_salon
        
    }
}