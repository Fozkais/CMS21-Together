using System.Linq;
using CMS.MainMenu.Controls;
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
		var buttons = panel.GetComponentsInChildren<MainMenuButton>().ToList();
		foreach (var btn in buttons)
		{
			var hasListeners = btn.OnClick != null && btn.OnClick.m_Calls.Count > 0;

			if (!disable && hasListeners)
				btn.SetDisabled(false, true);
			else
				btn.SetDisabled(true, true);
		}
	}

	public static void DestroySavesButton()
	{
		var j = 0;
		while (j < 4)
		{
			if (UICore.MP_Host.transform.childCount <= j) break;

			var saveBtn = UICore.MP_Host.transform.GetChild(j);
			Object.Destroy(saveBtn.gameObject);
			j++;
		}
	}

	public static void DestroyPanelButtons(Transform panel)
	{
		var j = 0;
		while (j < panel.childCount)
		{
			if (panel.childCount <= j) break;

			var btn = panel.transform.GetChild(j);
			Object.Destroy(btn.gameObject);
			j++;
		}
	}
}