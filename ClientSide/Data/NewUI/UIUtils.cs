using CMS21Together.Shared;

namespace CMS21Together.ClientSide.Data.NewUI;

public static class UIUtils
{
	public static string GetSaveName(int index)
	{
		var validIndex = index;
		if (SavesManager.ModSaves.ContainsKey(validIndex))
			if (SavesManager.ModSaves[validIndex].Name != "EmptySave")
				return SavesManager.ModSaves[validIndex].Name;
		return "New game";
	}
}