using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data.Vanilla;
using HarmonyLib;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class LifterLogic
{
	public static bool listen = true;

	[HarmonyPatch(typeof(CarLifter), "Action")]
	[HarmonyPostfix]
	public static void LifterActionHook(int actionType, CarLifter __instance)
	{
		if (!Client.Instance.isConnected || !listen)
		{
			listen = true;
			return;
		}

		var action = 0;
		if (actionType == 0) action = 1;
		else if (actionType == 1) action = -1;

		var currentState = (int)__instance.currentState + action;
		var carLoaderID = __instance.connectedCarLoader.gameObject.name[10] - '0' - 1;

		var state = GetState(currentState);
		ClientSend.LifterPacket(state, carLoaderID);
	}

	private static ModLifterState GetState(int value)
	{
		var state = ModLifterState.ground;
		switch (value)
		{
			case 0:
				state = ModLifterState.ground;
				break;
			case 1:
				state = ModLifterState.low;
				break;
			case 2:
				state = ModLifterState.high;
				break;
		}

		return state;
	}
}