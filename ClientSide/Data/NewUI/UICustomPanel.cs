using System;
using CMS.MainMenu.Controls;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Globalization;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CMS21Together.ClientSide.Data.NewUI;

public static class UICustomPanel
{
	public static void CreateSplitter(Transform parent, Vector2 pos, Vector2 size)
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
	
	public static void CreateSaveInfoPanel(MainMenuButton button, int index)
	{
		UICore.last_index_pressed = index;
		if (UICore.TMP_Window)
			Object.Destroy(UICore.TMP_Window);
		UICore.TMP_Window = new GameObject("SaveInfoWindow");
		UICore.TMP_Window.transform.SetParent(UICore.UI_Main.transform, false);

		var panelRect = UICore.TMP_Window.AddComponent<RectTransform>();
		panelRect.anchorMin = new Vector2(0.5f, 0.5f);
		panelRect.anchorMax = new Vector2(0.5f, 0.5f);
		panelRect.pivot = new Vector2(0.5f, 0.5f);
		panelRect.sizeDelta = new Vector2(400, 250);
		panelRect.anchoredPosition = Vector2.zero;

		var img = UICore.TMP_Window.AddComponent<Image>();
		img.color = new Color(.031f, .027f, .033f, 0.85f);
		
		var saveTxt = UIElements.CreateText(UICore.TMP_Window.transform, "Save Info", 24);
		var saveTxtRect = saveTxt.GetComponent<RectTransform>();
		saveTxtRect.anchorMin = new Vector2(0.5f, 1f);
		saveTxtRect.anchorMax = new Vector2(0.5f, 1f);
		saveTxtRect.pivot = new Vector2(0.5f, 1f);
		saveTxtRect.sizeDelta = new Vector2(230, 45);
		saveTxtRect.anchoredPosition = new Vector2(-50, 0);

		CreateSplitter(UICore.TMP_Window.transform, new Vector2(0, -40), new(390, 2));

		string lastSave = "Never";
		string time = "0 min";
		ModSaveData data = SavesManager.ModSaves[index];
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
		var nameTxt = UIElements.CreateText(UICore.TMP_Window.transform, "Name : " + data?.Name, 20);
		var nameTxtRect = nameTxt.GetComponent<RectTransform>();
		nameTxtRect.anchorMin = new Vector2(0f, 1f);
		nameTxtRect.anchorMax = new Vector2(0f, 1f);
		nameTxtRect.pivot = new Vector2(0f, 1f);
		nameTxtRect.sizeDelta = new Vector2(230, 45);
		nameTxtRect.anchoredPosition = new Vector2(10, -50);
		
		var gmTxt = UIElements.CreateText(UICore.TMP_Window.transform, "Gamemode : " + data?.selectedGamemode, 20);
		var gmTxtRect = gmTxt.GetComponent<RectTransform>();
		gmTxtRect.anchorMin = new Vector2(0f, 1f);
		gmTxtRect.anchorMax = new Vector2(0f, 1f);
		gmTxtRect.pivot = new Vector2(0f, 1f);
		gmTxtRect.sizeDelta = new Vector2(230, 45);
		gmTxtRect.anchoredPosition = new Vector2(10, -80);
		
		var timeTxt = UIElements.CreateText(UICore.TMP_Window.transform, "Time Played : " + time, 20);
		var timeTxtRect = timeTxt.GetComponent<RectTransform>();
		timeTxtRect.anchorMin = new Vector2(0f, 1f);
		timeTxtRect.anchorMax = new Vector2(0f, 1f);
		timeTxtRect.pivot = new Vector2(0f, 1f);
		timeTxtRect.sizeDelta = new Vector2(230, 45);
		timeTxtRect.anchoredPosition = new Vector2(10, -112.5f);
		
		var lsaveTxt = UIElements.CreateText(UICore.TMP_Window.transform, "Last save : " + lastSave, 20);
		var lsaveTxtRect = lsaveTxt.GetComponent<RectTransform>();
		lsaveTxtRect.anchorMin = new Vector2(0f, 1f);
		lsaveTxtRect.anchorMax = new Vector2(0f, 1f);
		lsaveTxtRect.pivot = new Vector2(0f, 1f);
		lsaveTxtRect.sizeDelta = new Vector2(230, 45);
		lsaveTxtRect.anchoredPosition = new Vector2(10, -140);
		
		CreateSplitter(UICore.TMP_Window.transform, new Vector2(0, -185), new(390, 2));
		
		var backBtn = UIElements.CreateButton(UICore.TMP_Window.transform, "Delete Save", 
		() => { UIActions.DeleteSave(button, index); Object.Destroy(UICore.TMP_Window); });
		var backRect = backBtn.GetComponent<RectTransform>();
		backRect.anchorMin = new Vector2(0.5f, 0f);
		backRect.anchorMax = new Vector2(0.5f, 0f);
		backRect.pivot = new Vector2(0.5f, 0f);
		backRect.sizeDelta = new Vector2(233, 44);
		backRect.anchoredPosition = new Vector2(0, 10);
	}

	public static void CreateInfoPanel(string msg)
	{
		UIUtils.SwitchPanelButton(UICore.Active_Panel.transform, true);
		if (UICore.TMP_Info_Window)
			Object.Destroy(UICore.TMP_Window);
		UICore.TMP_Info_Window = new GameObject("NoticeWindow");
		UICore.TMP_Info_Window.transform.SetParent(UICore.UI_Main.transform, false);

		var panelRect = UICore.TMP_Info_Window.AddComponent<RectTransform>();
		panelRect.anchorMin = new Vector2(0.5f, 0.5f);
		panelRect.anchorMax = new Vector2(0.5f, 0.5f);
		panelRect.pivot = new Vector2(0.5f, 0.5f);
		panelRect.sizeDelta = new Vector2(400, 200);
		panelRect.anchoredPosition = Vector2.zero;

		var img = UICore.TMP_Info_Window.AddComponent<Image>();
		img.color = new Color(.031f, .027f, .033f, 0.98f);
		
		
		var label = UIElements.CreateText(UICore.TMP_Info_Window.transform, "Notice", 24);
		var labelRect = label.GetComponent<RectTransform>();
		labelRect.anchorMin = new Vector2(1, 1f);
		labelRect.anchorMax = new Vector2(1, 1f);
		labelRect.pivot = new Vector2(1, 1f);
		labelRect.sizeDelta = new Vector2(230, 45);
		labelRect.anchoredPosition = new Vector2(-12, 0);
		
		CreateSplitter(UICore.TMP_Info_Window.transform, new Vector2(0, -40), new(390, 2));
		
		var txt = UIElements.CreateText(UICore.TMP_Info_Window.transform, msg, 18);
		txt.alignment = TextAnchor.MiddleCenter;
		var txtRect = txt.GetComponent<RectTransform>();
		txtRect.anchorMin = new Vector2(0.5f, 1f);
		txtRect.anchorMax = new Vector2(0.5f, 1f);
		txtRect.pivot = new Vector2(0.5f, 1f);
		txtRect.sizeDelta = new Vector2(230, 90);
		txtRect.anchoredPosition = new Vector2(0, -50);
		
		CreateSplitter(UICore.TMP_Info_Window.transform, new Vector2(0, -140), new(390, 2));
		
		var confirmBtn = UIElements.CreateButton(UICore.TMP_Info_Window.transform,
			"Confirm", (() => { UIUtils.SwitchPanelButton(UICore.Active_Panel.transform, false); Object.Destroy(UICore.TMP_Info_Window); }));
		var confirmRect = confirmBtn.GetComponent<RectTransform>();
		confirmRect.anchorMin = new Vector2(1f, 0f);
		confirmRect.anchorMax = new Vector2(1f, 0f);
		confirmRect.pivot = new Vector2(1f, 0f);
		confirmRect.sizeDelta = new Vector2(133, 44);
		confirmRect.anchoredPosition = new Vector2(-130, 5);
		
		MelonLogger.Msg("Notice : " + msg); 
	}

	public static void CreateNewSavePanel(MainMenuButton btn, int index)
	{
		if (UICore.TMP_Window)
			Object.Destroy(UICore.TMP_Window);
		UICore.TMP_Window = new GameObject("CreateSaveWindow");
		UICore.TMP_Window.transform.SetParent(UICore.UI_Main.transform, false);

		var panelRect = UICore.TMP_Window.AddComponent<RectTransform>();
		panelRect.anchorMin = new Vector2(0.5f, 0.5f);
		panelRect.anchorMax = new Vector2(0.5f, 0.5f);
		panelRect.pivot = new Vector2(0.5f, 0.5f);
		panelRect.sizeDelta = new Vector2(400, 250);
		panelRect.anchoredPosition = Vector2.zero;

		var img = UICore.TMP_Window.AddComponent<Image>();
		img.color = new Color(.031f, .027f, .033f, 0.85f);

		
		var saveTxt = UIElements.CreateText(UICore.TMP_Window.transform, "Create Save", 24);
		var saveTxtRect = saveTxt.GetComponent<RectTransform>();
		saveTxtRect.anchorMin = new Vector2(0.5f, 1f);
		saveTxtRect.anchorMax = new Vector2(0.5f, 1f);
		saveTxtRect.pivot = new Vector2(0.5f, 1f);
		saveTxtRect.sizeDelta = new Vector2(230, 45);
		saveTxtRect.anchoredPosition = new Vector2(-70, 0);

		CreateSplitter(UICore.TMP_Window.transform, new Vector2(0, -40), new(390, 2));
		
		var nameTxt = UIElements.CreateText(UICore.TMP_Window.transform, "Save Name : ", 22);
		var nameTxtRect = nameTxt.GetComponent<RectTransform>();
		nameTxtRect.anchorMin = new Vector2(0f, 1f);
		nameTxtRect.anchorMax = new Vector2(0f, 1f);
		nameTxtRect.pivot = new Vector2(0f, 1f);
		nameTxtRect.sizeDelta = new Vector2(230, 45);
		nameTxtRect.anchoredPosition = new Vector2(10, -50);
		
		var nameField = UIElements.CreateInput(UICore.TMP_Window.transform, "");
		nameField.transform.parent.GetChild(1).gameObject.SetActive(false);
		var nameFieldRect = nameField.transform.parent.GetComponent<RectTransform>();
		nameFieldRect.anchorMin = new Vector2(0f, 1f);
		nameFieldRect.anchorMax = new Vector2(0f, 1f);
		nameFieldRect.pivot = new Vector2(0f, 1f);
		nameFieldRect.sizeDelta = new Vector2(350, 42);
		nameFieldRect.anchoredPosition = new Vector2(20, -100);
		var nameFieldRect2 = nameField.GetComponent<RectTransform>();
		nameFieldRect2.anchorMin = new Vector2(0.01f, 1f);
		nameFieldRect2.anchorMax = new Vector2(0.99f, 0.7f);
		nameFieldRect2.sizeDelta = new Vector2(1, 46);
		nameFieldRect2.anchoredPosition = new Vector2(0, -15);
		
		var gmTxt = UIElements.CreateText(UICore.TMP_Window.transform, "Gamemode : ", 22);
		var gmTxtRect = gmTxt.GetComponent<RectTransform>();
		gmTxtRect.anchorMin = new Vector2(0f, 1f);
		gmTxtRect.anchorMax = new Vector2(0f, 1f);
		gmTxtRect.pivot = new Vector2(0f, 1f);
		gmTxtRect.sizeDelta = new Vector2(230, 45);
		gmTxtRect.anchoredPosition = new Vector2(10, -140);
		
		List<string> gamemodes = new List<string>();
		gamemodes.Add("Easy");
		gamemodes.Add("Normal");
		gamemodes.Add("Expert");
		gamemodes.Add("Sandbox");
		var gmSelec = UIElements.CreateSelector(UICore.TMP_Window.transform,  gamemodes);
		var gmSelecRect = gmSelec.GetComponent<RectTransform>();
		gmSelecRect.anchorMin = new Vector2(0f, 1f);
		gmSelecRect.anchorMax = new Vector2(0f, 1f);
		gmSelecRect.pivot = new Vector2(0f, 1f);
		gmSelecRect.sizeDelta = new Vector2(130, 65);
		gmSelecRect.anchoredPosition = new Vector2(130, -132);
		gmSelec.EnableArrows();
		gmSelec.SetValue(1);
		
		CreateSplitter(UICore.TMP_Window.transform, new Vector2(0, -185), new(390, 2));
		
		var backBtn = UIElements.CreateButton(UICore.TMP_Window.transform,
			"Cancel", (() => { if (UICore.TMP_Window) Object.Destroy(UICore.TMP_Window); }));
		var backRect = backBtn.GetComponent<RectTransform>();
		backRect.anchorMin = new Vector2(0.5f, 0f);
		backRect.anchorMax = new Vector2(0.5f, 0f);
		backRect.pivot = new Vector2(0.5f, 0f);
		backRect.sizeDelta = new Vector2(133, 44);
		backRect.anchoredPosition = new Vector2(-120, 10);
		
		var confirmBtn = UIElements.CreateButton(UICore.TMP_Window.transform,
			"Confirm", (() => { UIActions.CreateNewSave(nameField, gmSelec, btn, index); }));
		var confirmRect = confirmBtn.GetComponent<RectTransform>();
		confirmRect.anchorMin = new Vector2(0.5f, 0f);
		confirmRect.anchorMax = new Vector2(0.5f, 0f);
		confirmRect.pivot = new Vector2(0.5f, 0f);
		confirmRect.sizeDelta = new Vector2(133, 44);
		confirmRect.anchoredPosition = new Vector2(120, 10);
	}

	public static void JoinAsHostPanel(MainMenuButton btn, int index)
	{
		if (UICore.TMP_Window)
			Object.Destroy(UICore.TMP_Window);
		UICore.TMP_Window = new GameObject("HostJoinWindow");
		UICore.TMP_Window.transform.SetParent(UICore.UI_Main.transform, false);

		var panelRect = UICore.TMP_Window.AddComponent<RectTransform>();
		panelRect.anchorMin = new Vector2(0.5f, 0.5f);
		panelRect.anchorMax = new Vector2(0.5f, 0.5f);
		panelRect.pivot = new Vector2(0.5f, 0.5f);
		panelRect.sizeDelta = new Vector2(400, 230);
		panelRect.anchoredPosition = Vector2.zero;

		var img = UICore.TMP_Window.AddComponent<Image>();
		img.color = new Color(.031f, .027f, .033f, 0.85f);

		
		var saveTxt = UIElements.CreateText(UICore.TMP_Window.transform, "Create Lobby", 24);
		var saveTxtRect = saveTxt.GetComponent<RectTransform>();
		saveTxtRect.anchorMin = new Vector2(0.5f, 1f);
		saveTxtRect.anchorMax = new Vector2(0.5f, 1f);
		saveTxtRect.pivot = new Vector2(0.5f, 1f);
		saveTxtRect.sizeDelta = new Vector2(145, 45);
		saveTxtRect.anchoredPosition = new Vector2(-118, 0);

		CreateSplitter(UICore.TMP_Window.transform, new Vector2(0, -40), new(390, 2));
		
		var nameTxt = UIElements.CreateText(UICore.TMP_Window.transform, "Username : ", 22);
		var nameTxtRect = nameTxt.GetComponent<RectTransform>();
		nameTxtRect.anchorMin = new Vector2(0f, 1f);
		nameTxtRect.anchorMax = new Vector2(0f, 1f);
		nameTxtRect.pivot = new Vector2(0f, 1f);
		nameTxtRect.sizeDelta = new Vector2(230, 45);
		nameTxtRect.anchoredPosition = new Vector2(10, -50);
		
		var nameField = UIElements.CreateInput(UICore.TMP_Window.transform, "");
		nameField.transform.parent.GetChild(1).gameObject.SetActive(false);
		var nameFieldRect = nameField.transform.parent.GetComponent<RectTransform>();
		nameFieldRect.anchorMin = new Vector2(0f, 1f);
		nameFieldRect.anchorMax = new Vector2(0f, 1f);
		nameFieldRect.pivot = new Vector2(0f, 1f);
		nameFieldRect.sizeDelta = new Vector2(350, 42);
		nameFieldRect.anchoredPosition = new Vector2(20, -100);
		var nameFieldRect2 = nameField.GetComponent<RectTransform>();
		nameFieldRect2.anchorMin = new Vector2(0.01f, 1f);
		nameFieldRect2.anchorMax = new Vector2(0.99f, 0.7f);
		nameFieldRect2.sizeDelta = new Vector2(1, 46);
		nameFieldRect2.anchoredPosition = new Vector2(0, -15);
		
		CreateSplitter(UICore.TMP_Window.transform, new Vector2(0, -160), new(390, 2));
		
		var backBtn = UIElements.CreateButton(UICore.TMP_Window.transform,
			"Cancel", (() => { if (UICore.TMP_Window) Object.Destroy(UICore.TMP_Window); }));
		var backRect = backBtn.GetComponent<RectTransform>();
		backRect.anchorMin = new Vector2(0.5f, 0f);
		backRect.anchorMax = new Vector2(0.5f, 0f);
		backRect.pivot = new Vector2(0.5f, 0f);
		backRect.sizeDelta = new Vector2(133, 44);
		backRect.anchoredPosition = new Vector2(-120, 10);
		
		var confirmBtn = UIElements.CreateButton(UICore.TMP_Window.transform,
			"Confirm", (() =>
			{
				if (nameField.text == "")
				{
					UICustomPanel.CreateInfoPanel("Invalid username.");
					return;
				}
				UIActions.StartServer(nameField.text, index);
			}));
		var confirmRect = confirmBtn.GetComponent<RectTransform>();
		confirmRect.anchorMin = new Vector2(0.5f, 0f);
		confirmRect.anchorMax = new Vector2(0.5f, 0f);
		confirmRect.pivot = new Vector2(0.5f, 0f);
		confirmRect.sizeDelta = new Vector2(133, 44);
		confirmRect.anchoredPosition = new Vector2(120, 10);
	}

	public static void JoinPanel()
	{
		if (UICore.TMP_Window)
			Object.Destroy(UICore.TMP_Window);
		UICore.TMP_Window = new GameObject("JoinWindow");
		UICore.TMP_Window.transform.SetParent(UICore.UI_Main.transform, false);

		var panelRect = UICore.TMP_Window.AddComponent<RectTransform>();
		panelRect.anchorMin = new Vector2(0.5f, 0.5f);
		panelRect.anchorMax = new Vector2(0.5f, 0.5f);
		panelRect.pivot = new Vector2(0.5f, 0.5f);
		panelRect.sizeDelta = new Vector2(400, 330);
		panelRect.anchoredPosition = Vector2.zero;

		var img = UICore.TMP_Window.AddComponent<Image>();
		img.color = new Color(.031f, .027f, .033f, 0.85f);

		
		var saveTxt = UIElements.CreateText(UICore.TMP_Window.transform, "Join a Lobby", 24);
		var saveTxtRect = saveTxt.GetComponent<RectTransform>();
		saveTxtRect.anchorMin = new Vector2(0.5f, 1f);
		saveTxtRect.anchorMax = new Vector2(0.5f, 1f);
		saveTxtRect.pivot = new Vector2(0.5f, 1f);
		saveTxtRect.sizeDelta = new Vector2(145, 45);
		saveTxtRect.anchoredPosition = new Vector2(-118, 0);

		CreateSplitter(UICore.TMP_Window.transform, new Vector2(0, -40), new(390, 2));
		
		var nameTxt = UIElements.CreateText(UICore.TMP_Window.transform, "Username : ", 22);
		var nameTxtRect = nameTxt.GetComponent<RectTransform>();
		nameTxtRect.anchorMin = new Vector2(0f, 1f);
		nameTxtRect.anchorMax = new Vector2(0f, 1f);
		nameTxtRect.pivot = new Vector2(0f, 1f);
		nameTxtRect.sizeDelta = new Vector2(230, 45);
		nameTxtRect.anchoredPosition = new Vector2(10, -50);
		
		var nameField = UIElements.CreateInput(UICore.TMP_Window.transform, "");
		nameField.transform.parent.GetChild(1).gameObject.SetActive(false);
		var nameFieldRect = nameField.transform.parent.GetComponent<RectTransform>();
		nameFieldRect.anchorMin = new Vector2(0f, 1f);
		nameFieldRect.anchorMax = new Vector2(0f, 1f);
		nameFieldRect.pivot = new Vector2(0f, 1f);
		nameFieldRect.sizeDelta = new Vector2(350, 42);
		nameFieldRect.anchoredPosition = new Vector2(20, -100);
		var nameFieldRect2 = nameField.GetComponent<RectTransform>();
		nameFieldRect2.anchorMin = new Vector2(0.01f, 1f);
		nameFieldRect2.anchorMax = new Vector2(0.99f, 0.7f);
		nameFieldRect2.sizeDelta = new Vector2(1, 46);
		nameFieldRect2.anchoredPosition = new Vector2(0, -15);
		
		var addressTxt = UIElements.CreateText(UICore.TMP_Window.transform, "Server Address : ", 22);
		var addressTxtRect = addressTxt.GetComponent<RectTransform>();
		addressTxtRect.anchorMin = new Vector2(0f, 1f);
		addressTxtRect.anchorMax = new Vector2(0f, 1f);
		addressTxtRect.pivot = new Vector2(0f, 1f);
		addressTxtRect.sizeDelta = new Vector2(230, 45);
		addressTxtRect.anchoredPosition = new Vector2(10, -150);
		
		var addressField = UIElements.CreateInput(UICore.TMP_Window.transform, "");
		addressField.transform.parent.GetChild(1).gameObject.SetActive(false);
		var addressFieldRect = addressField.transform.parent.GetComponent<RectTransform>();
		addressFieldRect.anchorMin = new Vector2(0f, 1f);
		addressFieldRect.anchorMax = new Vector2(0f, 1f);
		addressFieldRect.pivot = new Vector2(0f, 1f);
		addressFieldRect.sizeDelta = new Vector2(350, 42);
		addressFieldRect.anchoredPosition = new Vector2(20, -200);
		var addressFieldRect2 = addressField.GetComponent<RectTransform>();
		addressFieldRect2.anchorMin = new Vector2(0.01f, 1f);
		addressFieldRect2.anchorMax = new Vector2(0.99f, 0.7f);
		addressFieldRect2.sizeDelta = new Vector2(1, 46);
		addressFieldRect2.anchoredPosition = new Vector2(0, -15);
		
		CreateSplitter(UICore.TMP_Window.transform, new Vector2(0, -260), new(390, 2));
		
		var backBtn = UIElements.CreateButton(UICore.TMP_Window.transform,
			"Cancel", (() => { if (UICore.TMP_Window) Object.Destroy(UICore.TMP_Window); }));
		var backRect = backBtn.GetComponent<RectTransform>();
		backRect.anchorMin = new Vector2(0.5f, 0f);
		backRect.anchorMax = new Vector2(0.5f, 0f);
		backRect.pivot = new Vector2(0.5f, 0f);
		backRect.sizeDelta = new Vector2(133, 44);
		backRect.anchoredPosition = new Vector2(-120, 10);
		
		var confirmBtn = UIElements.CreateButton(UICore.TMP_Window.transform,
			"Confirm", (() =>
			{
				if (nameField.text == "")
				{
					UICustomPanel.CreateInfoPanel("Invalid username.");
					return;
				}
				if (addressField.text == "")
				{
					UICustomPanel.CreateInfoPanel("Invalid server address.");
					return;
				}
				UIActions.StartClient(nameField.text, addressField.text);
			}));
		var confirmRect = confirmBtn.GetComponent<RectTransform>();
		confirmRect.anchorMin = new Vector2(0.5f, 0f);
		confirmRect.anchorMax = new Vector2(0.5f, 0f);
		confirmRect.pivot = new Vector2(0.5f, 0f);
		confirmRect.sizeDelta = new Vector2(133, 44);
		confirmRect.anchoredPosition = new Vector2(120, 10);
	}
}