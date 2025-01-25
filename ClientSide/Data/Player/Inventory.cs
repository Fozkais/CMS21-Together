using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.Shared.Data.Vanilla;
using HarmonyLib;
using MelonLoader;

namespace CMS21Together.ClientSide.Data.Player;

[HarmonyPatch]
public static class Inventory
{
	public static List<ModItem> items = new();
	public static List<ModGroupItem> groupItems = new();
	private static bool loadSkip;

	public static bool listenToAddItem = true;
	public static bool listenToAddGroupItem = true;
	
	public static bool listenToRemoveItem = true;
	public static bool listenToRemoveGroupItem = true;

	public static void Reset()
	{
		items.Clear();
		groupItems.Clear();
		loadSkip = false;
		
		listenToAddItem = true;
		listenToAddGroupItem = true;
		listenToRemoveItem = true;
		listenToRemoveGroupItem = true;
	}


	[HarmonyPatch(typeof(global::Inventory), "Add", typeof(Item), typeof(bool))]
	[HarmonyPrefix]
	public static void AddItemHook(Item item, bool showPopup = false)
	{
		if (!Client.Instance.isConnected || !listenToAddItem) {listenToAddItem = true; return;}
		
		//MelonLogger.Msg($"Add new item with UID: {item.UID}.");
		var newItem = new ModItem(item);
		items.Add(newItem);
		ClientSend.ItemPacket(newItem, InventoryAction.add);
	}

	[HarmonyPatch(typeof(global::Inventory), "AddGroup")]
	[HarmonyPrefix]
	public static void AddGroupItemHook(GroupItem group)
	{
		if (!Client.Instance.isConnected || !listenToAddGroupItem) {listenToAddGroupItem = true; return;}

		//MelonLogger.Msg($"Add new group item with UID: {group.UID}.");
		var newItem = new ModGroupItem(group);
		groupItems.Add(newItem);
		ClientSend.GroupItemPacket(newItem, InventoryAction.add);
	}

	[HarmonyPatch(typeof(global::Inventory), "Delete")]
	[HarmonyPrefix]
	public static void RemoveItemHook(Item item, global::Inventory __instance)
	{
		if (!Client.Instance.isConnected || !listenToRemoveItem) {listenToRemoveItem = true; return;}

		if (item == null) return;

		if (items.Any(s => s.UID == item.UID))
		{
			var itemToRemove = items.First(s => s.UID == item.UID);
			ClientSend.ItemPacket(itemToRemove, InventoryAction.remove);
			items.Remove(itemToRemove);
		}
	}

	[HarmonyPatch(typeof(global::Inventory), "DeleteGroup")]
	[HarmonyPrefix]
	public static void RemoveGroupItemHook(long UId)
	{
		if (!Client.Instance.isConnected || !listenToRemoveGroupItem) {listenToRemoveGroupItem = true; return;}

		if (groupItems.Any(s => s.UID == UId))
		{
			var itemToRemove = groupItems.First(s => s.UID == UId);
			ClientSend.GroupItemPacket(itemToRemove, InventoryAction.remove);
			groupItems.Remove(itemToRemove);
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
				groupItems.Add(newItem);
				ClientSend.GroupItemPacket(newItem, InventoryAction.add);
			}

		MelonLogger.Msg($"[Inventory->LoadHook] Loaded {groupItems.Count} groupItem.");

		foreach (var item in inventoryData.items)
			if (item != null)
			{
				var newItem = new ModItem(item);
				items.Add(newItem);
				ClientSend.ItemPacket(newItem, InventoryAction.add);
			}

		MelonLogger.Msg($"[Inventory->LoadHook] Loaded {items.Count} Item.");
		return true;
	}

	public static IEnumerator HandleItem(ModItem item, InventoryAction action)
	{
		yield return GameData.GameReady();

		switch (action)
		{
			case InventoryAction.add:
				items.Add(item);
				listenToAddItem = false;
				GameData.Instance.localInventory.Add(item.ToGame());
				break;
			case InventoryAction.remove:
				if (items.Any(i => i.UID == item.UID))
					items.Remove(item);
				listenToRemoveItem = false;
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
				groupItems.Add(item);
				listenToAddGroupItem = false;
				GameData.Instance.localInventory.AddGroup(item.ToGame());
				break;
			case InventoryAction.remove:
				if (groupItems.Any(i => i.UID == item.UID))
					groupItems.Remove(item);
				listenToRemoveGroupItem = false;
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