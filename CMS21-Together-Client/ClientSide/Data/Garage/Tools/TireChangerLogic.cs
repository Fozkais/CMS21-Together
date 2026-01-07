using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data.Vanilla;
using HarmonyLib;

using TireC= TireChangerLogic;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class TireChangerLogic
{
	public static bool listen = true;

	[HarmonyPatch(typeof(TireC), nameof(TireC.SetGroupOnTireChanger), typeof(GroupItem), typeof(bool), typeof(bool))]
	[HarmonyPostfix]
	public static void TireChangerFix(GroupItem groupItem, bool instant, bool connect, TireC __instance)
	{
		if (!Client.Instance.isConnected || !listen)
		{
			listen = true;
			return;
		}

		if (groupItem == null || groupItem.ItemList.Count == 0) return;

		ClientSend.SetTireChangerPacket(new ModGroupItem(groupItem), instant, connect);
	}

	[HarmonyPatch(typeof(PieMenuController), "_GetOnClick_b__72_61")]
	[HarmonyPostfix]
	public static void TireRemoveActionFix()
	{
		if (!Client.Instance.isConnected || !listen)
		{
			listen = true;
			return;
		}

		ClientSend.ClearTireChangerPacket();
	}
}