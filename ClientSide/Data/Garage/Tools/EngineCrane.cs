using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data.Vanilla;
using HarmonyLib;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class EngineCrane
{
	public static bool listen = true;
	
	[HarmonyPatch(typeof(NotificationCenter), nameof(NotificationCenter.InsertEngineToCar))]
	[HarmonyPrefix]
	public static void InsertEngineIntoCarHook(GroupItem engine)
	{
		if(!Client.Instance.isConnected || !listen) { listen = true; return;}
		
		ClientSend.EngineCraneHandlePacket(1,new ModGroupItem(engine));
	}
        
	[HarmonyPatch(typeof(CarLoader), nameof(CarLoader.UseEngineCrane))]
	[HarmonyPostfix]
	public static void UseEngineCraneHook(CarLoader __instance)
	{
		if(!Client.Instance.isConnected || !listen) { listen = true; return;}
		
		ClientSend.EngineCraneHandlePacket(-1);
	}
}