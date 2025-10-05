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

	public static void DestroySavesButton()
	{
		int j = 0;
		while (j < 4)
		{
			if (UICore.MP_Host.transform.childCount <= j) break;
			
			var saveBtn = UICore.MP_Host.transform.GetChild(j);
			UnityEngine.Object.Destroy(saveBtn.gameObject);
			j++;
		}
	}
	
	public static void DestroyPanelButtons(Transform panel)
	{
		int j = 0;
		while (j < panel.childCount)
		{
			if (panel.childCount <= j) break;
			
			var btn = panel.transform.GetChild(j);
			Object.Destroy(btn.gameObject);
			j++;
		}
	}

}