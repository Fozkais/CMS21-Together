using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS.Extensions;
using CMS.Helpers;
using CMS.UI;
using CMS.UI.Helpers;
using CMS.UI.Windows;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.Shared.Data.Vanilla;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Player;

[HarmonyPatch]
public static class InventoryFix
{
	[HarmonyPatch(typeof(UIHelper), nameof(UIHelper.GetItemsForID))]
	[HarmonyPrefix]
	public static bool GetItemsForIDFix(Il2CppSystem.Collections.Generic.List<Item> items,
		string id, ref Il2CppSystem.Collections.Generic.List<BaseItem> __result)
	{
		if (!Client.Instance.isConnected)
			return true;

		var array = items.ToArray();
		var snapshot = new List<string>();
		for (int i = 0; i < array.Count; i++)
			snapshot.Add(array[i].ID);
		
		var matches = new ConcurrentBag<int>();
		Parallel.For(0, snapshot.Count, i =>
		{
			if (snapshot[i].IndexOf(id, StringComparison.OrdinalIgnoreCase) >= 0)
				matches.Add(i);
		});
		
		var newRes = new Il2CppSystem.Collections.Generic.List<BaseItem>();
		foreach (int index in matches)
			newRes.Add(array[index]);

		__result = newRes;
		return false;
	}
	
	[HarmonyPatch(typeof(UIHelper), nameof(UIHelper.GetBaseItemsForIDExact))]
	[HarmonyPrefix]
	public static bool GetBaseItemsForIDExactFix(Il2CppSystem.Collections.Generic.List<Item> items,
		string id, ref Il2CppSystem.Collections.Generic.List<BaseItem> __result)
	{
		if (!Client.Instance.isConnected) {return true;}

		var array = items.ToArray();
		var snapshotIds = new string[items.Count];
		for (int i = 0; i < array.Count; i++)
			snapshotIds[i] = array[i]?.ID;
		
		var matchedIndices = new ConcurrentBag<int>();
		Parallel.For(0, snapshotIds.Length, i =>
		{
			var idValue = snapshotIds[i];
			if (idValue != null && idValue == id)
				matchedIndices.Add(i);
		});
		
		var resultList = new Il2CppSystem.Collections.Generic.List<BaseItem>();
		foreach (int index in matchedIndices)
		{
			var item = array[index];
			if (item != null)
				resultList.Add(item);
		}

		__result = resultList;
		
		return false;
	}


	public static List<Item> tmpInventory = new List<Item>();
	
	[HarmonyPatch(typeof(NotificationCenter), nameof(NotificationCenter.MoveItem), typeof(Item), typeof(bool), typeof(string))]
	[HarmonyPrefix]
	public static bool MoveItemFix(Item itemToMove, bool toWarehouse, string windowType, NotificationCenter __instance)
	{
		if (!Client.Instance.isConnected) { return true; }

		if (windowType == "Warehouse")
		{
			if (toWarehouse)
			{
				Singleton<GameManager>.Instance.Inventory.Delete(itemToMove);
				itemToMove.MakeNewUID();
				Singleton<GameManager>.Instance.Warehouse.Add(itemToMove);
			}
			else
			{
				Singleton<GameManager>.Instance.Warehouse.Delete(itemToMove);
				itemToMove.MakeNewUID();
				Singleton<GameManager>.Instance.Inventory.Add(itemToMove);
			}
			WindowManager.Instance.GetWindowByID<WarehouseWindow>(WindowID.Warehouse).Refresh(false);
			/*string text;
			string text2;
			if (ItemHelper.IsLicensePlate(itemToMove, out text))
			{
				text2 = ItemHelper.GetLocalizedLicensePlateName(text);
			}
			else
			{
				text2 = itemToMove.GetLocalizedName();
			}
			__instance.uiManager.ShowPopup("GUI_ItemMoved".Localize(), text2, PopupType.Normal);*/
			return false;
		}

		if (windowType == "ItemsExchange")
		{
			ItemsExchangeWindow windowByID = WindowManager.Instance.GetWindowByID<ItemsExchangeWindow>(WindowID.ItemsExchange);
			if (windowByID.Junk == null || windowByID.Junk.ItemsInTrash == null)
			{
				return false;
			}

			if (toWarehouse)
			{
				windowByID.Junk.ItemsInTrash.Remove(itemToMove);
				Singleton<GameManager>.Instance.TempInventory.AddItem(itemToMove);
				tmpInventory.Add(itemToMove);
			}
			else
			{
				windowByID.Junk.ItemsInTrash.Add(itemToMove);
				Singleton<GameManager>.Instance.TempInventory.RemoveItem(itemToMove);
				tmpInventory.Remove(itemToMove);
			}

			SoundManager.Get().PlaySFXOneShot("PartTakeOff");
			windowByID.Refresh(false);
			string text3;
			string text4;
			if (ItemHelper.IsLicensePlate(itemToMove, out text3))
			{
				text4 = ItemHelper.GetLocalizedLicensePlateName(text3);
			}
			else
			{
				text4 = itemToMove.GetLocalizedName();
			}

			//__instance.uiManager.ShowPopup("GUI_ItemMoved".Localize(), text4, PopupType.Normal);
		}
		return false;
	}
}