using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CMS21MP.ClientSide.Data;
using CMS21MP.ClientSide.DataHandle;
using CMS21MP.SharedData;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CMS21MP.CustomData
{
    [HarmonyPatch]
    public class HarmonyPatches
    {

        private static bool ListenToFade;
        public static bool ListenToCursorBlock;
        private static CarLoader LoaderToListen;
        
        [HarmonyPatch(typeof(CarLoader), "LoadCar")]
        [HarmonyPostfix]
        public static void LoadCarPatch(string name, CarLoader __instance)
        { 
            
            MelonCoroutines.Start(LoadCarCouroutine(__instance, name));
        }
        
        [HarmonyPatch(typeof(Cursor3D), "BlockCursor")]
        [HarmonyPrefix]
        public static bool EnableGamepadMountObjectPatch(bool block, Cursor3D __instance)
        {
            if (ListenToCursorBlock)
            {
                MelonLogger.Msg("BlockCursor Bypass!");
                return false;
            }
           // MelonLogger.Msg("BlockCursor not Bypass!");
            return true;
        }
        
            
        [HarmonyPatch(typeof(CarLoader), "LoadCarFromFile", new Type[]{ typeof(NewCarData)})]
        [HarmonyPostfix]
        public static void LoadCarFromFilePatch(NewCarData carDataCheck, CarLoader __instance)
        {
            
            MelonCoroutines.Start(LoadCarCouroutine(__instance, carDataCheck.carToLoad));
        }

        private static IEnumerator LoadCarCouroutine(CarLoader __instance, string name)
        {
            if (String.IsNullOrEmpty(name)) 
                yield break;
            
            LoaderToListen = __instance;
            ListenToFade = true;
            
            while(GameData.DataInitialzed == false) // DO NOT REMOVE!
                yield return new WaitForSeconds(1);
            
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(1);
            
             var loaderNumber = __instance.gameObject.name[10].ToString();
             
             MelonLogger.Msg("A car is being Loaded! : " + name + ", "  + loaderNumber);

             yield return new WaitForEndOfFrame();
             

            if (loaderNumber == "1")
            {
                ModCar newCar0 = new ModCar(0, __instance.ConfigVersion, SceneChecker.wichScene(SceneManager.GetActiveScene().name), __instance.placeNo);
                if(!ClientData.carOnScene.Any(s => s.Value.carLoaderID == 0))
                {
                    MelonLogger.Msg("Pass 1");
                    ClientData.carOnScene.Add(0,newCar0);
                    ClientSend.SendCarInfo(new ModCar(newCar0));
                }
            }
            else if (loaderNumber == "2")
            {
                ModCar newCar1 = new ModCar(1, __instance.ConfigVersion, SceneChecker.wichScene(SceneManager.GetActiveScene().name),
                    __instance.placeNo);
                if (!ClientData.carOnScene.Any(s => s.Value.carLoaderID == 1))
                {
                    MelonLogger.Msg("Pass 2");
                    ClientData.carOnScene.Add(1, newCar1);
                    ClientSend.SendCarInfo(new ModCar(newCar1));
                    
                }
            }
            else if (loaderNumber == "3")
            {
                ModCar newCar2 = new ModCar(2, __instance.ConfigVersion, SceneChecker.wichScene(SceneManager.GetActiveScene().name),
                    __instance.placeNo);
                if (!ClientData.carOnScene.Any(s => s.Value.carLoaderID == 2))
                {
                    MelonLogger.Msg("Pass 3");
                    ClientData.carOnScene.Add(2, newCar2);
                    ClientSend.SendCarInfo(new ModCar(newCar2));
                }
            }
            else if (loaderNumber == "4")
            {
                ModCar newCar3 = new ModCar(3, __instance.ConfigVersion, SceneChecker.wichScene(SceneManager.GetActiveScene().name),
                    __instance.placeNo);
                if (!ClientData.carOnScene.Any(s => s.Value.carLoaderID == 3))
                {
                    MelonLogger.Msg("Pass 4");
                    ClientData.carOnScene.Add(3, newCar3);
                    ClientSend.SendCarInfo(new ModCar(newCar3));
                }
            }
            else if (loaderNumber == "5")
            {
                ModCar newCar4 = new ModCar(4, __instance.ConfigVersion, SceneChecker.wichScene(SceneManager.GetActiveScene().name),
                    __instance.placeNo);
                if (!ClientData.carOnScene.Any(s => s.Value.carLoaderID == 4))
                {
                    MelonLogger.Msg("Pass 5");
                    ClientData.carOnScene.Add(4, newCar4);
                    ClientSend.SendCarInfo(new ModCar(newCar4));
                }
            }

        }
        

        [HarmonyPatch(typeof(ScreenFader), "NormalFadeOut")]
        [HarmonyPostfix]
        public static void NormalFadeOutPatch()
        {
            MelonLogger.Msg("FadeOut Called!");
            if (ListenToFade)
            {
                MelonCoroutines.Start(HandleLoadedCar());
            }
        }
        
        [HarmonyPatch(typeof(CarLoader), "SetEngine")]
        [HarmonyPostfix]
        public static void SetEnginePatch(CarLoader __instance)
        {
            MelonCoroutines.Start(HandleLoadedCar(__instance));
        }

        private static IEnumerator HandleLoadedCar(CarLoader _loader=null)
        {
            
            ListenToFade = false;
            
            while(GameData.DataInitialzed == false) // DO NOT REMOVE!
                yield return new WaitForSeconds(1.1f);
            
            
            yield return new WaitForSeconds(1);
            yield return new WaitForEndOfFrame();
            string loaderNumber;
            //int validIndex;

            if (_loader == null)
            {
                loaderNumber = LoaderToListen.gameObject.name[10].ToString();
               // validIndex = ClientData.carOnScene.FindIndex(s => s.carID == LoaderToListen.carToLoad && s.carVersion == LoaderToListen.ConfigVersion); 
                MelonLogger.Msg("A car as been Loaded! : " + LoaderToListen.carToLoad + ", " + loaderNumber);
            }
            else
            {
                loaderNumber = _loader.gameObject.name[10].ToString();
                //validIndex = ClientData.carOnScene.FindIndex(s => s.carID == _loader.carToLoad && s.carVersion == _loader.ConfigVersion); 
                MelonLogger.Msg("A car as been Loaded ! : " + _loader.carToLoad + ", " + loaderNumber);
            }
            
           // ClientData.carOnScene[validIndex].isCarLoaded = true;
            // MelonCoroutines.Start(Car.GetPartsReferencesCoroutine(0));
            
            switch (loaderNumber)
            {
                case "1":
                    ClientData.carOnScene[0].isCarLoaded = true;
                    MelonCoroutines.Start(Car.GetPartsReferencesCoroutine(0));
                    break;
                case "2":
                    ClientData.carOnScene[1].isCarLoaded = true;
                    MelonCoroutines.Start(Car.GetPartsReferencesCoroutine(1));
                    break;
                case "3":
                    ClientData.carOnScene[2].isCarLoaded = true;
                    MelonCoroutines.Start(Car.GetPartsReferencesCoroutine(2));
                    break;
                case "4":
                    ClientData.carOnScene[3].isCarLoaded = true;
                    MelonCoroutines.Start(Car.GetPartsReferencesCoroutine(3));
                    break;
                case "5":
                    ClientData.carOnScene[4].isCarLoaded = true;
                    MelonCoroutines.Start(Car.GetPartsReferencesCoroutine(4));
                    break;
            }

            LoaderToListen = null;
        }

        public static IEnumerator ResetCursorBlockCoroutine()
        {
            ListenToCursorBlock = true;
            yield return new WaitForSeconds(0.1f);
            ListenToCursorBlock = false;
        }
    }
}