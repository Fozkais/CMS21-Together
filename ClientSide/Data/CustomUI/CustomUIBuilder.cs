using System;
using System.Collections.Generic;
using CMS.MainMenu.Controls;
using CMS.UI.Controls;
using CMS.UI.Logic;
using CMS21Together.ServerSide;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using Il2CppSystem.Globalization;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CMS21Together.ClientSide.Data.CustomUI;

public static class CustomUIBuilder
{
	private static int CurrentButtonIndex = 6;

	public static List<GameObject> tmpWindow = new();
	public static List<GameObject> tmpWindow2 = new();

	public static void LoadCustomlogo()
	{
		var stream = DataHelper.LoadContent("CMS21Together.Assets.cms21TogetherLogo.png");

		var buffer = new byte[stream.Length];
		stream.Read(buffer, 0, (int)stream.Length);

		Object[] textures = Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<Texture2D>());
		if (textures.Length < 1) return;

		for (var index = 0; index < textures.Length; index++)
		{
			var texture = textures[index].TryCast<Texture2D>();

			if (texture != null)
				if (texture.name == "cms21Logo")
					ImageConversion.LoadImage(texture, buffer);
		}
	}


	public static void CreateSaveInfoPanel(ModSaveData saveData)
	{
		var saveInfoObject = new GameObject("SaveInfoWindow");
		tmpWindow.Add(saveInfoObject);

		var img = saveInfoObject.AddComponent<Image>();
		img.rectTransform.parent = GetParentFromSection(CustomUIManager.currentSection);
		img.rectTransform.parentInternal = GetParentFromSection(CustomUIManager.currentSection);
		img.color = new Color(.031f, .027f, .033f, 0.85f);
		img.rectTransform.sizeDelta = new Vector2(600, 420);
		img.rectTransform.anchoredPosition = new Vector2(460, -50);

		var t1_pos = new Vector2(620, 150);
		var t1_size = new Vector2(400, 100);
		CreateText(t1_pos, t1_size, "Save Info", 16, saveInfoObject.transform);

		var splitter1 = new GameObject("splitter");
		var splitter1Img = splitter1.AddComponent<Image>();
		splitter1Img.rectTransform.parent = saveInfoObject.transform;
		splitter1Img.rectTransform.parentInternal = saveInfoObject.transform;
		splitter1Img.color = new Color(1f, 1f, 1f, 0.5f);
		splitter1Img.rectTransform.sizeDelta = new Vector2(580, 2);
		splitter1Img.rectTransform.anchoredPosition = new Vector2(0, 120);

		string time;
		string lastSave;
		if (saveData.alreadyLoaded)
		{
			var timePlayed = TimeSpan.FromMinutes(SavesManager.profileData[saveData.saveIndex].PlayTime);
			if (timePlayed.TotalHours >= 1.0)
				time = $"{Math.Round(timePlayed.TotalHours)} h";
			else if (!(timePlayed.TotalMinutes < 1.0))
				time = $"{Math.Round(timePlayed.TotalMinutes)} min";
			else
				time = "1 min";

			var currentCulture = CultureInfo.CurrentCulture;
			CultureInfo.CurrentCulture = GlobalData.DefaultCultureInfo;
			lastSave = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(SavesManager.profileData[saveData.saveIndex].LastSave)).ToLocalTime().DateTime.ToString("g");
			CultureInfo.CurrentCulture = currentCulture;
		}
		else
		{
			time = "0 min";
			lastSave = "Never";
		}


		var t2_pos = new Vector2(620, 70);
		var t2_size = new Vector2(400, 100);
		CreateText(t2_pos, t2_size, "Name : " + saveData.Name, 14, saveInfoObject.transform);

		var t3_pos = new Vector2(620, 20);
		var t3_size = new Vector2(400, 100);
		CreateText(t3_pos, t3_size, "Gamemode : " + saveData.selectedGamemode, 14, saveInfoObject.transform);

		var t4_pos = new Vector2(620, -30);
		var t4_size = new Vector2(400, 100);
		CreateText(t4_pos, t4_size, "Time Played : " + time, 14, saveInfoObject.transform);

		var t5_pos = new Vector2(620, -70);
		var t5_size = new Vector2(400, 100);
		CreateText(t5_pos, t5_size, "Last save : " + lastSave, 14, saveInfoObject.transform);

		var splitter2 = Object.Instantiate(splitter1, saveInfoObject.transform, true);
		var splitter2Img = splitter2.GetComponent<Image>();
		splitter2Img.rectTransform.sizeDelta = new Vector2(580, 2);
		splitter2Img.rectTransform.anchoredPosition = new Vector2(0, -110);

		Action b1_action = delegate
		{
			CustomUIManager.LockUI(CustomUIManager.currentSection);

			var position = new Vector2(600, 0);
			var size = new Vector2(600, 300);
			Action a1 = delegate
			{
				for (var i = 0; i < tmpWindow2.Count; i++)
					Object.Destroy(tmpWindow2[i]);

				tmpWindow2.Clear();
				CustomUIManager.UnlockUI(CustomUIManager.currentSection);
			};
			Action a2 = delegate
			{
				//PreferencesManager.SavePreferences();

				SavesManager.RemoveModSave(saveData.saveIndex);

				CustomUIManager.MP_Saves_Buttons[saveData.saveIndex - 4].button.GetComponentInChildren<Text>().text = "New Game";
				CustomUIManager.MP_Saves_Buttons[saveData.saveIndex - 4].button.OnEnable();

				for (var i = 0; i < tmpWindow.Count; i++)
					Object.Destroy(tmpWindow[i]);
				for (var i = 0; i < tmpWindow2.Count; i++)
					Object.Destroy(tmpWindow2[i]);

				tmpWindow.Clear();
				tmpWindow2.Clear();
				CustomUIManager.UnlockUI(CustomUIManager.currentSection);
			};
			CreateNewInputWindow(position, size, new[] { a1, a2 }, new[] { "Cancel", "Confirm" }, InputFieldType.deleteSave);
		};
		var b1_pos = new Vector2(-10, -155);
		var b1_size = new Vector2(168, 65);
		var buttonInfo1 = new ButtonInfo(b1_pos, b1_size, b1_action, "Delete", -1, saveInfoObject.transform);
		tmpWindow.Add(CreateNewButton(CustomUIManager.currentSection, buttonInfo1, false, null, false).button.gameObject);
		tmpWindow[tmpWindow.Count - 1].SetActive(true);

		var splitter3 = Object.Instantiate(splitter1, saveInfoObject.transform, true);
		var splitter3Img = splitter3.GetComponent<Image>();
		splitter3Img.rectTransform.sizeDelta = new Vector2(580, 2);
		splitter3Img.rectTransform.anchoredPosition = new Vector2(0, -200);
	}

	public static void BuildLobbyHeader()
	{
		var lobbyHeaderObject = new GameObject("LobbyHeader");
		var img = lobbyHeaderObject.AddComponent<Image>();
		img.rectTransform.parent = GetParentFromSection(CustomUISection.MP_Lobby);
		img.rectTransform.parentInternal = GetParentFromSection(CustomUISection.MP_Lobby);

		img.color = new Color(.031f, .027f, .033f, 0.85f);

		CustomUIManager.MP_Lobby_Addition.Add((0, lobbyHeaderObject));

		if (Server.Instance.isRunning)
		{
			img.rectTransform.sizeDelta = new Vector2(600, 125);
			img.rectTransform.anchoredPosition = new Vector2(500, 186);

			var t1_pos = new Vector2(620, -30);
			var t1_size = new Vector2(400, 100);
			CreateText(t1_pos, t1_size, "Player", 16, lobbyHeaderObject.transform);

			var t2_pos = new Vector2(800, -30);
			var t2_size = new Vector2(400, 100);
			CreateText(t2_pos, t2_size, "Ready State", 16, lobbyHeaderObject.transform);

			var t3_pos = new Vector2(1060, -30);
			var t3_size = new Vector2(400, 100);
			CreateText(t3_pos, t3_size, "Ping", 16, lobbyHeaderObject.transform);

			var splitter2 = new GameObject("splitter");
			var splitter2Img = splitter2.AddComponent<Image>();
			splitter2Img.rectTransform.parent = lobbyHeaderObject.transform;
			splitter2Img.rectTransform.parentInternal = lobbyHeaderObject.transform;
			splitter2Img.color = new Color(1f, 1f, 1f, 0.5f);
			splitter2Img.rectTransform.sizeDelta = new Vector2(580, 2);
			splitter2Img.rectTransform.anchoredPosition = new Vector2(0, 0);

			var t4_pos = new Vector2(798, 30);
			var t4_size = new Vector2(600, 100);
			CreateText(t4_pos, t4_size, $"Save Name: {SavesManager.currentSave.Name}", 12, lobbyHeaderObject.transform);

			if (ClientData.UserData.selectedNetworkType == NetworkType.Steam)
			{
				var t5_pos = new Vector2(905, 30);
				var t5_size = new Vector2(400, 100);
				CreateText(t5_pos, t5_size, $"Server ID: {SteamLobby.lobbyID}", 14, lobbyHeaderObject.transform);
			}
		}
		else
		{
			img.rectTransform.sizeDelta = new Vector2(600, 75);
			img.rectTransform.anchoredPosition = new Vector2(500, 186);

			var t1_pos = new Vector2(620, 0);
			var t1_size = new Vector2(400, 100);
			CreateText(t1_pos, t1_size, "Player", 16, lobbyHeaderObject.transform);

			var t2_pos = new Vector2(800, 0);
			var t2_size = new Vector2(400, 100);
			CreateText(t2_pos, t2_size, "Ready State", 16, lobbyHeaderObject.transform);

			var t3_pos = new Vector2(1060, 0);
			var t3_size = new Vector2(400, 100);
			CreateText(t3_pos, t3_size, "Ping", 16, lobbyHeaderObject.transform);
		}

		var splitter1 = new GameObject("splitter");
		var splitter1Img = splitter1.AddComponent<Image>();
		splitter1Img.rectTransform.parent = lobbyHeaderObject.transform;
		splitter1Img.rectTransform.parentInternal = lobbyHeaderObject.transform;
		splitter1Img.color = new Color(1f, 1f, 1f, 0.5f);
		splitter1Img.rectTransform.sizeDelta = new Vector2(580, 2);
		splitter1Img.rectTransform.anchoredPosition = new Vector2(0, -62);

		CustomUIManager.MP_Lobby_Addition[0].Item2.SetActive(true);
	}

	public static StringSelector CreateNewSelector(Vector2 position, Vector2 size, string[] choices, Transform parent)
	{
		var selectorObject = Object.Instantiate(CustomUIManager.templateSelector);
		var rectTransform = selectorObject.GetComponent<RectTransform>();

		rectTransform.parent = parent;
		rectTransform.parentInternal = parent;

		rectTransform.anchoredPosition = position;
		rectTransform.sizeDelta = size;

		var selector = selectorObject.GetComponent<StringSelector>();
		selector.options = new Il2CppSystem.Collections.Generic.List<string>();
		for (var i = 0; i < choices.Length; i++)
			selector.options.Add(choices[i]);

		return selector;
	}

	public static void CreateNewInputWindow(Vector2 position, Vector2 size, Action[] actions, string[] texts, InputFieldType type)
	{
		var inputFieldObject = new GameObject("InputFieldWindow");
		var img = inputFieldObject.AddComponent<Image>();
		img.rectTransform.parent = GetParentFromSection(CustomUIManager.currentSection);
		img.rectTransform.parentInternal = GetParentFromSection(CustomUIManager.currentSection);

		img.color = new Color(.031f, .027f, .033f, 0.85f);
		img.rectTransform.sizeDelta = size;
		img.rectTransform.anchoredPosition = position;

		tmpWindow2.Add(inputFieldObject);

		var b1_pos = new Vector2(-200, -100);
		var b1_size = new Vector2(168, 65);
		var b1_action = actions[0];
		var buttonInfo1 = new ButtonInfo(b1_pos, b1_size, b1_action, texts[0], -1, inputFieldObject.transform);
		tmpWindow2.Add(CreateNewButton(CustomUIManager.currentSection, buttonInfo1, false, null, false).button.gameObject);

		var b2_pos = new Vector2(200, -100);
		var b2_size = new Vector2(168, 65);
		var b2_action = actions[1];
		var buttonInfo = new ButtonInfo(b2_pos, b2_size, b2_action, texts[1], -1, inputFieldObject.transform);
		tmpWindow2.Add(CreateNewButton(CustomUIManager.currentSection, buttonInfo, false, null, false).button.gameObject);


		if (type == InputFieldType.newSave)
		{
			var t1_pos = new Vector2(660, 130);
			var t1_size = new Vector2(400, 100);
			tmpWindow2.Add(CreateText(t1_pos, t1_size, "Enter save name :", 16, inputFieldObject.transform));

			var t2_pos = new Vector2(660, 5);
			var t2_size = new Vector2(400, 100);
			tmpWindow2.Add(CreateText(t2_pos, t2_size, "Gamemode :", 16, inputFieldObject.transform));

			var s1_pos = new Vector2(-180, 0);
			var s1_size = new Vector2(400, 100);
			var selector = CreateNewSelector(s1_pos, s1_size, new[] { "Sandbox", "Campaign" }, inputFieldObject.transform);
			tmpWindow2.Add(selector.gameObject);
			selector.gameObject.SetActive(true);
			selector.EnableArrows();
			selector.SetValue(0);
			selector.optionText.resizeTextMaxSize = 24;
			selector.optionText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-125, 2.5f);
			selector.rightArrow.GetComponent<RectTransform>().anchoredPosition = new Vector2(-250, 0);

			var i1_pos = new Vector2(0, 260);
			var i1_size = new Vector2(400, 100);
			tmpWindow2.Add(CreateNewInputField(i1_pos, i1_size, inputFieldObject.transform)); // need to be the last added
		}
		else if (type == InputFieldType.username)
		{
			var t1_pos = new Vector2(660, 100);
			var t1_size = new Vector2(400, 100);
			tmpWindow2.Add(CreateText(t1_pos, t1_size, "Enter username :", 16, inputFieldObject.transform));
			var i1_pos = new Vector2(0, 175);
			var i1_size = new Vector2(400, 100);
			tmpWindow2.Add(CreateNewInputField(i1_pos, i1_size, inputFieldObject.transform, ClientData.UserData.username)); // need to be the last added
		}
		else if (type == InputFieldType.deleteSave)
		{
			var t1_pos = new Vector2(800, 50);
			var t1_size = new Vector2(400, 100);
			tmpWindow2.Add(CreateText(t1_pos, t1_size, "Delete the save ?", 16, inputFieldObject.transform));
		}

		tmpWindow2[1].SetActive(true);
		tmpWindow2[2].SetActive(true);
	}

	public static void CreateNewDoubleInputWindow(Vector2 position, Vector2 size, Action[] actions, string[] texts)
	{
		var inputFieldObject = new GameObject("DoubleInputFieldWindow");
		var img = inputFieldObject.AddComponent<Image>();
		img.rectTransform.parent = GetParentFromSection(CustomUIManager.currentSection);
		img.rectTransform.parentInternal = GetParentFromSection(CustomUIManager.currentSection);

		img.color = new Color(.031f, .027f, .033f, 0.85f);
		img.rectTransform.sizeDelta = size;
		img.rectTransform.anchoredPosition = position;

		tmpWindow.Add(inputFieldObject);

		var b1_pos = new Vector2(-200, -150);
		var b1_size = new Vector2(168, 65);
		var b1_action = actions[0];
		var buttonInfo1 = new ButtonInfo(b1_pos, b1_size, b1_action, texts[0], -1, inputFieldObject.transform);
		tmpWindow.Add(CreateNewButton(CustomUIManager.currentSection, buttonInfo1, false, null, false).button.gameObject);

		var b2_pos = new Vector2(200, -150);
		var b2_size = new Vector2(168, 65);
		var b2_action = actions[1];
		var buttonInfo = new ButtonInfo(b2_pos, b2_size, b2_action, texts[1], -1, inputFieldObject.transform);
		tmpWindow.Add(CreateNewButton(CustomUIManager.currentSection, buttonInfo, false, null, false).button.gameObject);

		var t1_pos = new Vector2(635, 130);
		var t1_size = new Vector2(400, 100);
		if (ClientData.UserData.selectedNetworkType != NetworkType.Steam)
			tmpWindow.Add(CreateText(t1_pos, t1_size, "Enter server address :", 16, inputFieldObject.transform));
		else
			tmpWindow.Add(CreateText(t1_pos, t1_size, "Enter server ID :", 16, inputFieldObject.transform));

		var t2_pos = new Vector2(635, 0);
		var t2_size = new Vector2(400, 100);
		tmpWindow.Add(CreateText(t2_pos, t2_size, "Enter username :", 16, inputFieldObject.transform));

		var i1_pos = new Vector2(0, 250);
		var i1_size = new Vector2(400, 100);
		if (ClientData.UserData.selectedNetworkType != NetworkType.Steam)
			tmpWindow.Add(CreateNewInputField(i1_pos, i1_size, inputFieldObject.transform, ClientData.UserData.ip)); // need to be the last added
		else
			tmpWindow.Add(CreateNewInputField(i1_pos, i1_size, inputFieldObject.transform, ClientData.UserData.lobbyID));

		var i2_pos = new Vector2(0, 130);
		var i2_size = new Vector2(400, 100);
		tmpWindow.Add(CreateNewInputField(i2_pos, i2_size, inputFieldObject.transform, ClientData.UserData.username));

		tmpWindow[1].SetActive(true);
		tmpWindow[2].SetActive(true);
	}

	public static GameObject CreateNewInputField(Vector2 position, Vector2 size, Transform parent, string defaultText = "")
	{
		var inputFieldObject = Object.Instantiate(CustomUIManager.templateInputField, parent, true);

		for (var i = 0; i < inputFieldObject.transform.childCount; i++)
		{
			var obj = inputFieldObject.transform.GetChild(i).gameObject;
			if (obj.name != "InputField")
				Object.Destroy(obj);
		}

		inputFieldObject.GetComponent<Image>().enabled = false;

		inputFieldObject.GetComponent<RectTransform>().anchoredPosition = position;
		inputFieldObject.GetComponent<RectTransform>().sizeDelta = size;
		inputFieldObject.GetComponentInChildren<InputField>().text = defaultText;

		/*MelonLogger.Msg($"ChildrenCount = {inputFieldObject.transform.childCount}");
		if (inputField != null)
		{
		    MelonLogger.Msg($"ChildrenCount = {inputField.transform.childCount}");
		    var placeHolder = inputField.GetComponentsInChildren<Text>()[0].gameObject;
		    if (placeHolder != null)
		    {
		        placeHolder.GetComponent<Text>().text = placeholderText; // placeholderText was a parameter
		        placeHolder.SetActive(true);
		    }

		    inputField.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		}*/

		inputFieldObject.SetActive(true);
		return inputFieldObject;
	}

	public static GameObject CreateText(Vector2 position, Vector2 size, string text, int fontSize, Transform parent, TextAnchor anchor = TextAnchor.MiddleLeft)
	{
		var textObject = Object.Instantiate(CustomUIManager.templateText, parent, true);
		var textRect = textObject.GetComponent<RectTransform>();
		textRect.sizeDelta = size;
		textRect.localPosition = position;

		textObject.GetComponent<Text>().alignment = anchor;
		textObject.GetComponent<Text>().text = text;
		textObject.GetComponent<Text>().fontSize = fontSize;

		return textObject;
	}

	public static GameObject CreateImage(ButtonImage image, Transform parent)
	{
		var imgObject = new GameObject("Image");
		imgObject.transform.parent = parent;
		imgObject.AddComponent<Image>();
		imgObject.GetComponent<Image>().sprite = image.sprite;
		imgObject.transform.position = image.position;
		imgObject.transform.localPosition = image.position;
		imgObject.transform.localScale = image.scale;

		return imgObject;
	}

	public static ButtonState CreateNewButton(CustomUISection section, ButtonInfo buttonInfo, bool disabled, ButtonImage image = null, bool save = true)
	{
		var buttonObject = Object.Instantiate(CustomUIManager.templateButton);
		var buttonTransform = buttonObject.GetComponent<RectTransform>();
		var button = buttonObject.GetComponent<MainMenuButton>();

		if (buttonInfo.customParent == null)
		{
			buttonTransform.parent = GetParentFromSection(section);
			buttonTransform.parentInternal = GetParentFromSection(section);
		}
		else
		{
			buttonTransform.parent = buttonInfo.customParent;
			buttonTransform.parentInternal = buttonInfo.customParent;
		}

		button.Y = CurrentButtonIndex;
		CurrentButtonIndex += 1;

		buttonTransform.anchoredPosition = buttonInfo.position;
		buttonTransform.sizeDelta = buttonInfo.size;
		/*button.OnMouseHover = CustomUIManager.templateButton.GetComponent<MainMenuButton>().OnMouseHover; TODO: Fix MouseHover ?*/

		button.OnClick = new MainMenuButton.ButtonEvent();
		button.OnClick.AddListener(buttonInfo.action);

		if (image != null)
		{
			buttonObject.GetComponentInChildren<Text>().gameObject.SetActive(false);
			CreateImage(image, buttonObject.transform);
		}
		else
		{
			buttonObject.GetComponentInChildren<Text>().text = buttonInfo.text;
		}

		if (disabled)
		{
			button.DoStateTransition(SelectionState.Disabled, true);
			button.SetDisabled(true, true);
		}

		var buttonState = new ButtonState
		{
			button = button,
			Disabled = disabled
		};

		buttonObject.transform.localScale = new Vector3(1, 1, 1);

		if (save)
			AddButtonToSection(buttonState, section);

		return buttonState;
	}

	public static void AddButtonToSection(ButtonState buttonState, CustomUISection section)
	{
		if (section == CustomUISection.V_Main)
			CustomUIManager.V_Main_Buttons.Add(buttonState);
		else if (section == CustomUISection.MP_Main)
			CustomUIManager.MP_Main_Buttons.Add(buttonState);
		else if (section == CustomUISection.MP_Host)
			CustomUIManager.MP_Host_Buttons.Add(buttonState);
		else if (section == CustomUISection.MP_Lobby)
			CustomUIManager.MP_Lobby_Buttons.Add(buttonState);
		else if (section == CustomUISection.MP_Saves)
			CustomUIManager.MP_Saves_Buttons.Add(buttonState);
		else
			MelonLogger.Msg("[CustomUIBuilder->AddButtonToSection] section is not valid.");
	}

	public static Transform GetParentFromSection(CustomUISection section)
	{
		if (section == CustomUISection.V_Main)
			return CustomUIManager.V_Main_Parent;
		if (section == CustomUISection.MP_Main)
			return CustomUIManager.MP_Main_Parent;
		if (section == CustomUISection.MP_Host)
			return CustomUIManager.MP_Host_Parent;
		if (section == CustomUISection.MP_Lobby)
			return CustomUIManager.MP_Lobby_Parent;
		if (section == CustomUISection.MP_Saves)
			return CustomUIManager.MP_Saves_Parent;
		MelonLogger.Msg("[CustomUIBuilder->GetParentFromSection] section is not valid.");

		return null;
	}
}

public enum InputFieldType
{
	newSave,
	username,
	deleteSave
}

public class ButtonImage
{
	public Vector3 position;
	public Vector3 scale;
	public Sprite sprite;
}

public class ButtonInfo
{
	public Action action;
	public Transform customParent;
	public int index;
	public Vector2 position;
	public Vector2 size;
	public string text;

	public ButtonInfo(Vector2 _position, Vector2 _size, Action _action, string _text, int _index, Transform _customParent = null)
	{
		position = _position;
		size = _size;
		action = _action;
		text = _text;
		customParent = _customParent;
		index = _index;
	}
}