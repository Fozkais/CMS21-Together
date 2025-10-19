using System.Linq;
using CMS21Together.ServerSide;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace CMS21Together.ClientSide.Data.NewUI;

public static class UILobby
{
	public static void CreateLobby(bool isHost, string serverID, int save_index=-1)
	{
		if (UICore.TMP_Window)
			Object.Destroy(UICore.TMP_Window);
		UICore.TMP_Window = new GameObject("LobbyWindow");
		UICore.TMP_Window.transform.SetParent(UICore.UI_Main.transform, false);

		var panelRect = UICore.TMP_Window.AddComponent<RectTransform>();
		panelRect.anchorMin = new Vector2(0.5f, 0.5f);
		panelRect.anchorMax = new Vector2(0.5f, 0.5f);
		panelRect.pivot = new Vector2(0.5f, 0.5f);
		panelRect.sizeDelta = new Vector2(450, 330);
		panelRect.anchoredPosition = new Vector2(20, 0);

		var img = UICore.TMP_Window.AddComponent<Image>();
		img.color = new Color(.031f, .027f, .033f, 0.85f);
		
		var saveTxt = UIElements.CreateText(UICore.TMP_Window.transform, "Game Lobby", 24);
		var saveTxtRect = saveTxt.GetComponent<RectTransform>();
		saveTxtRect.anchorMin = new Vector2(0.5f, 1f);
		saveTxtRect.anchorMax = new Vector2(0.5f, 1f);
		saveTxtRect.pivot = new Vector2(0.5f, 1f);
		saveTxtRect.sizeDelta = new Vector2(145, 45);
		saveTxtRect.anchoredPosition = new Vector2(-144, 0);
		
		var idTxt = UIElements.CreateText(UICore.TMP_Window.transform, "ID: " + serverID, 20);
		var idTxtRect = idTxt.GetComponent<RectTransform>();
		idTxtRect.anchorMin = new Vector2(0.5f, 1f);
		idTxtRect.anchorMax = new Vector2(0.5f, 1f);
		idTxtRect.pivot = new Vector2(0.5f, 1f);
		idTxtRect.sizeDelta = new Vector2(185, 45);
		idTxtRect.anchoredPosition = new Vector2(150, 0);

		UICustomPanel.CreateSplitter(UICore.TMP_Window.transform, new Vector2(0, -40), new(440, 2));
		
		string saveName = isHost ? SavesManager.ModSaves[save_index].Name : "Game";
		var saveNameTxt = UIElements.CreateText(UICore.TMP_Window.transform, "Save name: "  + saveName, 18);
		var saveNameTxtRect = saveNameTxt.GetComponent<RectTransform>();
		saveNameTxtRect.anchorMin = new Vector2(0.5f, 1f);
		saveNameTxtRect.anchorMax = new Vector2(0.5f, 1f);
		saveNameTxtRect.pivot = new Vector2(0.5f, 1f);
		saveNameTxtRect.sizeDelta = new Vector2(250, 45);
		saveNameTxtRect.anchoredPosition = new Vector2(-90, -36.5f);
		
		var playerReadyTxt = UIElements.CreateText(UICore.TMP_Window.transform, "Players Ready: " + $"0/{ClientData.Instance.connectedClients.Count}", 18);
		var playerReadyTxtRect = playerReadyTxt.GetComponent<RectTransform>();
		playerReadyTxtRect.anchorMin = new Vector2(0.5f, 1f);
		playerReadyTxtRect.anchorMax = new Vector2(0.5f, 1f);
		playerReadyTxtRect.pivot = new Vector2(0.5f, 1f);
		playerReadyTxtRect.sizeDelta = new Vector2(200, 45);
		playerReadyTxtRect.anchoredPosition = new Vector2(160, -36.5f);
		
		UICustomPanel.CreateSplitter(UICore.TMP_Window.transform, new Vector2(0, -75), new(440, 2));
		
		AddPlayer("Waiting for player...", 2);
		AddPlayer("Waiting for player...", 3);
		AddPlayer("Waiting for player...", 4);
		
		CreateButtons(isHost, save_index);
	}

	public static void CreateButtons(bool isHost, int save_index)
	{
		if (UICore.MP_Lobby)
			UIUtils.DestroyPanelButtons(UICore.MP_Lobby.transform);
		
		var hostBtn = UIElements.CreateButton(UICore.MP_Lobby.transform,
			"Start Game", (() => { UIActions.StartGame(save_index); }));
		var hostRect = hostBtn.GetComponent<RectTransform>();
		hostRect.anchorMin = new Vector2(0f, 0.5f);
		hostRect.anchorMax = new Vector2(0f, 0.5f);
		hostRect.pivot = new Vector2(0f, 0.5f);
		hostRect.sizeDelta = new Vector2(233, 44);
		hostRect.anchoredPosition = new Vector2(0, 344);
		if (!Server.Instance.isRunning)
			hostBtn.SetLocked();
		
		var joinBtn = UIElements.CreateButton(UICore.MP_Lobby.transform, "Ready Up", null);
		var joinRect = joinBtn.GetComponent<RectTransform>();
		joinRect.anchorMin = new Vector2(0, 0.5f);
		joinRect.anchorMax = new Vector2(0f, 0.5f);
		joinRect.pivot = new Vector2(0f, 0.5f);
		joinRect.sizeDelta = new Vector2(233, 45);
		joinRect.anchoredPosition = new Vector2(0, 295);
		joinBtn.OnClick.AddListener(UIActions.SwitchReady(joinBtn));
		joinBtn.SetLocked(false);
		joinBtn.SetDisabled(false, true);
		
		var typeBtn = UIElements.CreateButton(UICore.MP_Lobby.transform, "Invite via Steam", null);
		var typeRect = typeBtn.GetComponent<RectTransform>();
		typeRect.anchorMin = new Vector2(0f, 0.5f);
		typeRect.anchorMax = new Vector2(0f, 0.5f);
		typeRect.pivot = new Vector2(0f, 0.5f);
		typeRect.sizeDelta = new Vector2(233, 44);
		typeRect.anchoredPosition = new Vector2(0, 246);
		
		var copyIdBtn = UIElements.CreateButton(UICore.MP_Lobby.transform, 
			"Copy server ID", (() =>
			{
				if (!Server.Instance.isRunning)
					GUIUtility.systemCopyBuffer = ClientData.UserData.lobbyID;
				else
					GUIUtility.systemCopyBuffer = Server.Instance.serverID;
			}));
		var copyIdRect = copyIdBtn.GetComponent<RectTransform>();
		copyIdRect.anchorMin = new Vector2(0f, 0.5f);
		copyIdRect.anchorMax = new Vector2(0f, 0.5f);
		copyIdRect.pivot = new Vector2(0f, 0.5f);
		copyIdRect.sizeDelta = new Vector2(233, 44);
		copyIdRect.anchoredPosition = new Vector2(0, 197);
		if (ClientData.UserData.selectedNetworkType == NetworkType.TCP)
			copyIdBtn.SetLocked();
		
		var backBtn = UIElements.CreateButton(UICore.MP_Lobby.transform, "Back to menu", 
			() =>
			{
				if (isHost) MelonCoroutines.Start(Server.Instance.CloseServer());
				UICore.ShowPanel(UICore.MP_Main.gameObject, true);
			});
		var backRect = backBtn.GetComponent<RectTransform>();
		backRect.anchorMin = new Vector2(0f, 0.5f);
		backRect.anchorMax = new Vector2(0f, 0.5f);
		backRect.pivot = new Vector2(0f, 0.5f);
		backRect.sizeDelta = new Vector2(233, 44);
		backRect.anchoredPosition = new Vector2(0, 99);
	}

	public static GameObject AddPlayer(string username="Waiting for player...", int index=1)
	{
		int pos = 10 - ((index - 1) * 59);
		
		GameObject player = new GameObject("PlayerPanel");
		player.transform.SetParent(UICore.TMP_Window.transform, false);

		var panelRect = player.AddComponent<RectTransform>();
		panelRect.anchorMin = new Vector2(0.5f, 0.5f);
		panelRect.anchorMax = new Vector2(0.5f, 0.5f);
		panelRect.pivot = new Vector2(0.5f, 0.5f);
		panelRect.sizeDelta = new Vector2(450, 90);
		panelRect.anchoredPosition = new Vector2(0, pos);
		
		var playerNameTxt = UIElements.CreateText(player.transform, username, 18);
		var playerNameTxtRect = playerNameTxt.GetComponent<RectTransform>();
		playerNameTxtRect.anchorMin = new Vector2(0.5f, 1f);
		playerNameTxtRect.anchorMax = new Vector2(0.5f, 1f);
		playerNameTxtRect.pivot = new Vector2(0.5f, 1f);
		playerNameTxtRect.sizeDelta = new Vector2(200, 45);
		playerNameTxtRect.anchoredPosition = new Vector2(-115, 30);

		string content = username == "Waiting for player..." ? "" : "Not Ready";
		var playerStatusTxt = UIElements.CreateText(player.transform, content, 18);
		var playerStatusTxtRect = playerStatusTxt.GetComponent<RectTransform>();
		playerStatusTxtRect.anchorMin = new Vector2(0.5f, 1f);
		playerStatusTxtRect.anchorMax = new Vector2(0.5f, 1f);
		playerStatusTxtRect.pivot = new Vector2(0.5f, 1f);
		playerStatusTxtRect.sizeDelta = new Vector2(200, 45);
		playerStatusTxtRect.anchoredPosition = new Vector2(40, 28);
		playerStatusTxt.color = Color.red;
		
		var backBtn = UIElements.CreateButton(player.transform, "Kick", null);
		var backRect = backBtn.GetComponent<RectTransform>();
		backRect.anchorMin = new Vector2(1f, 0f);
		backRect.anchorMax = new Vector2(1f, 0f);
		backRect.pivot = new Vector2(0.5f, 0f);
		backRect.sizeDelta = new Vector2(100, 33);
		backRect.anchoredPosition = new Vector2(-80, 80);
		
		UICustomPanel.CreateSplitter(player.transform, new Vector2(0, -20), new(440, 2));
		return player;
	}


	public static void RefreshPlayers()
	{
		DeleteAllPlayer();
		int i = 0;

		foreach (UserData data in ClientData.Instance.connectedClients.Values)
		{
			var p = AddPlayer(data.username, i + 1);
			if (p == null)
			{
				MelonLogger.Warning($"[UILobby] AddPlayer returned null for {data.username}");
				i++;
				continue;
			}
			if (data.isReady && p.transform.childCount > 1)
			{
				var t = p.transform.GetChild(1)?.GetComponent<Text>();
				if (t != null)
				{
					t.text = "Ready";
					t.color = Color.green;
				}
				else
					MelonLogger.Warning("[UILobby] Player prefab child(1) has no Text component");
			}
			i++;
		}
		while (i < 4)
		{
			AddPlayer("Waiting for player...", i + 1);
			i++;
		}
		int ready_player = ClientData.Instance.connectedClients.Count(p => p.Value.isReady);
		if (UICore.TMP_Window != null && UICore.TMP_Window.transform.childCount > 4)
		{
			var t2 = UICore.TMP_Window.transform.GetChild(4).GetComponent<Text>();
			if (t2 != null)
				t2.text = $"Players Ready: {ready_player}/{ClientData.Instance.connectedClients.Count}";
			else
				MelonLogger.Warning("[UILobby] Child(4) of TMP_Window has no Text component");
		}
		else
			MelonLogger.Warning("[UILobby] TMP_Window missing or not enough children");
	}

	private static void DeleteAllPlayer()
	{
		GameObject lobbyWindow = UICore.TMP_Window;

		for (int i = lobbyWindow.transform.childCount - 1; i >= 6; i--)
		{
			Object.Destroy(lobbyWindow.transform.GetChild(i).gameObject);
		}
	}
}