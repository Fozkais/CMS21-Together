using System;
using System.Collections.Generic;

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