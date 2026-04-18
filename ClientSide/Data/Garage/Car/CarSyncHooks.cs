using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared;
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

		var carLoaderID = DataHelper.ExtractCarLoaderIDFromName(__instance.gameObject.name);
		if (carLoaderID < 0) return;
		if (!ClientData.Instance.loadedCars.TryGetValue(carLoaderID, out var car)) return;

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

		var carLoaderID = DataHelper.ExtractCarLoaderIDFromName(__instance.gameObject.name);
		if (carLoaderID < 0) return true;
		MelonLogger.Msg($"Move {__instance.carToLoad} to {no}.");
		if (! ClientData.Instance.loadedCars.ContainsKey(carLoaderID))
			return true;
		if (no == -1)
			return false;

		var car = ClientData.Instance.loadedCars[carLoaderID];

		// Guard against echo: if the position already matches what we have locally, the
		// ChangePosition likely came from a relayed packet that slipped past the listen
		// flag (e.g. via resync). Re-broadcasting would fight the originating client.
		if (car.carPosition == no) return true;

		car.carPosition = no;

		ClientSend.CarPositionPacket(carLoaderID, no);
		return true;
	}
}