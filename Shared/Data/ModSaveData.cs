using System;
using System.Collections.Generic;
using Steamworks.Data;
using UnityEngine.Serialization;

namespace CMS21Together.Shared.Data;

[Serializable]
public class ModSaveData
{
	public string Name;
	public int saveIndex;
	public Gamemode selectedGamemode = Gamemode.Sandbox;
	public bool alreadyLoaded;
	public List<PlayerInfo> PlayerInfos = new List<PlayerInfo>();
	public long[] InventoryItemUID;
	public ModSaveData(string saveName, int index, bool loaded)
	{
		Name = saveName;
		saveIndex = index;
		selectedGamemode = Gamemode.Sandbox;
		alreadyLoaded = loaded;
		InventoryItemUID = new long[]
		{
			1000,
			10000000,
			20000000,
			30000000
		};
	}

	public ModSaveData()
	{
	}
}