using System;
using System.Collections.Generic;

namespace CMS21_Together_Core.Data.GameType;

[Serializable]
public class ModGroupItem
{
	public int Index;

	public bool IsNormalGroup;
	public List<ModItem> ItemList = new();
	public float Size;
	public string ID;
	public long UID;

}

