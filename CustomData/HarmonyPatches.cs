using System.Collections;
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
        [HarmonyPatch(typeof(CarLoader), "LoadCar")]
        [HarmonyPostfix]
        public static void LoadCarPatch(string name, CarLoader __instance)
        {
            MelonLogger.Msg("A car is being Loaded! : " + name);

            var loaderNumber = __instance.gameObject.name[10].ToString();

            if (loaderNumber == "1")
            {
                ModCar newCar0 = new ModCar(0, __instance.ConfigVersion, SceneManager.GetActiveScene().name,
                    __instance.placeNo);
                if (!ClientData.carOnScene.Any(s => s.carLoaderID == 0))
                {
                    ClientData.carOnScene.Add(newCar0);
                    ClientSend.SendCarInfo(new ModCar(ClientData.carOnScene[0]));
                }
            }
            else if (loaderNumber == "2")
            {
                ModCar newCar1 = new ModCar(1, __instance.ConfigVersion, SceneManager.GetActiveScene().name,
                    __instance.placeNo);
                if (!ClientData.carOnScene.Any(s => s.carLoaderID == 1))
                {
                    ClientData.carOnScene.Add(newCar1);
                    ClientSend.SendCarInfo(new ModCar(ClientData.carOnScene[1]));
                }
            }
            else if (loaderNumber == "3")
            {
                ModCar newCar2 = new ModCar(2, __instance.ConfigVersion, SceneManager.GetActiveScene().name,
                    __instance.placeNo);
                if (!ClientData.carOnScene.Any(s => s.carLoaderID == 2))
                {
                    ClientData.carOnScene.Add(newCar2);
                    ClientSend.SendCarInfo(new ModCar(ClientData.carOnScene[2]));
                }
            }
            else if (loaderNumber == "4")
            {
                ModCar newCar3 = new ModCar(3, __instance.ConfigVersion, SceneManager.GetActiveScene().name,
                    __instance.placeNo);
                if (!ClientData.carOnScene.Any(s => s.carLoaderID == 3))
                {
                    ClientData.carOnScene.Add(newCar3);
                    ClientSend.SendCarInfo(new ModCar(ClientData.carOnScene[3]));
                }
            }
            else if (loaderNumber == "5")
            {
                ModCar newCar4 = new ModCar(4, __instance.ConfigVersion, SceneManager.GetActiveScene().name,
                    __instance.placeNo);
                if (!ClientData.carOnScene.Any(s => s.carLoaderID == 4))
                {
                    ClientData.carOnScene.Add(newCar4);
                    ClientSend.SendCarInfo(new ModCar(ClientData.carOnScene[4]));
                }
            }
            
        }

        [HarmonyPatch(typeof(CarLoader), "SetEngine")]
        [HarmonyPostfix]
        public static void SetEnginePatch(CarLoader __instance)
        {
            MelonCoroutines.Start(WaitForEndOfSetEngine(__instance));
            MelonLogger.Msg("A car as Loaded! : " + __instance.carToLoad);
        }

        public static IEnumerator WaitForEndOfSetEngine(CarLoader __instance) //TODO: Handle case where car is removed from scene
        {
            yield return new WaitForEndOfFrame();
            
            while (!__instance.done || !__instance.modelLoaded)
                yield return new WaitForEndOfFrame();
            
            yield return new WaitForEndOfFrame();
            
            var loaderNumber = __instance.gameObject.name[10].ToString();

            switch (loaderNumber)
            {
                case "1":
                    ClientData.carOnScene[0].isCarLoaded = true;
                    MelonCoroutines.Start(StartGetReferencesCoroutine(0));
                    break;
                case "2":
                    ClientData.carOnScene[1].isCarLoaded = true;
                    MelonCoroutines.Start(StartGetReferencesCoroutine(1));
                    break;
                case "3":
                    ClientData.carOnScene[2].isCarLoaded = true;
                    MelonCoroutines.Start(StartGetReferencesCoroutine(2));
                    break;
                case "4":
                    ClientData.carOnScene[3].isCarLoaded = true;
                    MelonCoroutines.Start(StartGetReferencesCoroutine(3));
                    break;
                case "5":
                    ClientData.carOnScene[4].isCarLoaded = true;
                    MelonCoroutines.Start(StartGetReferencesCoroutine(4));
                    break;
            }
        }
        
        public static IEnumerator StartGetReferencesCoroutine(int carLoaderID)
        {
            var ReferenceCouroutine = MelonCoroutines.Start(Car.GetPartsReferencesCoroutine(carLoaderID));
            yield return ReferenceCouroutine;
        }
    }
}