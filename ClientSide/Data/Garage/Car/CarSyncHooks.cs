using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace CMS21Together.ClientSide.Data.Garage.Car;

[HarmonyPatch]
public static class CarSyncHooks
{
    public static bool listenToChangePosition = true;
    
    [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.SwitchCarPart), new []{ typeof(string) })]
    [HarmonyPostfix]
    public static void SwitchCarPartHook(string name, CarLoader __instance)
    {
        if(!Client.Instance.isConnected) { return;}
        
        int carLoaderID = (__instance.gameObject.name[10] - '0') - 1;
        ModCar car = ClientData.Instance.loadedCars[carLoaderID];
        
        if (PartUpdateHooks.FindBodyPartInDictionary(car,  name, out int key))
        {
            CarPart part = car.partInfo.BodyPartsReferences[key];
            MelonCoroutines.Start(PartUpdateHooks.SendBodyPart(part, key, carLoaderID));
        }
            
    }
    
    [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.ChangePosition), new []{ typeof(int) })]
    [HarmonyPostfix]
    public static void ChangePositionHook(int no, CarLoader __instance)
    {
        if(!Client.Instance.isConnected || !listenToChangePosition ) {listenToChangePosition = true; return;}
        
        int carLoaderID = (__instance.gameObject.name[10] - '0') - 1;
        ModCar car = ClientData.Instance.loadedCars[carLoaderID];
        car.carPosition = no;

        ClientSend.CarPositionPacket(carLoaderID, no);



    }
}