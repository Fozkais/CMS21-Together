using System;

namespace CMS21Together.Shared.Data;

[Serializable]
public class ModSaveData
{
	public string Name;
	public int saveIndex;
	public Gamemode selectedGamemode = Gamemode.Sandbox;
	public bool alreadyLoaded;

	public ModSaveData(string saveName, Gamemode gamemode, int index, bool loaded)
	{
		Name = saveName;
		saveIndex = index;
		selectedGamemode = gamemode;
		alreadyLoaded = loaded;
	}

	public ModSaveData(string saveName, int index, bool loaded)
	{
		Name = saveName;
		saveIndex = index;
		selectedGamemode = Gamemode.Sandbox;
		alreadyLoaded = loaded;
	}

	public ModSaveData()
	{
	}
}