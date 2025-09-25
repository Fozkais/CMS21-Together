using System.Collections.Generic;
using System.Linq;
using CMS.MainMenu.Controls;
using CMS21Together.Shared;
using UnityEngine;

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

	public static void SwitchPanelButton(Transform panel, bool disable)
	{
		List<MainMenuButton> buttons = panel.GetComponentsInChildren<MainMenuButton>().ToList();
		foreach (MainMenuButton btn in buttons)
		{
			bool hasListeners = btn.OnClick != null && btn.OnClick.m_Calls.Count > 0;

			if (!disable && hasListeners)
				btn.SetDisabled(false, true);
			else
				btn.SetDisabled(true, true);
		}
	}

}