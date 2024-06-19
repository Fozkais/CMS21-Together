using System;
using System.Collections;
using System.IO;
using System.Linq;
using CMS21Together.ClientSide.Handle;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using HarmonyLib;
using Il2Cpp;
using Il2CppCMS.Helpers;
using MelonLoader;
using UnityEngine;
using BinaryWriter = Il2CppSystem.IO.BinaryWriter;
using Object = UnityEngine.Object;

namespace CMS21Together.ClientSide.Data.Car
{
    [HarmonyPatch]
    public static class CarHarmonyHooks
    {
        public static bool ListenToCursorBlock;
        public static bool ListenToDeleteCar = true;
        private static bool screenfadeFix;
        

        [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.LoadCar))]
        [HarmonyPrefix]
        public static void LoadCarPrePatch(string name, CarLoader __instance)
        {
            if (Client.Instance.isConnected)
            {
                // MelonLogger.Msg($"(CarLoader.LoadCar) {name} is being Loaded. ");
                if (ModSceneManager.currentScene() == GameScene.garage)
                {
                    string carLoaderID = __instance.gameObject.name[10].ToString();
                    MelonLogger.Msg($"A car is being loaded! : {name}, ID:{carLoaderID}");
            
                    int convertedLoaderID = CarInitialization.ConvertCarLoaderID(carLoaderID);
                    if (ClientData.Instance.tempCarList.Contains((convertedLoaderID, name)))
                    {
                        MelonLogger.Msg($"Skip {__instance.carToLoad} sync.");
                        return;
                    }
                    MelonCoroutines.Start(CarInitialization.InitializePrePatch(__instance, name, convertedLoaderID));
                    
                }
            }
            
        }
        
        [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.DeleteCar), new Type[] { })]
        [HarmonyPrefix]
        public static void DeleteCarPatch(CarLoader __instance)
        {
            if (Client.Instance.isConnected)
            {
                // MelonLogger.Msg($"(CarLoader.LoadCar) {name} is being Loaded. ");
                if (ModSceneManager.currentScene() == GameScene.garage)
                {
                    string carLoaderID = __instance.gameObject.name[10].ToString();
                    int convertedLoaderID = CarInitialization.ConvertCarLoaderID(carLoaderID);

                    if (ListenToDeleteCar)
                    {
                        if (ClientData.Instance.LoadedCars.Any(s => s.Value.carLoaderID == convertedLoaderID))
                        {
                            var car = ClientData.Instance.LoadedCars
                                .First(s => s.Value.carLoaderID == convertedLoaderID).Value;
                            ClientSend.SendModCar(new ModCar(car), true);
                            ClientData.Instance.LoadedCars.Remove(car.carLoaderID);
                        }
                    }
                    
                }
            }
            
        }

        
        
        [HarmonyPatch(typeof(CarLoader), "PreparePartScriptCuller")]
        [HarmonyPostfix]
        public static void PreparePartScriptCullerPatch(CarLoader __instance)
        {
            if (Client.Instance.isConnected)
            {
                if (ModSceneManager.currentScene() == GameScene.garage)
                {
                    string carLoaderID = __instance.gameObject.name[10].ToString();
                    int convertedLoaderID = CarInitialization.ConvertCarLoaderID(carLoaderID);
                    
                    if (ClientData.Instance.tempCarList.Contains((convertedLoaderID, __instance.carToLoad)))
                    {
                        ListenToDeleteCar = false;
                        MelonLogger.Msg($"Deleting {__instance.carToLoad}...");
                        ClientData.Instance.tempCarList.Remove((convertedLoaderID,  __instance.carToLoad));
                        __instance.DeleteCar();
                        ListenToDeleteCar = true;
                        return;
                    }
                    MelonCoroutines.Start(CarInitialization.CarIsLoaded(__instance));
                }
                //MelonLogger.Msg($"(CarLoader.PreparePartScriptCuller) {__instance.carToLoad} should be loaded. ");
            }
        }
        
        [HarmonyPatch(typeof(Cursor3D), "BlockCursor")]
        [HarmonyPrefix]
        public static void BlockCursorPatch(bool block, Cursor3D __instance)
        {
            if (Client.Instance.isConnected)
            {
                if(block)
                    if(ListenToCursorBlock)
                        MelonCoroutines.Start(FixCursorLock());
            }
        }

        private static IEnumerator FixCursorLock()
        {
            yield return new WaitForSeconds(2.5f);
            if (Cursor3D.Get().isCursorBlocked)
            {
                ListenToCursorBlock = false;
                Cursor3D.Get().BlockCursor(false);
                ListenToCursorBlock = true;
            }
        }
        
        [HarmonyPatch(typeof(NotificationCenter), "SelectSceneToLoad", 
            new Type[]{ typeof(string), typeof(SceneType), typeof(bool), typeof(bool)})]
        [HarmonyPrefix]
        public  static void SceneChangePatch( string newSceneName, SceneType sceneType, bool useFader, bool saveGame)
        {
            if (Client.Instance.isConnected || ServerData.isRunning)
            {
                if (ClientData.Instance.GameReady)
                {
                    CarHarmonyHooks.ListenToDeleteCar = false;
                    try
                    {
                        if (newSceneName != "garage")
                        {
                            ClientData.Instance.tempCarList.Clear();
                            var profile = SavesManager.currentSave;
                            for (var i = 0; i < profile.carsInGarage.Count; i++)
                            {
                                var SaveCar = profile.carsInGarage[i];
                                if (!String.IsNullOrEmpty(SaveCar.carToLoad))
                                {
                                    ClientData.Instance.tempCarList.Add((i, SaveCar.carToLoad));
                                }
                            }
                            ClientSend.SendResyncCars(ClientData.Instance.tempCarList);
                        }
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Msg("Error on sceneChange: " + e);
                    }
                }
            }
        }
        
        /*[HarmonyPatch(typeof(BodyPartData), "Serialize")]
        [HarmonyPrefix]
        public static bool PatchBodyPartDataSerialize(BinaryWriter binaryWriter, BodyPartData __instance)
        {
            var tintcolor = __instance.TintColor;
            var color = __instance.Color;
            var paintdata = __instance.PaintData;
            var empty = new FloatArrayWrapper();
            
            binaryWriter.Write(__instance.Id ?? string.Empty);
            binaryWriter.Write(__instance.Switched);
            binaryWriter.WriteAsUint(__instance.Condition);
            binaryWriter.Write(__instance.Unmounted);
            binaryWriter.Write(__instance.TunedID ?? string.Empty);
            binaryWriter.Write(__instance.IsTinted);
            if (__instance.IsTinted)
            {
                if(tintcolor != null)
                    SerializationHelper.WriteFloatArrayWrapper(binaryWriter, ref tintcolor);
                else
                    SerializationHelper.WriteFloatArrayWrapper(binaryWriter, ref empty);
            }
            if(color != null)
                SerializationHelper.WriteFloatArrayWrapper(binaryWriter, ref color);
            else
                SerializationHelper.WriteFloatArrayWrapper(binaryWriter, ref empty);
            
            binaryWriter.Write((byte)__instance.PaintType);
            if ((byte)__instance.PaintType == 6)
            {
                SerializationHelper.WritePaintData(binaryWriter, ref paintdata);
            }
            if(__instance.Livery != null)
                binaryWriter.Write(__instance.Livery);
            else
                binaryWriter.Write(string.Empty);
            
            binaryWriter.WriteAsUint(__instance.LiveryStrength);
            binaryWriter.Write(__instance.OutsaidRustEnabled);
            binaryWriter.WriteAsUint(__instance.Dent);
            binaryWriter.Write((byte)__instance.Quality);
            binaryWriter.WriteAsUint(__instance.Dust);
            binaryWriter.WriteAsUint(__instance.WashFactor);

            return false;
        }*/
    }
}