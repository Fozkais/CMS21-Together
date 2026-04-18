using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared;
using HarmonyLib;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class OilBin
{
	public static bool listen = true;

	[HarmonyPatch(typeof(CarLoader), nameof(CarLoader.UseOilbin))]
	[HarmonyPrefix]
	public static void UseOilBinPatch(CarLoader __instance)
	{
		if(!Client.Instance.isConnected) return;
		if (!listen) { listen = true; return;}

		int carLoaderID = DataHelper.ExtractCarLoaderIDFromName(__instance.gameObject.name);
		if (carLoaderID < 0) return;
		ClientSend.SendOilBin(carLoaderID);
	}
}