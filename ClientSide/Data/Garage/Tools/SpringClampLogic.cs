using System.Collections;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using HarmonyLib;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class SpringClampLogic
{
	public static bool listen = true;

	[HarmonyPatch(typeof(global::SpringClampLogic), nameof(global::SpringClampLogic.SetGroupOnSpringClamp))]
	[HarmonyPrefix]
	public static void SetGroupOnSpringClampHook(GroupItem groupItem, bool instant, bool mount)
	{
		if (!Client.Instance.isConnected || !listen)
		{
			listen = true;
			return;
		}

		var item = new ModGroupItem(groupItem);
		ClientSend.SetSpringClampPacket(item, instant, mount);
	}

	[HarmonyPatch(typeof(global::SpringClampLogic), nameof(global::SpringClampLogic.ClearSpringClamp))]
	[HarmonyPrefix]
	public static void ClearSpringPHook()
	{
		if (!Client.Instance.isConnected || !listen)
		{
			listen = true;
			return;
		}

		ClientSend.ClearSpringClampPacket();
	}

	public static IEnumerator Action(ModGroupItem item, bool instant, bool mount)
	{
		if (SceneManager.CurrentScene() != GameScene.garage) yield break;

		while (!GameData.isReady)
			yield return new WaitForSeconds(0.2f);

		listen = false;
		GameData.Instance.springClampLogic.SetGroupOnSpringClamp(item.ToGame(), instant, mount);
	}
}