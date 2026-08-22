using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS.MainMenu.Controls;
using CMS.UI.Controls;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla.Cars;
using MelonLoader;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
			if (!Server.Instance.isRunning)
			{
				if (ClientData.UserData.selectedNetworkType == NetworkType.Steam)
					UILobby.CreateLobby(false, address);
				else
					UILobby.CreateLobby(false, "");
			}
		};
		Client.Instance.OnDisconnected += () =>
		{
			UICore.ShowPanel(UICore.MP_Main);
			UICustomPanel.CreateInfoPanel("Failed to connect to server !");
			if (Server.Instance.isRunning)
				MelonCoroutines.Start(Server.Instance.CloseServer());
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
			if (ClientData.UserData.selectedNetworkType == NetworkType.Steam)
				UILobby.CreateLobby(true, Server.Instance.serverID, save_index);
			else
				UILobby.CreateLobby(true, "", save_index);
		};
		Client.Instance.OnDisconnected += () =>
		{
			UICore.ShowPanel(UICore.MP_Main);
			UICustomPanel.CreateInfoPanel("Failed to connect to server !");
		};
		Server.Instance.StartServer(ClientData.UserData.selectedNetworkType);
		SaveSystem.LoadGame(SaveSystem.Extensions[save_index], save_index);
	}
	
	public static UnityAction ChangeNetworkType(MainMenuButton button)
	{
		Action action = () =>
		{
			switch (ClientData.UserData.selectedNetworkType)
			{
				case NetworkType.Steam:
					ClientData.UserData.selectedNetworkType = NetworkType.DirectIP;
					break;
				case NetworkType.DirectIP:
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
		if (SaveSystem.Extensions.Any(s => s.Name == input.text))
		{
			UICustomPanel.CreateInfoPanel("A save with the same name already exist.");
			return;
		}

		SaveSystem.Extensions[index].Name = input.text;
		SaveSystem.Extensions[index].SelectedGamemode = SaveSystem.GetGamemodeFromInt(selector.Current);
		btn.text.text = input.text;
		btn.text.OnEnable();
		UnityEngine.Object.Destroy(UICore.TMP_Window);
	}

	public static void DeleteSave(MainMenuButton button, int save_index)
	{
		SaveSystem.DeleteSave(save_index);

		button.GetComponentInChildren<Text>().text = "New Game";
		button.OnEnable();
	}

	public static UnityAction PreviousSaves(MainMenuButton button, MainMenuButton next_button)
	{
		Action action = () =>
		{
			if (UIMenu.save_btn_index == 0)
			{
				button.SetDisabled(true, true);
				return;
			}
			UIMenu.save_btn_index -= 4;

			UIUtils.DestroySavesButton();
			Vector2 b_pos = new Vector2(0, 344);

			int i = UIMenu.save_btn_index;
			while (i < UIMenu.save_btn_index + 4 && i < 16)
			{
				var saveBtn = UIElements.CreateButton(UICore.MP_Host.transform, UIUtils.GetSaveName(i + 4), null);
				var saveRect = saveBtn.GetComponent<RectTransform>();
				saveRect.anchorMin = new Vector2(0f, 0.5f);
				saveRect.anchorMax = new Vector2(0f, 0.5f);
				saveRect.pivot = new Vector2(0f, 0.5f);
				saveRect.sizeDelta = new Vector2(233, 44);
				saveRect.anchoredPosition = b_pos;
				saveBtn.transform.SetSiblingIndex(i % 4);
				
				saveBtn.OnClick.AddListener(UIActions.LoadGame(saveBtn, i + 4));
				saveBtn.SetLocked(false);
				saveBtn.SetDisabled(false, true);
				
				b_pos.y -= 49;
				i++;
			}

			if (UIMenu.save_btn_index == 0)
				button.SetDisabled(true, true);
			else
				button.SetDisabled(false, true);
			
			next_button.SetLocked(false);
			next_button.SetDisabled(false);
		};
		return action;
	}
	
	public static UnityAction NextSaves(MainMenuButton button, MainMenuButton prev_button)
	{
		Action action = () =>
		{
			if (UIMenu.save_btn_index >= 12)
			{
				button.SetDisabled(true, true);
				return;
			}
			UIMenu.save_btn_index += 4;

			UIUtils.DestroySavesButton();
			Vector2 b_pos = new Vector2(0, 344);

			int i = UIMenu.save_btn_index;
			while (i < UIMenu.save_btn_index + 4 && i < 16)
			{
				var saveBtn = UIElements.CreateButton(UICore.MP_Host.transform, UIUtils.GetSaveName(i + 4), null);
				var saveRect = saveBtn.GetComponent<RectTransform>();
				saveRect.anchorMin = new Vector2(0f, 0.5f);
				saveRect.anchorMax = new Vector2(0f, 0.5f);
				saveRect.pivot = new Vector2(0f, 0.5f);
				saveRect.sizeDelta = new Vector2(233, 44);
				saveRect.anchoredPosition = b_pos;
				saveBtn.transform.SetSiblingIndex(i % 4);
				
				saveBtn.OnClick.AddListener(UIActions.LoadGame(saveBtn, i + 4));
				saveBtn.SetLocked(false);
				saveBtn.SetDisabled(false, true);
				
				b_pos.y -= 49;
				i++;
			}

			if (UIMenu.save_btn_index >= 12)
				button.SetDisabled(true, true);
			else
				button.SetDisabled(false, true);
			
			prev_button.SetLocked(false);
			prev_button.SetDisabled(false);
		};
		return action;
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

	public static void StartGame(int save_index)
	{
		if (ServerData.Instance == null) return;

		foreach (var player in ServerData.Instance.connectedClients.Values)
		{
			if (player != null && !player.isReady)
			{
				UICustomPanel.CreateInfoPanel("All player are not ready.");
				return;
			}
		}
		MelonCoroutines.Start(GameLaunch(save_index));
	}

	private static IEnumerator GameLaunch(int save_index)
	{
		yield return new WaitForEndOfFrame();
		
		SaveSystem.StartGame(save_index);
		int i = 0;
		Dictionary<int, ModNewCarData> parksCars = new Dictionary<int, ModNewCarData>();
		foreach (NewCarData carData in SaveSystem.selectedSave.carsOnParking)
		{
			if (carData != null && !String.IsNullOrEmpty(carData.carToLoad))
			{
				parksCars.Add(i, new ModNewCarData(carData));
			}
			i++;	
		}
		ServerSend.StartPacket(SaveSystem.Extensions[save_index].SelectedGamemode, parksCars);
		if (SaveSystem.Extensions[save_index].AdditionnalStand != null)
			ServerData.Instance.engineStand2 = SaveSystem.Extensions[save_index].AdditionnalStand;
	}
}