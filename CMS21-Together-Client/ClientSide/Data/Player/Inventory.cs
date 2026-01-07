using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS.Helpers;
using CMS21_Together_Core.Data.Vanilla;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
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

	[HarmonyPatch(typeof(UIHelper), nameof(UIHelper.GetItemsForID))]
	[HarmonyPrefix]
	public static bool GetItemsForIDFix(Il2CppSystem.Collections.Generic.List<Item> items,
		string id, ref Il2CppSystem.Collections.Generic.List<BaseItem> __result)
	{
		if (!Client.Instance.isConnected)
			return true;

		var array = items.ToArray();
		var snapshot = new List<string>();
		for (var i = 0; i < array.Count; i++)
			snapshot.Add(array[i].ID);

		var matches = new ConcurrentBag<int>();
		Parallel.For(0, snapshot.Count, i =>
		{
			if (snapshot[i].IndexOf(id, StringComparison.OrdinalIgnoreCase) >= 0)
				matches.Add(i);
		});

		var newRes = new Il2CppSystem.Collections.Generic.List<BaseItem>();
		foreach (var index in matches)
			newRes.Add(array[index]);

		__result = newRes;
		return false;
	}

	[HarmonyPatch(typeof(UIHelper), nameof(UIHelper.GetBaseItemsForIDExact))]
	[HarmonyPrefix]
	public static bool GetBaseItemsForIDExactFix(Il2CppSystem.Collections.Generic.List<Item> items,
		string id, ref Il2CppSystem.Collections.Generic.List<BaseItem> __result)
	{
		if (!Client.Instance.isConnected) return true;

		var array = items.ToArray();
		var snapshotIds = new string[items.Count];
		for (var i = 0; i < array.Count; i++)
			snapshotIds[i] = array[i]?.ID;

		var matchedIndices = new ConcurrentBag<int>();
		Parallel.For(0, snapshotIds.Length, i =>
		{
			var idValue = snapshotIds[i];
			if (idValue != null && idValue == id)
				matchedIndices.Add(i);
		});

		var resultList = new Il2CppSystem.Collections.Generic.List<BaseItem>();
		foreach (var index in matchedIndices)
		{
			var item = array[index];
			if (item != null)
				resultList.Add(item);
		}

		__result = resultList;

		return false;
	}


	[HarmonyPatch(typeof(global::Inventory), "Add", typeof(Item), typeof(bool))]
	[HarmonyPrefix]
	public static void AddItemHook(Item item, bool showPopup = false)
	{
		if (!Client.Instance.isConnected) return;
		if (modItems.Any(i => i.UID == item.UID)) return;

		var newItem = new ModItem(item);
		modItems.Add(newItem);
		ClientSend.ItemPacket(newItem, InventoryAction.add);
	}

	[HarmonyPatch(typeof(global::Inventory), "AddGroup")]
	[HarmonyPrefix]
	public static void AddGroupItemHook(GroupItem group)
	{
		if (!Client.Instance.isConnected) return;
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
		if (!Client.Instance.isConnected) return;

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
		if (!Client.Instance.isConnected) return;

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

		// Security Rule: Validate inventory data is available
		var inventoryData = Singleton<GameManager>.Instance.GameDataManager.CurrentProfileData.inventoryData;
		if (inventoryData == null)
		{
			MelonLogger.Warning("[Inventory->LoadHook] inventoryData is null, skipping load.");
			return true;
		}

		// Business Logic: Load group items from save data
		var loadedGroups = 0;
		foreach (var group in inventoryData.groups)
			if (group != null)
			{
				var newItem = new ModGroupItem(group);

				// Business Rule: Check if group item already exists to prevent duplication during load
				if (!modGroupItems.Any(i => i.UID == newItem.UID))
				{
					modGroupItems.Add(newItem);
					ClientSend.GroupItemPacket(newItem, InventoryAction.add);
					loadedGroups++;
				}
				else
				{
					MelonLogger.Msg($"[Inventory->LoadHook] Group item with UID {newItem.UID} already exists, skipping load.");
				}
			}

		MelonLogger.Msg($"[Inventory->LoadHook] Loaded {loadedGroups} new group items (total: {modGroupItems.Count}).");

		// Business Logic: Load items from save data
		var loadedItems = 0;
		foreach (var item in inventoryData.items)
			if (item != null)
			{
				var newItem = new ModItem(item);

				// Business Rule: Check if item already exists to prevent duplication during load
				if (!modItems.Any(i => i.UID == newItem.UID))
				{
					modItems.Add(newItem);
					ClientSend.ItemPacket(newItem, InventoryAction.add);
					loadedItems++;
				}
				else
				{
					MelonLogger.Msg($"[Inventory->LoadHook] Item with UID {newItem.UID} already exists, skipping load.");
				}
			}

		MelonLogger.Msg($"[Inventory->LoadHook] Loaded {loadedItems} new items (total: {modItems.Count}).");
		return true;
	}

	public static IEnumerator HandleItem(ModItem item, InventoryAction action)
	{
		yield return GameData.GameReady();

		// Security Rule: Validate item is not null before processing
		if (item == null)
		{
			MelonLogger.Warning("[Inventory->HandleItem] Received null item, skipping.");
			yield break;
		}

		// Security Rule: Validate GameData is ready before accessing inventory
		if (GameData.Instance == null || GameData.Instance.localInventory == null)
		{
			MelonLogger.Warning("[Inventory->HandleItem] GameData or localInventory is null, skipping.");
			yield break;
		}

		switch (action)
		{
			case InventoryAction.add:
				// Business Rule: Check if item already exists to prevent duplication
				// This can happen when the host adds an item: AddItemHook adds it, then HandleItem receives it back from server
				if (modItems.Any(i => i.UID == item.UID))
				{
					MelonLogger.Msg($"[Inventory->HandleItem] Item with UID {item.UID} already exists, skipping add to prevent duplication.");
					yield break;
				}

				// Business Logic: Add item to modItems list and game inventory
				modItems.Add(item);

				// Security Rule: Validate item conversion before adding to game inventory
				var gameItem = item.ToGame();
				if (gameItem != null)
				{
					GameData.Instance.localInventory.Add(gameItem);
					MelonLogger.Msg($"[Inventory->HandleItem] Added item UID: {item.UID}");
				}
				else
				{
					MelonLogger.Warning($"[Inventory->HandleItem] Failed to convert ModItem to game Item for UID: {item.UID}");
					// Business Rule: Remove from modItems if conversion failed to keep state consistent
					modItems.Remove(item);
				}

				break;
			case InventoryAction.remove:
				// Business Rule: Only remove if item exists in modItems
				if (modItems.Any(i => i.UID == item.UID))
				{
					modItems.Remove(item);

					// Security Rule: Validate item conversion before removing from game inventory
					var itemToRemove = item.ToGame();
					if (itemToRemove != null)
					{
						GameData.Instance.localInventory.Delete(itemToRemove);
						MelonLogger.Msg($"[Inventory->HandleItem] Removed item UID: {item.UID}");
					}
					else
					{
						MelonLogger.Warning($"[Inventory->HandleItem] Failed to convert ModItem to game Item for removal, UID: {item.UID}");
					}
				}
				else
				{
					MelonLogger.Msg($"[Inventory->HandleItem] Item with UID {item.UID} not found in modItems, skipping remove.");
				}

				break;
		}
	}

	public static IEnumerator HandleGroupItem(ModGroupItem item, InventoryAction action)
	{
		yield return GameData.GameReady();

		// Security Rule: Validate item is not null before processing
		if (item == null)
		{
			MelonLogger.Warning("[Inventory->HandleGroupItem] Received null group item, skipping.");
			yield break;
		}

		// Security Rule: Validate GameData is ready before accessing inventory
		if (GameData.Instance == null || GameData.Instance.localInventory == null)
		{
			MelonLogger.Warning("[Inventory->HandleGroupItem] GameData or localInventory is null, skipping.");
			yield break;
		}

		switch (action)
		{
			case InventoryAction.add:
				// Business Rule: Check if group item already exists to prevent duplication
				// This can happen when the host adds an item: AddGroupItemHook adds it, then HandleGroupItem receives it back from server
				if (modGroupItems.Any(i => i.UID == item.UID))
				{
					MelonLogger.Msg($"[Inventory->HandleGroupItem] Group item with UID {item.UID} already exists, skipping add to prevent duplication.");
					yield break;
				}

				// Business Logic: Add group item to modGroupItems list and game inventory
				modGroupItems.Add(item);

				// Security Rule: Validate item conversion before adding to game inventory
				var gameGroupItem = item.ToGame();
				if (gameGroupItem != null)
				{
					GameData.Instance.localInventory.AddGroup(gameGroupItem);
					MelonLogger.Msg($"[Inventory->HandleGroupItem] Added group item UID: {item.UID}");
				}
				else
				{
					MelonLogger.Warning($"[Inventory->HandleGroupItem] Failed to convert ModGroupItem to game GroupItem for UID: {item.UID}");
					// Business Rule: Remove from modGroupItems if conversion failed to keep state consistent
					modGroupItems.Remove(item);
				}

				break;
			case InventoryAction.remove:
				// Business Rule: Only remove if group item exists in modGroupItems
				if (modGroupItems.Any(i => i.UID == item.UID))
				{
					modGroupItems.Remove(item);
					GameData.Instance.localInventory.DeleteGroup(item.UID);
					MelonLogger.Msg($"[Inventory->HandleGroupItem] Removed group item UID: {item.UID}");
				}
				else
				{
					MelonLogger.Msg($"[Inventory->HandleGroupItem] Group item with UID {item.UID} not found in modGroupItems, skipping remove.");
				}

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