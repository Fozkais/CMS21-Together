using System;
using System.Collections.Generic;
using CMS21_Together_Core.Data.Vanilla.GarageTool;

namespace CMS21_Together_Core.Data;

[Serializable]
public class ModSaveData
{
	public string Name;
	public int saveIndex;
	public Gamemode selectedGamemode = Gamemode.Sandbox;
	public bool alreadyLoaded;
	public List<PlayerInfo> playerInfos = new List<PlayerInfo>();
	public long[] inventoryItemUID;
	public bool storyMissionInProgress;
	public int missionFinished;
	public int money;
	public ModEngineStand additionnalStand;
	
	public ModSaveData(string saveName, int index, bool loaded)
	{
		Name = saveName;
		saveIndex = index;
		selectedGamemode = Gamemode.Sandbox;
		alreadyLoaded = loaded;
		inventoryItemUID = new long[]
		{
			1000,
			10000000,
			20000000,
			30000000
		};
		storyMissionInProgress = false;
		missionFinished = 0;
		money = 0;
		additionnalStand = null;
	}

	public ModSaveData()
	{
	}
}