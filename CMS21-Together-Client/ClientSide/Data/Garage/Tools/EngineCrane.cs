using System.Collections;
using System.Linq;
using CMS;
using CMS21_Together_Core.Data.Vanilla;
using CMS21Together.ClientSide.Data.Handle;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class EngineCrane
{
	public static bool listen = true;

	[HarmonyPatch(typeof(NotificationCenter), nameof(NotificationCenter.InsertEngineToCar))]
	[HarmonyPrefix]
	public static void InsertEngineIntoCarHook(GroupItem engine)
	{
		if (!Client.Instance.isConnected || !listen)
		{
			listen = true;
			return;
		}

		MelonLogger.Msg("[EngineCrane->InsertEngineIntoCarHook] Hook!");
		ClientSend.EngineCraneHandlePacket(1, -1, new ModGroupItem(engine));
	}

	[HarmonyPatch(typeof(CarLoader), nameof(CarLoader.UseEngineCrane))]
	[HarmonyPostfix]
	public static void UseEngineCraneHook(CarLoader __instance)
	{
		if (!Client.Instance.isConnected || !listen)
		{
			listen = true;
			return;
		}

		MelonLogger.Msg("[EngineCrane->UseEngineCraneHook] Hook!");
		var carLoaderID = __instance.gameObject.name[10] - '0' - 1;
		ClientSend.EngineCraneHandlePacket(-1, carLoaderID);
	}

	public static IEnumerator UseEngineCrane(int carLoaderID)
	{
		while (!GameData.isReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		//GameData.Instance.carLoaders[carLoaderID].UseEngineCrane();
		var carLoader = GameData.Instance.carLoaders[carLoaderID];

		var engine = carLoader.GetEngine();
		var iO = engine.GetComponent<InteractiveObject>();
		if (iO == null) yield break;
		var text = iO.CanUnmountGroup();
		if (!string.IsNullOrEmpty(text)) yield break;
		var mountedItemsAmount = iO.GetMountedItemsAmount();
		Debug.Log(string.Format("[ToolsManager] -> UseEngineCrane() Engine parts amount: {0}", mountedItemsAmount));
		if (mountedItemsAmount < 1) yield break;
		if (!carLoader.EngineData.isElectric && carLoader.FluidsData.GetLevel(CarFluidType.EngineOil) > 0f) yield break;
		if (carLoader.GetEngineSide() == direction.front)
		{
			var carPart = carLoader.GetCarPart("hood");
			if (!carPart.Switched && !carPart.Unmounted) MainMod.StartCoroutine(carLoader.SwitchCarPart(carPart, true));
			carPart = carLoader.GetCarPart("engine_cover_openable");
			if (!carPart.Switched && !carPart.Unmounted) MainMod.StartCoroutine(carLoader.SwitchCarPart(carPart, true));
			carPart = carLoader.GetCarPart("clamshell_front_openable");
			if (!carPart.Switched && !carPart.Unmounted) MainMod.StartCoroutine(carLoader.SwitchCarPart(carPart, true));
		}
		else
		{
			var carPart2 = carLoader.GetCarPart("trunk");
			if (!carPart2.Switched && !carPart2.Unmounted) MainMod.StartCoroutine(carLoader.SwitchCarPart(carPart2, true));
			carPart2 = carLoader.GetCarPart("clamshell_rear_openable");
			if (!carPart2.Switched && !carPart2.Unmounted) MainMod.StartCoroutine(carLoader.SwitchCarPart(carPart2, true));
		}

		NotificationCenter.Get().ActionUnMountGroup(iO);
	}

	public static IEnumerator InsertEngineIntoCar(ModGroupItem engine)
	{
		while (!GameData.isReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		var group = engine.ToGame();

		var selectedCarLoader = global::ToolsMoveManager.Get().GetConnectedCarLoader(IOSpecialType.EngineCrane);
		if (selectedCarLoader == null) yield break;
		var currentEngine = selectedCarLoader.GetEngine();
		if (currentEngine == null) yield break;
		if (selectedCarLoader.GetCustomerCar() && group.ID != currentEngine.name) yield break;

		var engineCrane = global::ToolsMoveManager.Get().GetTool(IOSpecialType.EngineCrane);
		engineCrane.GetComponent<InteractiveObject>().on = false;

		yield return YieldInstructions.WaitForEndOfFrame;
		if (selectedCarLoader.GetEngineSide() == direction.front)
		{
			var carPart = selectedCarLoader.GetCarPart("hood");
			if (!carPart.Switched && !carPart.Unmounted) MainMod.StartCoroutine(selectedCarLoader.SwitchCarPart(carPart, true));
			carPart = selectedCarLoader.GetCarPart("engine_cover_openable");
			if (!carPart.Switched && !carPart.Unmounted) MainMod.StartCoroutine(selectedCarLoader.SwitchCarPart(carPart, true));
			carPart = selectedCarLoader.GetCarPart("clamshell_front_openable");
			if (!carPart.Switched && !carPart.Unmounted) MainMod.StartCoroutine(selectedCarLoader.SwitchCarPart(carPart, true));
		}
		else
		{
			var carPart2 = selectedCarLoader.GetCarPart("trunk");
			if (!carPart2.Switched && !carPart2.Unmounted) MainMod.StartCoroutine(selectedCarLoader.SwitchCarPart(carPart2, true));
			carPart2 = selectedCarLoader.GetCarPart("clamshell_rear_openable");
			if (!carPart2.Switched && !carPart2.Unmounted) MainMod.StartCoroutine(selectedCarLoader.SwitchCarPart(carPart2, true));
		}

		if (group.ID == currentEngine.name)
		{
			Debug.Log("[NotificationCenter] -> ActionInsertEngineToCar() Change existing engine");
			if (group.ID == selectedCarLoader.EngineParams.Type) selectedCarLoader.EngineParams.EngineSwap = string.Empty;
			Component[] array = currentEngine.gameObject.GetComponentsInChildren<PartScript>();
			for (var i = 0; i < array.Length; i++)
			{
				var partScript = (PartScript)array[i];
				var name = partScript.gameObject.name;
				var num = group.ItemList.IndexOf(group.ItemList.ToArray().ToList().Find(inventoryItem => inventoryItem.ID == name));
				if (num != -1)
				{
					if (partScript.IsUnmounted) partScript.MountByGroup(true);
					partScript.TunePart(group.ItemList.ToArray()[num].GetNormalID());
					partScript.SetCondition(group.ItemList.ToArray()[num].Condition);
					partScript.IsExamined = group.ItemList.ToArray()[num].IsExamined;
					partScript.Quality = group.ItemList.ToArray()[num].Quality;
					if (group.ItemList.ToArray()[num].IsPainted)
					{
						partScript.SetColor(group.ItemList.ToArray()[num].Color.GetColor());
						partScript.CurrentPaintType = group.ItemList.ToArray()[num].PaintType;
						partScript.CurrentPaintData = group.ItemList.ToArray()[num].PaintData;
						if (partScript.CurrentPaintType != PaintType.Custom)
							PaintHelper.SetPaintType(partScript.gameObject, partScript.CurrentPaintType, false);
						else
							PaintHelper.SetCustomPaintType(partScript.gameObject, partScript.CurrentPaintData, false);
					}

					partScript.SetMountObjectData(group.ItemList.ToArray()[num].MountObjectData);
					group.ItemList.RemoveAt(num);
				}
			}
		}
		else
		{
			Debug.LogWarning("Swap engine");
			Debug.Log("[NotificationCenter] -> ActionInsertEngineToCar() Swap engine");
			selectedCarLoader.EngineParams.EngineSwap = group.ID;
			Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_swap", 1);
			MainMod.StartCoroutine(selectedCarLoader.SwapEngine(group));
		}

		Singleton<GameManager>.Instance.Inventory.DeleteGroup(group.UID);
	}
}