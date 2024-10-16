using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CMS21Together.ClientSide.Data.CustomUI;

public static class UI_Lobby
{
	public static int saveIndex;
	private static Dictionary<int, GameObject> lobbyPlayer = new();

	public static void InitializeLobbyMenu()
	{
		var parent = new GameObject("MP_LobbyButtons");
		parent.transform.parent = GameObject.Find("MainButtons").transform.parent;
		parent.transform.localPosition = new Vector3(-380, 0, 0);
		parent.transform.localScale = new Vector3(.65f, .65f, .65f);

		CustomUIManager.MP_Lobby_Parent = parent.transform;

		var b1_pos = new Vector2(20, 58);
		var b1_size = new Vector2(336, 65);
		Action b1_action = delegate
		{
			foreach (var player in ServerData.Instance.connectedClients.Values)
				if (player != null && !player.isReady)
					return;
			MelonCoroutines.Start(StartGame());
		};
		var b1_info = new ButtonInfo(b1_pos, b1_size, b1_action, "Start game", 0);
		CustomUIBuilder.CreateNewButton(CustomUISection.MP_Lobby, b1_info, false);

		var b2_pos = new Vector2(20, -17);
		var b2_size = new Vector2(336, 65);
		Action b2_action = delegate
		{
			foreach (var i in ClientData.Instance.connectedClients.Keys)
			{
				var player = ClientData.Instance.connectedClients[i];
				if (player != null)
					if (player.playerID == ClientData.UserData.playerID)
					{
						player.isReady = !player.isReady;
						ClientSend.ReadyPacket(player.isReady, i);
						ChangeReadyState(i, player.isReady);
					}
			}
		};
		var b2_info = new ButtonInfo(b2_pos, b2_size, b2_action, "Toggle Ready", 1);
		CustomUIBuilder.CreateNewButton(CustomUISection.MP_Lobby, b2_info, false);

		var b3_pos = new Vector2(20, -317);
		var b3_size = new Vector2(336, 65);
		Action b3_action = delegate { OpenMainMenu(); };
		var b3_info = new ButtonInfo(b3_pos, b3_size, b3_action, "Disconnect", 2);
		CustomUIBuilder.CreateNewButton(CustomUISection.MP_Lobby, b3_info, false); // Need to be last

		CustomUIManager.DisableUI(CustomUISection.MP_Lobby);
	}

	public static void RemovePlayerFromLobby(int index)
	{
		if (CustomUIManager.MP_Lobby_Addition.Any(elt => elt.Item1 == index))
		{
			var elt = CustomUIManager.MP_Lobby_Addition.First(elt => elt.Item1 == index);
			Object.Destroy(elt.Item2);
			CustomUIManager.MP_Lobby_Addition.Remove(elt);
		}
	}

	public static void AddPlayerToLobby(string username, int index)
	{
		if (CustomUIManager.MP_Lobby_Addition.Any(elt => elt.Item1 == index)) return;

		var lobbyPlayerObject = new GameObject("LobbyPlayer");
		var img = lobbyPlayerObject.AddComponent<Image>();
		img.rectTransform.parent = CustomUIBuilder.GetParentFromSection(CustomUISection.MP_Lobby);
		img.rectTransform.parentInternal = CustomUIBuilder.GetParentFromSection(CustomUISection.MP_Lobby);

		var pos = new Vector2(500, 175 - CustomUIManager.MP_Lobby_Addition.Count * 75);

		img.color = new Color(.031f, .027f, .033f, 0.85f);
		img.rectTransform.sizeDelta = new Vector2(600, 75);
		img.rectTransform.anchoredPosition = pos;

		CustomUIManager.MP_Lobby_Addition.Add((index, lobbyPlayerObject));

		var t1_pos = new Vector2(620, 0);
		var t1_size = new Vector2(400, 100);
		CustomUIBuilder.CreateText(t1_pos, t1_size, username, 16, lobbyPlayerObject.transform);

		var t2_pos = new Vector2(800, -2);
		var t2_size = new Vector2(400, 100);
		CustomUIBuilder.CreateText(t2_pos, t2_size, "Not ready", 16, lobbyPlayerObject.transform);

		var t3_pos = new Vector2(1060, -2);
		var t3_size = new Vector2(400, 100);
		CustomUIBuilder.CreateText(t3_pos, t3_size, "?ms", 16, lobbyPlayerObject.transform);

		CustomUIManager.MP_Lobby_Addition[0].Item2.SetActive(true);
	}

	public static void ChangeReadyState(int index, bool ready)
	{
		if (ready)
			CustomUIManager.MP_Lobby_Addition[index].Item2.transform.GetChild(1).GetComponent<Text>().text = "Ready";
		else
			CustomUIManager.MP_Lobby_Addition[index].Item2.transform.GetChild(1).GetComponent<Text>().text = "Not Ready";
		CustomUIManager.MP_Lobby_Addition[index].Item2.transform.GetChild(1).GetComponent<Text>().OnEnable();
	}


	public static void OpenMainMenu()
	{
		CustomUIManager.DisableUI(CustomUISection.MP_Lobby);
		CustomUIManager.EnableUI(CustomUISection.MP_Main);

		if (Server.Instance.isRunning)
			Server.Instance.CloseServer();
		else
			Client.Instance.Disconnect();

		// if (!ModSceneManager.isInMenu())
		//  {
		//       NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("Menu", SceneType.Menu, true, false));
		//   }
	}

	private static IEnumerator StartGame()
	{
		yield return new WaitForEndOfFrame();

		StartGame(saveIndex);
		SavesManager.ModSaves[saveIndex].alreadyLoaded = true;
		SavesManager.SaveModSave(saveIndex);
	}

	private static void StartGame(int _saveIndex)
	{
		SavesManager.StartGame(_saveIndex);
		ServerSend.StartPacket(SavesManager.ModSaves[_saveIndex].selectedGamemode);
	}
}