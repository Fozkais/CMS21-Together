using System;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using Il2CppSystem.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace CMS21Together.ClientSide.Data.NewUI;

public static class UICustomPanel
{
	private static void CreateSplitter(Transform parent, Vector2 pos, Vector2 size)
	{
		var splitter = new GameObject("splitter");
		splitter.transform.SetParent(parent, false);
		var splitterImg = splitter.AddComponent<Image>();
		splitterImg.color = new Color(1f, 1f, 1f, 0.5f);
		splitterImg.rectTransform.anchorMin = new Vector2(0.5f, 1f);
		splitterImg.rectTransform.anchorMax = new Vector2(0.5f, 1f);
		splitterImg.rectTransform.pivot = new Vector2(0.5f, 1f);
		splitterImg.rectTransform.sizeDelta = size;
		splitterImg.rectTransform.anchoredPosition = pos;
	}
	
	public static void CreateSaveInfoPanel(ModSaveData data)
	{
		GameObject panel = new GameObject("SaveInfoWindow");
		panel.transform.SetParent(UICore.UI_Main.transform, false);

		var panelRect = panel.AddComponent<RectTransform>();
		panelRect.anchorMin = new Vector2(0.5f, 0.5f);
		panelRect.anchorMax = new Vector2(0.5f, 0.5f);
		panelRect.pivot = new Vector2(0.5f, 0.5f);
		panelRect.sizeDelta = new Vector2(400, 250);
		panelRect.anchoredPosition = Vector2.zero;

		var img = panel.AddComponent<Image>();
		img.color = new Color(.031f, .027f, .033f, 0.85f);
		
		// --- Title ---
		var saveTxt = UIElements.CreateText(panel.transform, "Save Info", 24);
		var saveTxtRect = saveTxt.GetComponent<RectTransform>();
		saveTxtRect.anchorMin = new Vector2(0.5f, 1f);
		saveTxtRect.anchorMax = new Vector2(0.5f, 1f);
		saveTxtRect.pivot = new Vector2(0.5f, 1f);
		saveTxtRect.sizeDelta = new Vector2(230, 45);
		saveTxtRect.anchoredPosition = new Vector2(-50, 0);

		CreateSplitter(panel.transform, new Vector2(0, -40), new(390, 2));

		string lastSave = "Never";
		string time = "0 min";
		if (data != null && data.alreadyLoaded)
		{
			var timePlayed = TimeSpan.FromMinutes(SavesManager.profileData[data.saveIndex].PlayTime);
			if (timePlayed.TotalHours >= 1)
				time = $"{Math.Round(timePlayed.TotalHours)} h";
			else if (timePlayed.TotalMinutes >= 1.0)
				time = $"{Math.Round(timePlayed.TotalMinutes)} min";
			else
				time = "less than 1 min";
			
			var currentCulture = CultureInfo.CurrentCulture;
			CultureInfo.CurrentCulture = GlobalData.DefaultCultureInfo;
			lastSave = DateTimeOffset.FromUnixTimeSeconds(
				Convert.ToInt64(SavesManager.profileData[data.saveIndex].LastSave)).ToLocalTime().DateTime.ToString("g");
			CultureInfo.CurrentCulture = currentCulture;
		}
		var nameTxt = UIElements.CreateText(panel.transform, "Name : " + data?.Name, 20);
		var nameTxtRect = nameTxt.GetComponent<RectTransform>();
		nameTxtRect.anchorMin = new Vector2(0f, 1f);
		nameTxtRect.anchorMax = new Vector2(0f, 1f);
		nameTxtRect.pivot = new Vector2(0f, 1f);
		nameTxtRect.sizeDelta = new Vector2(230, 45);
		nameTxtRect.anchoredPosition = new Vector2(10, -50);
		
		var gmTxt = UIElements.CreateText(panel.transform, "Gamemode : " + data?.selectedGamemode, 20);
		var gmTxtRect = gmTxt.GetComponent<RectTransform>();
		gmTxtRect.anchorMin = new Vector2(0f, 1f);
		gmTxtRect.anchorMax = new Vector2(0f, 1f);
		gmTxtRect.pivot = new Vector2(0f, 1f);
		gmTxtRect.sizeDelta = new Vector2(230, 45);
		gmTxtRect.anchoredPosition = new Vector2(10, -80);
		
		var timeTxt = UIElements.CreateText(panel.transform, "Time Played : " + time, 20);
		var timeTxtRect = timeTxt.GetComponent<RectTransform>();
		timeTxtRect.anchorMin = new Vector2(0f, 1f);
		timeTxtRect.anchorMax = new Vector2(0f, 1f);
		timeTxtRect.pivot = new Vector2(0f, 1f);
		timeTxtRect.sizeDelta = new Vector2(230, 45);
		timeTxtRect.anchoredPosition = new Vector2(10, -112.5f);
		
		var lsaveTxt = UIElements.CreateText(panel.transform, "Last save : " + lastSave, 20);
		var lsaveTxtRect = lsaveTxt.GetComponent<RectTransform>();
		lsaveTxtRect.anchorMin = new Vector2(0f, 1f);
		lsaveTxtRect.anchorMax = new Vector2(0f, 1f);
		lsaveTxtRect.pivot = new Vector2(0f, 1f);
		lsaveTxtRect.sizeDelta = new Vector2(230, 45);
		lsaveTxtRect.anchoredPosition = new Vector2(10, -140);
		
		CreateSplitter(panel.transform, new Vector2(0, -185), new(390, 2));

		// --- Button ---
		var backBtn = UIElements.CreateButton(panel.transform, "Delete Save", null);
		var backRect = backBtn.GetComponent<RectTransform>();
		backRect.anchorMin = new Vector2(0.5f, 0f);
		backRect.anchorMax = new Vector2(0.5f, 0f);
		backRect.pivot = new Vector2(0.5f, 0f);
		backRect.sizeDelta = new Vector2(233, 44);
		backRect.anchoredPosition = new Vector2(0, 10);
	}
}