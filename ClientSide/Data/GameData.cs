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
        public static bool DataInitialzed;

        public static IEnumerator InitializeGameData() // TODO: reset data when changing scene
        {
            localPlayer = Object.FindObjectOfType<FPSInputController>().gameObject;
            
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