using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS.Helpers;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.Shared.Data.Vanilla;
using HarmonyLib;
using MelonLoader;

namespace CMS21Together.ClientSide.Data.Player;

[HarmonyPatch]
public static class Inventory
{
	public static List<ModItem> modItems = new();
	public static List<ModGroupItem> modGroupItems = new();
	private static bool loadSkip;

	public static void Reset()
	{
		modItems.Clear();
		modGroupItems.Clear();
		loadSkip = false;
		
	}

	[HarmonyPatch(typeof(UIHelper), nameof(UIHelper.GetItemsForID), typeof(List<Item>), typeof(string))]
	[HarmonyPrefix]
	public static bool GetItemsForIDFix(List<Item> items, string id, ref List<BaseItem> __result)
	{
		if (!Client.Instance.isConnected) {return true;}

		Item[] snapshot = items.ToArray();
		ConcurrentBag<BaseItem> bag = new ConcurrentBag<BaseItem>();
		Parallel.ForEach(snapshot, item =>
		{
			if (item.ID.IndexOf(id, StringComparison.OrdinalIgnoreCase) >= 0)
				bag.Add(item);
		});
		__result = bag.ToList();
		
		return false;
	}
	
	[HarmonyPatch(typeof(UIHelper), nameof(UIHelper.GetBaseItemsForIDExact), typeof(List<Item>), typeof(string))]
	[HarmonyPrefix]
	public static bool GetBaseItemsForIDExactFix(List<Item> items, string id, ref List<BaseItem> __result)
	{
		if (!Client.Instance.isConnected) {return true;}

		Item[] snapshot = items.ToArray();
		ConcurrentBag<BaseItem> bag = new ConcurrentBag<BaseItem>();
		Parallel.For(0, snapshot.Length, i =>
		{
			var item = snapshot[i];
			if (item.ID == id)
				bag.Add(item);
		});
		__result = bag.ToList();
		
		return false;
	}


	[HarmonyPatch(typeof(global::Inventory), "Add", typeof(Item), typeof(bool))]
	[HarmonyPrefix]
	public static void AddItemHook(Item item, bool showPopup = false)
	{
		if (!Client.Instance.isConnected) {return;}
		if (modItems.Any(i => i.UID == item.UID)) return;
		
		var newItem = new ModItem(item);
		modItems.Add(newItem);
		ClientSend.ItemPacket(newItem, InventoryAction.add);
	}

	[HarmonyPatch(typeof(global::Inventory), "AddGroup")]
	[HarmonyPrefix]
	public static void AddGroupItemHook(GroupItem group)
	{
		if (!Client.Instance.isConnected) {return;}
		if (modGroupItems.Any(i => i.UID == group.UID)) return;

		//MelonLogger.Msg($"Add new group item with UID: {group.UID}.");
		var newItem = new ModGroupItem(group);
		modGroupItems.Add(newItem);
		ClientSend.GroupItemPacket(newItem, InventoryAction.add);
	}

	[HarmonyPatch(typeof(global::Inventory), "Delete")]
	[HarmonyPrefix]
	public static void RemoveItemHook(Item item, global::Inventory __instance)
	{
		if (!Client.Instance.isConnected) {return;}

		if (item == null) return;

		if (modItems.Any(s => s.UID == item.UID))
		{
			var itemToRemove = modItems.First(s => s.UID == item.UID);
			ClientSend.ItemPacket(itemToRemove, InventoryAction.remove);
			modItems.Remove(itemToRemove);
		}
	}

	[HarmonyPatch(typeof(global::Inventory), "DeleteGroup")]
	[HarmonyPrefix]
	public static void RemoveGroupItemHook(long UId)
	{
		if (!Client.Instance.isConnected ) {return;}

		if (modGroupItems.Any(s => s.UID == UId))
		{
			var itemToRemove = modGroupItems.First(s => s.UID == UId);
			ClientSend.GroupItemPacket(itemToRemove, InventoryAction.remove);
			modGroupItems.Remove(itemToRemove);
		}
	}

	[HarmonyPatch(typeof(global::Inventory), "Load")]
	[HarmonyPrefix]
	public static bool LoadHook(global::Inventory __instance)
	{
		if (!Client.Instance.isConnected) return true;

		if (!Server.Instance.isRunning)
		{
			if (loadSkip)
			{
				ClientSend.ItemPacket(null, InventoryAction.resync);
				ClientSend.GroupItemPacket(null, InventoryAction.resync);
			}
			else
			{
				loadSkip = true;
				return false;
			}
		}

		var inventoryData = Singleton<GameManager>.Instance.GameDataManager.CurrentProfileData.inventoryData;
		foreach (var group in inventoryData.groups)
			if (group != null)
			{
				var newItem = new ModGroupItem(group);
				modGroupItems.Add(newItem);
				ClientSend.GroupItemPacket(newItem, InventoryAction.add);
			}

		MelonLogger.Msg($"[Inventory->LoadHook] Loaded {modGroupItems.Count} groupItem.");

		foreach (var item in inventoryData.items)
			if (item != null)
			{
				var newItem = new ModItem(item);
				modItems.Add(newItem);
				ClientSend.ItemPacket(newItem, InventoryAction.add);
			}

		MelonLogger.Msg($"[Inventory->LoadHook] Loaded {modItems.Count} Item.");
		return true;
	}

	public static IEnumerator HandleItem(ModItem item, InventoryAction action)
	{
		yield return GameData.GameReady();

		switch (action)
		{
			case InventoryAction.add:
				modItems.Add(item);
				GameData.Instance.localInventory.Add(item.ToGame());
				break;
			case InventoryAction.remove:
				if (modItems.Any(i => i.UID == item.UID))
					modItems.Remove(item);
				GameData.Instance.localInventory.Delete(item.ToGame());
				break;
		}
	}

	public static IEnumerator HandleGroupItem(ModGroupItem item, InventoryAction action)
	{
		yield return GameData.GameReady();
		switch (action)
		{
			case InventoryAction.add:
				modGroupItems.Add(item);
				GameData.Instance.localInventory.AddGroup(item.ToGame());
				break;
			case InventoryAction.remove:
				if (modGroupItems.Any(i => i.UID == item.UID))
					modGroupItems.Remove(item);
				GameData.Instance.localInventory.DeleteGroup(item.UID);
				break;
		}
	}
}

[Serializable]
public enum InventoryAction
{
	add,
	remove,
	resync
}