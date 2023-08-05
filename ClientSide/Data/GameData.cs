using System.Collections.Generic;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide.Data
{
    public static class GameData
    {
        public static CarLoader[] carLoaders;
        public static GameObject localPlayer;
        public static bool DataInitialzed { get; private set; }

        public static void InitializeGameData() // TODO: reset data when changing scene
        {
            carLoaders =  GameScript.Get().carOnScene;
            localPlayer = GameObject.FindObjectOfType<FPSInputController>().gameObject;
            
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