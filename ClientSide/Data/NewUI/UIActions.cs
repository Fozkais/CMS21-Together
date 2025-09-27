using System;
using System.Linq;
using CMS.MainMenu.Controls;
using CMS.UI.Controls;
using CMS21Together.ClientSide.Data.CustomUI;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = Il2CppSystem.Object;

namespace CMS21Together.ClientSide.Data.NewUI;

public static class UIActions
{

	public static void StartClient(string username, string address)
	{
		ClientData.UserData.username = username;
		if (ClientData.UserData.selectedNetworkType != NetworkType.Steam)
			ClientData.UserData.ip = address;
		else
			ClientData.UserData.lobbyID = address;
		TogetherModManager.SavePreferences();
		
		Client.Instance.OnConnected += () =>
		{
			UICore.ShowPanel(UICore.MP_Lobby);
			UILobby.CreateLobby(false);
		};
		Client.Instance.OnDisconnected += () =>
		{
			UICore.ShowPanel(UICore.MP_Main);
			UICustomPanel.CreateInfoPanel("Failed to connect to server !");
		};
		Client.Instance.ConnectToServer(ClientData.UserData.selectedNetworkType, address);
	}
	
	public static void StartServer(string username, int save_index)
	{
		ClientData.UserData.username = username;
		TogetherModManager.SavePreferences();
		
		Client.Instance.OnConnected += () =>
		{
			UICore.ShowPanel(UICore.MP_Lobby);
			UILobby.CreateLobby(true, save_index);
		};
		Client.Instance.OnDisconnected += () =>
		{
			UICore.ShowPanel(UICore.MP_Main);
			UICustomPanel.CreateInfoPanel("Failed to connect to server !");
		};
		Server.Instance.StartServer(ClientData.UserData.selectedNetworkType);
		SavesManager.LoadSave(SavesManager.ModSaves[save_index]);
	}
	
	public static UnityAction ChangeNetworkType(MainMenuButton button)
	{
		Action action = () =>
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

			button.text.text = $"Network type: {ClientData.UserData.selectedNetworkType}";
			button.text.OnEnable();
		};
		return action;
	}

	public static UnityAction LoadGame(MainMenuButton button, int save_index)
	{
		Action action = () =>
		{
			if (UIUtils.GetSaveName(save_index) != "New game" && UICore.last_index_pressed != save_index)
				UICore.ShowCustomPanel(UICore.MP_Host.transform, UICustomPanelType.SaveInfo, button, save_index);
			else if (UIUtils.GetSaveName(save_index) != "New game" && UICore.last_index_pressed == save_index)
				UICore.ShowCustomPanel(UICore.MP_Host.transform, UICustomPanelType.JoinAsHostMenu, button, save_index);
			else
				UICore.ShowCustomPanel(UICore.MP_Host.transform,UICustomPanelType.CreateSave, button, save_index);
		};
		return action;
	}

	public static void CreateNewSave(InputField input, StringSelector selector, MainMenuButton btn, int index)
	{
		if (SavesManager.ModSaves.Any(s => s.Value.Name == input.text))
		{
			UICustomPanel.CreateInfoPanel("A save with the same name already exist.");
			return;
		}

		SavesManager.ModSaves[index].Name = input.text;
		SavesManager.ModSaves[index].selectedGamemode = SavesManager.GetGamemodeFromInt(selector.Current);
		btn.text.text = input.text;
		btn.text.OnEnable();
		SavesManager.SaveModSave(index);
		UnityEngine.Object.Destroy(UICore.TMP_Window);
	}

	public static void DeleteSave(MainMenuButton button, int save_index)
	{
		SavesManager.RemoveModSave(save_index);

		button.GetComponentInChildren<Text>().text = "New Game";
		button.OnEnable();
	}

	public static UnityAction SwitchReady(MainMenuButton btn)
	{
		Action action = () =>
		{
			foreach (var i in ClientData.Instance.connectedClients.Keys)
			{
				var player = ClientData.Instance.connectedClients[i];
				if (player != null)
					if (player.playerID == ClientData.UserData.playerID)
					{
						player.isReady = !player.isReady;
						ClientSend.ReadyPacket(player.isReady, i);
						if (player.isReady)
							btn.text.text = "Unready";
						else
							btn.text.text = "Ready Up";
						btn.text.OnEnable();
					}
			}
		};
		return action;
	}
}