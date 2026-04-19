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
	private static bool resyncRequested;
	private static bool suppressInventoryHooks;

	public static void Reset()
	{
		modItems.Clear();
		modGroupItems.Clear();
		loadSkip = false;
		resyncRequested = false;
		suppressInventoryHooks = false;
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


	[HarmonyPatch(typeof(global::Inventory), "Add", typeof(Item), typeof(bool))]
	[HarmonyPrefix]
	public static void AddItemHook(Item item, bool showPopup = false)
	{
		if (!Client.Instance.isConnected) {return;}
		if (suppressInventoryHooks) return;
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
		if (suppressInventoryHooks) return;
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
		if (suppressInventoryHooks) return;

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
		if (suppressInventoryHooks) return;

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
			if (!loadSkip)
			{
				loadSkip = true;
				return false;
			}

			if (!resyncRequested)
			{
				ClientSend.ItemPacket(null, InventoryAction.resync);
				ClientSend.GroupItemPacket(null, InventoryAction.resync);
				resyncRequested = true;
			}

			return false;
		}

		// Security Rule: Validate inventory data is available
		var inventoryData = Singleton<GameManager>.Instance.GameDataManager.CurrentProfileData.inventoryData;
		if (inventoryData == null)
		{
			MelonLogger.Warning("[Inventory->LoadHook] inventoryData is null, skipping load.");
			return true;
		}

		// Business Logic: Load group items from save data
		int loadedGroups = 0;
		foreach (var group in inventoryData.groups)
		{
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
		}

		MelonLogger.Msg($"[Inventory->LoadHook] Loaded {loadedGroups} new group items (total: {modGroupItems.Count}).");

		// Business Logic: Load items from save data
		int loadedItems = 0;
		foreach (var item in inventoryData.items)
		{
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

				// Security Rule: Validate item conversion before adding to game inventory
				var gameItem = item.ToGame();
				if (gameItem != null)
				{
					suppressInventoryHooks = true;
					try
					{
						modItems.Add(item);
						GameData.Instance.localInventory.Add(gameItem);
						MelonLogger.Msg($"[Inventory->HandleItem] Added item UID: {item.UID}");
					}
					finally
					{
						suppressInventoryHooks = false;
					}
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
				var existingItem = modItems.FirstOrDefault(i => i.UID == item.UID);
				if (existingItem != null)
				{
					modItems.Remove(existingItem);
					
					var itemToRemove = GameData.Instance.localInventory.GetItem(item.UID);
					if (itemToRemove != null)
					{
						suppressInventoryHooks = true;
						try
						{
							GameData.Instance.localInventory.Delete(itemToRemove);
							MelonLogger.Msg($"[Inventory->HandleItem] Removed item UID: {item.UID}");
						}
						finally
						{
							suppressInventoryHooks = false;
						}
					}
					else
					{
						MelonLogger.Warning($"[Inventory->HandleItem] Local item not found for removal, UID: {item.UID}");
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

				// Security Rule: Validate item conversion before adding to game inventory
				var gameGroupItem = item.ToGame();
				if (gameGroupItem != null)
				{
					suppressInventoryHooks = true;
					try
					{
						modGroupItems.Add(item);
						GameData.Instance.localInventory.AddGroup(gameGroupItem);
						MelonLogger.Msg($"[Inventory->HandleGroupItem] Added group item UID: {item.UID}");
					}
					finally
					{
						suppressInventoryHooks = false;
					}
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
				var existingGroupItem = modGroupItems.FirstOrDefault(i => i.UID == item.UID);
				if (existingGroupItem != null)
				{
					modGroupItems.Remove(existingGroupItem);
					suppressInventoryHooks = true;
					try
					{
						GameData.Instance.localInventory.DeleteGroup(item.UID);
						MelonLogger.Msg($"[Inventory->HandleGroupItem] Removed group item UID: {item.UID}");
					}
					finally
					{
						suppressInventoryHooks = false;
					}
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

[HarmonyPatch]
public static class WarehouseSync
{
	private static bool loadSkip;
	private static bool resyncRequested;
	private static bool suppressWarehouseHooks;

	public static void Reset()
	{
		loadSkip = false;
		resyncRequested = false;
		suppressWarehouseHooks = false;
	}

	[HarmonyPatch(typeof(global::Warehouse), "Add", typeof(Item))]
	[HarmonyPrefix]
	public static void AddItemHook(Item item, global::Warehouse __instance)
	{
		if (!Client.Instance.isConnected || suppressWarehouseHooks || item == null) return;

		ClientSend.WarehouseItemPacket(GetSelectedWarehouseIndex(__instance), new ModItem(item), InventoryAction.add);
	}

	[HarmonyPatch(typeof(global::Warehouse), "Add", typeof(GroupItem))]
	[HarmonyPrefix]
	public static void AddGroupItemHook(GroupItem item, global::Warehouse __instance)
	{
		if (!Client.Instance.isConnected || suppressWarehouseHooks || item == null) return;

		ClientSend.WarehouseGroupItemPacket(GetSelectedWarehouseIndex(__instance), new ModGroupItem(item), InventoryAction.add);
	}

	[HarmonyPatch(typeof(global::Warehouse), "Delete", typeof(Item))]
	[HarmonyPrefix]
	public static void DeleteItemHook(Item item, global::Warehouse __instance)
	{
		if (!Client.Instance.isConnected || suppressWarehouseHooks || item == null) return;

		ClientSend.WarehouseItemPacket(FindItemWarehouseIndex(__instance, item.UID), new ModItem(item), InventoryAction.remove);
	}

	[HarmonyPatch(typeof(global::Warehouse), "Delete", typeof(GroupItem))]
	[HarmonyPrefix]
	public static void DeleteGroupItemHook(GroupItem group, global::Warehouse __instance)
	{
		if (!Client.Instance.isConnected || suppressWarehouseHooks || group == null) return;

		ClientSend.WarehouseGroupItemPacket(FindGroupWarehouseIndex(__instance, group.UID), new ModGroupItem(group), InventoryAction.remove);
	}

	[HarmonyPatch(typeof(global::Warehouse), "SetWarehouseName")]
	[HarmonyPrefix]
	public static void SetWarehouseNameHook(int index, string newName)
	{
		if (!Client.Instance.isConnected || suppressWarehouseHooks) return;

		ClientSend.WarehouseNamePacket(index, newName);
	}

	[HarmonyPatch(typeof(global::Warehouse), "UpgradeWarehouse")]
	[HarmonyPostfix]
	public static void UpgradeWarehouseHook(global::Warehouse __instance)
	{
		if (!Client.Instance.isConnected || suppressWarehouseHooks) return;

		ClientSend.WarehouseSnapshotPacket(new ModWarehouseData(__instance), InventoryAction.add);
	}

	[HarmonyPatch(typeof(global::Warehouse), "Load")]
	[HarmonyPrefix]
	public static bool LoadHook(global::Warehouse __instance)
	{
		if (!Client.Instance.isConnected) return true;

		if (!Server.Instance.isRunning)
		{
			if (!loadSkip)
			{
				loadSkip = true;
				return false;
			}

			if (!resyncRequested)
			{
				ClientSend.WarehouseSnapshotPacket(null, InventoryAction.resync);
				resyncRequested = true;
			}

			return false;
		}

		return true;
	}

	[HarmonyPatch(typeof(global::Warehouse), "Load")]
	[HarmonyPostfix]
	public static void LoadPostfix(global::Warehouse __instance)
	{
		if (!Client.Instance.isConnected || !Server.Instance.isRunning || suppressWarehouseHooks) return;

		ClientSend.WarehouseSnapshotPacket(new ModWarehouseData(__instance), InventoryAction.add);
	}

	public static IEnumerator HandleSnapshot(ModWarehouseData warehouseData)
	{
		yield return GameData.GameReady();

		if (warehouseData == null)
		{
			MelonLogger.Warning("[WarehouseSync->HandleSnapshot] Received null warehouse snapshot, skipping.");
			yield break;
		}

		var warehouse = GetWarehouse();
		if (warehouse == null)
		{
			MelonLogger.Warning("[WarehouseSync->HandleSnapshot] Warehouse is null, skipping.");
			yield break;
		}

		suppressWarehouseHooks = true;
		try
		{
			warehouseData.ApplyToGame(warehouse);
			MelonLogger.Msg("[WarehouseSync->HandleSnapshot] Applied warehouse snapshot.");
		}
		finally
		{
			suppressWarehouseHooks = false;
		}
	}

	public static IEnumerator HandleItem(int warehouseIndex, ModItem item, InventoryAction action)
	{
		yield return GameData.GameReady();

		if (item == null) yield break;

		var warehouse = GetWarehouse();
		if (warehouse == null) yield break;

		suppressWarehouseHooks = true;
		try
		{
			new ModWarehouseData().ApplyItemToGame(warehouse, warehouseIndex, item, action == InventoryAction.add);
			warehouse.UpdateWarehouseShelfLevel();
			MelonLogger.Msg($"[WarehouseSync->HandleItem] {action} item UID: {item.UID} in warehouse {warehouseIndex}");
		}
		finally
		{
			suppressWarehouseHooks = false;
		}
	}

	public static IEnumerator HandleGroupItem(int warehouseIndex, ModGroupItem item, InventoryAction action)
	{
		yield return GameData.GameReady();

		if (item == null) yield break;

		var warehouse = GetWarehouse();
		if (warehouse == null) yield break;

		suppressWarehouseHooks = true;
		try
		{
			new ModWarehouseData().ApplyGroupItemToGame(warehouse, warehouseIndex, item, action == InventoryAction.add);
			warehouse.UpdateWarehouseShelfLevel();
			MelonLogger.Msg($"[WarehouseSync->HandleGroupItem] {action} group item UID: {item.UID} in warehouse {warehouseIndex}");
		}
		finally
		{
			suppressWarehouseHooks = false;
		}
	}

	public static IEnumerator HandleName(int warehouseIndex, string name)
	{
		yield return GameData.GameReady();

		var warehouse = GetWarehouse();
		if (warehouse == null) yield break;

		suppressWarehouseHooks = true;
		try
		{
			warehouse.SetWarehouseName(warehouseIndex, name);
			MelonLogger.Msg($"[WarehouseSync->HandleName] Synced warehouse {warehouseIndex} name.");
		}
		finally
		{
			suppressWarehouseHooks = false;
		}
	}

	private static global::Warehouse GetWarehouse()
	{
		if (Singleton<GameManager>.Instance == null) return null;

		return Singleton<GameManager>.Instance.Warehouse;
	}

	private static int GetSelectedWarehouseIndex(global::Warehouse warehouse)
	{
		if (warehouse == null || warehouse.SelectedOption < 0) return 0;

		return warehouse.SelectedOption;
	}

	private static int FindItemWarehouseIndex(global::Warehouse warehouse, long uid)
	{
		if (warehouse?.warehouseList == null) return GetSelectedWarehouseIndex(warehouse);

		for (var warehouseIndex = 0; warehouseIndex < warehouse.warehouseList.Count; warehouseIndex++)
		{
			var items = warehouse.warehouseList[warehouseIndex];
			if (items == null) continue;

			for (var itemIndex = 0; itemIndex < items.Count; itemIndex++)
				if (items[itemIndex] != null && items[itemIndex].UID == uid)
					return warehouseIndex;
		}

		return GetSelectedWarehouseIndex(warehouse);
	}

	private static int FindGroupWarehouseIndex(global::Warehouse warehouse, long uid)
	{
		if (warehouse?.warehouseGroupList == null) return GetSelectedWarehouseIndex(warehouse);

		for (var warehouseIndex = 0; warehouseIndex < warehouse.warehouseGroupList.Count; warehouseIndex++)
		{
			var groups = warehouse.warehouseGroupList[warehouseIndex];
			if (groups == null) continue;

			for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
				if (groups[groupIndex] != null && groups[groupIndex].UID == uid)
					return warehouseIndex;
		}

		return GetSelectedWarehouseIndex(warehouse);
	}
}
