using CMS21Together.ClientSide.Data.Car;
using CMS21Together.ClientSide.Handle;
using HarmonyLib;
using Il2Cpp;

namespace CMS21Together.ClientSide.Data.GarageInteraction
{
    [HarmonyPatch]
    public static class ModOilBin
    {
        public static bool listenToOilBinAction = true;
        
        [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.UseOilbin))]
        [HarmonyPrefix]
        public static void UseOilBinPatch(CarLoader __instance)
        {
            if(!Client.Instance.isConnected) return;

            if (listenToOilBinAction)
            {
                int carLoaderID = CarInitialization.ConvertCarLoaderID(__instance.gameObject.gameObject.name[10].ToString());
                ClientSend.SendOilBin(carLoaderID);
            }
        }
    }
}