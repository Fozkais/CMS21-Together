using System;
using System.Collections.Generic;
using MelonLoader;

namespace CMS21Together.Shared.Data.Vanilla;

[Serializable]
public class ModGroupItem
{
	public int Index;

	public bool IsNormalGroup;
	public List<ModItem> ItemList = new();
	public float Size;
	public string ID;
	public long UID;

	public ModGroupItem()
	{
	}

	public ModGroupItem(GroupItem item)
	{
		if (item != null)
		{
			IsNormalGroup = item.IsNormalGroup;
			ItemList = new List<ModItem>();
			if (item.ItemList != null)
				foreach (var _item in item.ItemList)
					if (_item != null)
						ItemList.Add(new ModItem(_item));

			Size = item.Size;
			ID = item.ID;
			UID = item.UID;
		}
		else
		{
			MelonLogger.Msg("Error: GroupItem is null in ModGroupItem constructor.");
		}
	}

	public GroupItem ToGame(ModGroupItem ModGroupItem)
	{
		var original = new GroupItem();

		original.IsNormalGroup = ModGroupItem.IsNormalGroup;
		original.ItemList = Convert(ModGroupItem.ItemList);
		original.Size = ModGroupItem.Size;
		original.ID = ModGroupItem.ID;
		original.UID = ModGroupItem.UID;

		return original;
	}

	public GroupItem ToGame()
	{
		var original = new GroupItem();

		original.IsNormalGroup = IsNormalGroup;
		original.ItemList = Convert(ItemList);
		original.Size = Size;
		original.ID = ID;
		original.UID = UID;

		return original;
	}

	public Il2CppSystem.Collections.Generic.List<Item> Convert(List<ModItem> items)
	{
		var FinalItem = new Il2CppSystem.Collections.Generic.List<Item>();
		foreach (var item in items) FinalItem.Add(item.ToGame());

		return FinalItem;
	}
}