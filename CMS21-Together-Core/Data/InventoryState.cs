using System;
using System.Collections.Generic;
using CMS21_Together_Core.Data.GameType;

namespace CMS21_Together_Core.Data;

[Serializable]
public class InventoryState
{
	public List<ModItem> InventoryItems = new List<ModItem>();
	public List<ModGroupItem> InventoryGroupItems = new List<ModGroupItem>();
	
	public List<ModItem> WarehouseItems = new List<ModItem>();
	public List<ModGroupItem> WarehouseGroupItems = new List<ModGroupItem>();
}
