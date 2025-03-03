using CMS21Together.ClientSide.Data.Handle;
using HarmonyLib;
using MelonLoader;

namespace CMS21Together.ClientSide.Data.Garage.Car;

[HarmonyPatch]
public static class CarSyncHooks
{
	public static bool listenToChangePosition = true;

	[HarmonyPatch(typeof(CarLoader), nameof(CarLoader.SwitchCarPart), typeof(string))]
	[HarmonyPostfix]
	public static void SwitchCarPartHook(string name, CarLoader __instance)
	{
		if (!Client.Instance.isConnected) return;

		var carLoaderID = __instance.gameObject.name[10] - '0' - 1;
		var car = ClientData.Instance.loadedCars[carLoaderID];

		if (PartUpdateHooks.FindBodyPartInDictionary(car, name, out var key))
		{
			var part = car.CarPartInfo.BodyPartsReferences[key];
			MelonCoroutines.Start(PartUpdateHooks.SendBodyPart(part, key, carLoaderID));
		}
	}

	[HarmonyPatch(typeof(CarLoader), nameof(CarLoader.ChangePosition), typeof(int))]
	[HarmonyPrefix]
	public static bool ChangePositionHook(int no, CarLoader __instance)
	{
		if (!Client.Instance.isConnected || !listenToChangePosition)
		{
			listenToChangePosition = true;
			return true;
		}

		var carLoaderID = __instance.gameObject.name[10] - '0' - 1;
		MelonLogger.Msg($"Move {__instance.carToLoad} to {no}.");
		if (! ClientData.Instance.loadedCars.ContainsKey(carLoaderID))
			return true;
		if (no == -1)
			return false;
		
		var car = ClientData.Instance.loadedCars[carLoaderID];
		car.carPosition = no;
		
		ClientSend.CarPositionPacket(carLoaderID, no);
		return true;
	}
}