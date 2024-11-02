using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data.Vanilla;
using HarmonyLib;

namespace CMS21Together.ClientSide.Data.Garage.Tools;


[HarmonyPatch]
public static class EngineStand
{
	public static bool listen = true;
	
	[HarmonyPatch(typeof(EngineStandLogic), nameof(EngineStandLogic.IncreaseEngineStandAngle))] 
	[HarmonyPrefix]
	public static void IncreaseEngineStandAngleHook(float val)
	{
		if(!Client.Instance.isConnected || !listen) { listen = true; return;}
		
		ClientSend.EngineStandAnglePacket(val);
	}
	
	[HarmonyPatch(typeof(EngineStandLogic), nameof(EngineStandLogic.SetGroupOnEngineStand))] 
	[HarmonyPrefix]
	public static void SetGroupOnEngineStand(GroupItem groupItem, bool withFade = true)
	{
		if(!Client.Instance.isConnected || !listen) { listen = true; return;}

		ClientSend.EngineStandSetGroup(new ModGroupItem(groupItem));
	}
	
	[HarmonyPatch(typeof(PieMenuController), "_GetOnClick_b__72_35")]
	[HarmonyPrefix]
	public static void TakeOffEngineFromStandHook()
	{
		if(!Client.Instance.isConnected || !listen) { listen = true; return;}
		
		ClientSend.TakeOffEnginePacket();
	}
}