using System;
using System.Collections.Generic;
using CMS.ContainersSave;
using UnhollowerBaseLib;

namespace CMS21Together.Shared.Data.Vanilla;

[Serializable]
public class ModNewInventoryData
{
	public List<ModItem> Items = new();
	public List<ModGroupItem> GroupItems = new();
	public int lastUID;

	public ModNewInventoryData(List<Item> _items, List<GroupItem> _groupItems, int _lastUid)
	{
		foreach (var item in _items) Items.Add(new ModItem(item));
		foreach (var groupItem in _groupItems) GroupItems.Add(new ModGroupItem(groupItem));

		lastUID = _lastUid;
	}
}

[Serializable]
public class ModWarehouseNameData
{
	public bool Default;
	public string Name;

	public ModWarehouseNameData()
	{
	}

	public ModWarehouseNameData(WarehouseNameData data)
	{
		if (data == null) return;

		Default = data.Default;
		Name = data.Name;
	}

	public WarehouseNameData ToGame()
	{
		var data = new WarehouseNameData();
		data.Default = Default;
		data.Name = Name;
		return data;
	}
}

[Serializable]
public class ModWarehouseData
{
	public int AmountOfUnlockedWarehouses;
	public List<List<ModGroupItem>> GroupItems = new();
	public List<List<ModItem>> Items = new();
	public List<ModWarehouseNameData> Names = new();
	public int SelectedOption;

	public ModWarehouseData()
	{
	}

	public ModWarehouseData(global::Warehouse warehouse)
	{
		if (warehouse == null) return;

		AmountOfUnlockedWarehouses = global::Warehouse.amountOfUnlockedWarehouses;
		SelectedOption = warehouse.SelectedOption;

		if (warehouse.warehouseList != null)
		{
			foreach (var warehouseItems in warehouse.warehouseList)
			{
				var items = new List<ModItem>();
				if (warehouseItems != null)
				{
					foreach (var item in warehouseItems)
						if (item != null)
							items.Add(new ModItem(item));
				}
				Items.Add(items);
			}
		}

		if (warehouse.warehouseGroupList != null)
		{
			foreach (var warehouseGroups in warehouse.warehouseGroupList)
			{
				var groups = new List<ModGroupItem>();
				if (warehouseGroups != null)
				{
					foreach (var group in warehouseGroups)
						if (group != null)
							groups.Add(new ModGroupItem(group));
				}
				GroupItems.Add(groups);
			}
		}

		if (warehouse.warehouseNamesData != null)
		{
			foreach (var nameData in warehouse.warehouseNamesData)
				if (nameData != null)
					Names.Add(new ModWarehouseNameData(nameData));
		}
	}

	public void AddItem(int warehouseIndex, ModItem item)
	{
		if (item == null) return;

		var items = EnsureItems(warehouseIndex);
		if (FindItemIndex(items, item.UID) < 0)
			items.Add(item);
	}

	public void RemoveItem(int warehouseIndex, long uid)
	{
		var items = EnsureItems(warehouseIndex);
		var index = FindItemIndex(items, uid);
		if (index >= 0)
			items.RemoveAt(index);
	}

	public void AddGroupItem(int warehouseIndex, ModGroupItem item)
	{
		if (item == null) return;

		var groups = EnsureGroups(warehouseIndex);
		if (FindGroupIndex(groups, item.UID) < 0)
			groups.Add(item);
	}

	public void RemoveGroupItem(int warehouseIndex, long uid)
	{
		var groups = EnsureGroups(warehouseIndex);
		var index = FindGroupIndex(groups, uid);
		if (index >= 0)
			groups.RemoveAt(index);
	}

	public void SetName(int warehouseIndex, string name)
	{
		if (warehouseIndex < 0)
			warehouseIndex = 0;

		while (Names.Count <= warehouseIndex)
			Names.Add(new ModWarehouseNameData());

		Names[warehouseIndex].Name = name;
		Names[warehouseIndex].Default = false;
	}

	public void ApplyToGame(global::Warehouse warehouse)
	{
		if (warehouse == null) return;

		warehouse.warehouseList = ToGameItemLists();
		warehouse.warehouseGroupList = ToGameGroupLists();
		warehouse.warehouseNamesData = ToGameNames();
		global::Warehouse.amountOfUnlockedWarehouses = AmountOfUnlockedWarehouses;
		warehouse.SelectedOption = SelectedOption;
		warehouse.UpdateWarehouseShelfLevel();
	}

	public void ApplyItemToGame(global::Warehouse warehouse, int warehouseIndex, ModItem item, bool add)
	{
		if (warehouse == null || item == null) return;

		var items = EnsureGameItemList(warehouse, warehouseIndex);
		var existingIndex = FindGameItemIndex(items, item.UID);
		if (add)
		{
			if (existingIndex < 0)
				items.Add(item.ToGame());
		}
		else if (existingIndex >= 0)
		{
			items.RemoveAt(existingIndex);
		}
	}

	public void ApplyGroupItemToGame(global::Warehouse warehouse, int warehouseIndex, ModGroupItem item, bool add)
	{
		if (warehouse == null || item == null) return;

		var groups = EnsureGameGroupList(warehouse, warehouseIndex);
		var existingIndex = FindGameGroupIndex(groups, item.UID);
		if (add)
		{
			if (existingIndex < 0)
				groups.Add(item.ToGame());
		}
		else if (existingIndex >= 0)
		{
			groups.RemoveAt(existingIndex);
		}
	}

	private List<ModItem> EnsureItems(int warehouseIndex)
	{
		if (warehouseIndex < 0)
			warehouseIndex = 0;

		while (Items.Count <= warehouseIndex)
			Items.Add(new List<ModItem>());

		return Items[warehouseIndex];
	}

	private List<ModGroupItem> EnsureGroups(int warehouseIndex)
	{
		if (warehouseIndex < 0)
			warehouseIndex = 0;

		while (GroupItems.Count <= warehouseIndex)
			GroupItems.Add(new List<ModGroupItem>());

		return GroupItems[warehouseIndex];
	}

	private static int FindItemIndex(List<ModItem> items, long uid)
	{
		for (var i = 0; i < items.Count; i++)
			if (items[i] != null && items[i].UID == uid)
				return i;

		return -1;
	}

	private static int FindGroupIndex(List<ModGroupItem> groups, long uid)
	{
		for (var i = 0; i < groups.Count; i++)
			if (groups[i] != null && groups[i].UID == uid)
				return i;

		return -1;
	}

	private Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.List<Item>> ToGameItemLists()
	{
		var gameLists = new Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.List<Item>>();
		foreach (var warehouseItems in Items)
		{
			var gameItems = new Il2CppSystem.Collections.Generic.List<Item>();
			foreach (var item in warehouseItems)
				if (item != null)
					gameItems.Add(item.ToGame());

			gameLists.Add(gameItems);
		}

		return gameLists;
	}

	private Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.List<GroupItem>> ToGameGroupLists()
	{
		var gameLists = new Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.List<GroupItem>>();
		foreach (var warehouseGroups in GroupItems)
		{
			var gameGroups = new Il2CppSystem.Collections.Generic.List<GroupItem>();
			foreach (var group in warehouseGroups)
				if (group != null)
					gameGroups.Add(group.ToGame());

			gameLists.Add(gameGroups);
		}

		return gameLists;
	}

	private Il2CppReferenceArray<WarehouseNameData> ToGameNames()
	{
		var names = new Il2CppReferenceArray<WarehouseNameData>(Names.Count);
		for (var i = 0; i < Names.Count; i++)
			names[i] = Names[i]?.ToGame();

		return names;
	}

	private static Il2CppSystem.Collections.Generic.List<Item> EnsureGameItemList(global::Warehouse warehouse, int warehouseIndex)
	{
		if (warehouseIndex < 0)
			warehouseIndex = 0;

		if (warehouse.warehouseList == null)
			warehouse.warehouseList = new Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.List<Item>>();

		while (warehouse.warehouseList.Count <= warehouseIndex)
			warehouse.warehouseList.Add(new Il2CppSystem.Collections.Generic.List<Item>());

		return warehouse.warehouseList[warehouseIndex];
	}

	private static Il2CppSystem.Collections.Generic.List<GroupItem> EnsureGameGroupList(global::Warehouse warehouse, int warehouseIndex)
	{
		if (warehouseIndex < 0)
			warehouseIndex = 0;

		if (warehouse.warehouseGroupList == null)
			warehouse.warehouseGroupList = new Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.List<GroupItem>>();

		while (warehouse.warehouseGroupList.Count <= warehouseIndex)
			warehouse.warehouseGroupList.Add(new Il2CppSystem.Collections.Generic.List<GroupItem>());

		return warehouse.warehouseGroupList[warehouseIndex];
	}

	private static int FindGameItemIndex(Il2CppSystem.Collections.Generic.List<Item> items, long uid)
	{
		for (var i = 0; i < items.Count; i++)
			if (items[i] != null && items[i].UID == uid)
				return i;

		return -1;
	}

	private static int FindGameGroupIndex(Il2CppSystem.Collections.Generic.List<GroupItem> groups, long uid)
	{
		for (var i = 0; i < groups.Count; i++)
			if (groups[i] != null && groups[i].UID == uid)
				return i;

		return -1;
	}
}
