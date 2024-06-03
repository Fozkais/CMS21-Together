using System.Linq;
using CMS21Together.ClientSide.Data.Car;
using CMS21Together.ClientSide.Handle;
using CMS21Together.Shared.Data;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace CMS21Together.ClientSide.Data.GarageInteraction
{
    [HarmonyPatch]
    public static class ModEngineCrane
    {

        public static bool listentoCraneAction = true;
        
        [HarmonyPatch(typeof(NotificationCenter), nameof(NotificationCenter.InsertEngineToCar))]
        [HarmonyPrefix]
        public static void InsertEngineIntoCarPatch(GroupItem engine)
        {
            if(!Client.Instance.isConnected) return;

            if(listentoCraneAction)
                ClientSend.EngineCraneHandle(-1,new ModGroupItem(engine));
        }
        
        [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.UseEngineCrane))]
        [HarmonyPrefix]
        public static void RemoveEngineFromCarPatch(CarLoader __instance)
        {
            if(!Client.Instance.isConnected) return;

            if (listentoCraneAction)
            {
                int carLoaderID = CarInitialization.ConvertCarLoaderID(__instance.gameObject.gameObject.name[10].ToString());
                ClientSend.EngineCraneHandle(carLoaderID);
            }
        }


    }
}