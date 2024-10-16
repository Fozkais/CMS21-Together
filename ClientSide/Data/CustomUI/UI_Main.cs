using System;
using CMS.MainMenu.Controls;
using CMS.MainMenu.Sections;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CMS21Together.ClientSide.Data.CustomUI;

public class UI_Main
{
	public static void InitializeMainMenu()
	{
		CustomUIManager.V_Main_Parent = GameObject.Find("MainButtons").GetComponent<MainSection>().transform;

		var V_Main_Buttons = CustomUIManager.V_Main_Parent.GetComponentsInChildren<MainMenuButton>();
		foreach (var button in V_Main_Buttons)
		{
			var buttonState = new ButtonState
			{
				button = button,
				Disabled = false
			};

			CustomUIBuilder.AddButtonToSection(buttonState, CustomUISection.V_Main);
		}

		var playTransform = CustomUIManager.templateButton.GetComponent<RectTransform>();
		playTransform.anchoredPosition = new Vector2(-26, 58);
		playTransform.sizeDelta = new Vector2(180, 44);

		var b1_texture = DataHelper.LoadCustomTexture("CMS21Together.Assets.peoples.png");
		var b1_sprite = Sprite.Create(b1_texture, new Rect(0, 0, b1_texture.width, b1_texture.height), new Vector2(0, 0));

		var buttonImage = new ButtonImage();
		buttonImage.scale = new Vector3(.4f, .4f, .4f);
		buttonImage.sprite = b1_sprite;

		var b1_pos = new Vector2(95, 58);
		var b1_size = new Vector2(45, 45);
		Action b1_action = delegate { OpenMultiplayerMenu(); };
		var b1_info = new ButtonInfo(b1_pos, b1_size, b1_action, "", 0);
		CustomUIBuilder.CreateNewButton(CustomUISection.V_Main, b1_info, false, buttonImage);
	}

	public static void InitializeMultiplayerMenu()
	{
		var parent = new GameObject("MP_MainButtons");
		parent.transform.parent = GameObject.Find("MainButtons").transform.parent;
		parent.transform.localPosition = new Vector3(-380, 0, 0);
		parent.transform.localScale = new Vector3(.65f, .65f, .65f);

		CustomUIManager.MP_Main_Parent = parent.transform;

		var b1_pos = new Vector2(20, 58);
		var b1_size = new Vector2(336, 65);
		Action b1_action = delegate { OpenHostMenu(); };
		var b1_info = new ButtonInfo(b1_pos, b1_size, b1_action, "Host a game", 0);
		CustomUIBuilder.CreateNewButton(CustomUISection.MP_Main, b1_info, false);

		var b2_pos = new Vector2(20, -17);
		var b2_size = new Vector2(336, 65);
		Action b2_action = delegate { JoinWindow(); };
		var b2_info = new ButtonInfo(b2_pos, b2_size, b2_action, "Join a game", 1);
		CustomUIBuilder.CreateNewButton(CustomUISection.MP_Main, b2_info, false);

		var b3_pos = new Vector2(20, -92);
		var b3_size = new Vector2(336, 65);
		Action b3_action = delegate { ChangeNetworkType(); };
		var b3_info = new ButtonInfo(b3_pos, b3_size, b3_action, $"Network type: {ClientData.UserData.selectedNetworkType}", 2);
		CustomUIBuilder.CreateNewButton(CustomUISection.MP_Main, b3_info, false);

		var b4_pos = new Vector2(20, -242);
		var b4_size = new Vector2(336, 65);
		Action b4_action = delegate { };
		var b4_info = new ButtonInfo(b4_pos, b4_size, b4_action, "Mod settings", 3);
		CustomUIBuilder.CreateNewButton(CustomUISection.MP_Main, b4_info, true);

		var b5_pos = new Vector2(20, -317);
		var b5_size = new Vector2(336, 65);
		Action b5_action = delegate { OpenMainMenu(); };
		var b5_info = new ButtonInfo(b5_pos, b5_size, b5_action, "Back to menu", 4);
		CustomUIBuilder.CreateNewButton(CustomUISection.MP_Main, b5_info, false);

		CustomUIManager.DisableUI(CustomUISection.MP_Main);
	}

	private static void OpenMultiplayerMenu()
	{
		CustomUIManager.DisableUI(CustomUISection.V_Main);
		CustomUIManager.EnableUI(CustomUISection.MP_Main);
	}

	private static void OpenHostMenu()
	{
		CustomUIManager.DisableUI(CustomUISection.MP_Main);
		CustomUIManager.EnableUI(CustomUISection.MP_Host);
	}

	private static void OpenMainMenu()
	{
		CustomUIManager.DisableUI(CustomUISection.MP_Main);
		CustomUIManager.EnableUI(CustomUISection.V_Main);
	}

	private static void OpenLobby()
	{
		CustomUIManager.DisableUI(CustomUISection.MP_Main);
		CustomUIManager.EnableUI(CustomUISection.MP_Lobby);
	}

	private static void ChangeNetworkType()
	{
		switch (ClientData.UserData.selectedNetworkType)
		{
			case NetworkType.Steam:
				ClientData.UserData.selectedNetworkType = NetworkType.TCP;
				break;
			case NetworkType.TCP:
				ClientData.UserData.selectedNetworkType = NetworkType.Steam;
				break;
		}

		CustomUIManager.MP_Main_Buttons[2].button.text.text = $"Network type: {ClientData.UserData.selectedNetworkType}";
		CustomUIManager.MP_Main_Buttons[2].button.text.OnEnable();
	}

	private static void JoinWindow()
	{
		CustomUIManager.LockUI(CustomUIManager.currentSection);

		var position = new Vector2(600, 0);
		var size = new Vector2(600, 400);
		Action a1 = delegate
		{
			for (var i = 0; i < CustomUIBuilder.tmpWindow.Count; i++)
				Object.Destroy(CustomUIBuilder.tmpWindow[i]);

			CustomUIBuilder.tmpWindow.Clear();
			CustomUIManager.UnlockUI(CustomUIManager.currentSection);
		};
		Action a2 = delegate
		{
			var _index = CustomUIBuilder.tmpWindow.Count - 1;
			var inputFiel2 = CustomUIBuilder.tmpWindow[_index].GetComponentInChildren<InputField>();
			var inputFiel1 = CustomUIBuilder.tmpWindow[_index - 1].GetComponentInChildren<InputField>();
			var username = inputFiel2.text;
			var address = inputFiel1.text;

			ClientData.UserData.username = username;
			ClientData.UserData.ip = address;
			TogetherModManager.SavePreferences();

			if (!string.IsNullOrEmpty(ClientData.UserData.username))
			{
				Client.Instance.ConnectToServer(ClientData.UserData.selectedNetworkType);
				Application.runInBackground = true;

				OpenLobby();
			}
			else
			{
				return;
			}


			for (var i = 0; i < CustomUIBuilder.tmpWindow.Count; i++)
				Object.Destroy(CustomUIBuilder.tmpWindow[i]);

			CustomUIBuilder.tmpWindow.Clear();
			CustomUIManager.UnlockUI(CustomUIManager.currentSection);
		};

		CustomUIBuilder.CreateNewDoubleInputWindow(position, size, new[] { a1, a2 }, new[] { "Close", "Join" });
	}
}