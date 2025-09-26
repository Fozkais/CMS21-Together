using CMS21Together.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace CMS21Together.ClientSide.Data.NewUI;

public static class UILobby
{
	public static void CreateLobby(bool isHost)
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
		
		var idTxt = UIElements.CreateText(UICore.TMP_Window.transform, "ID: " + "IX4E8B", 20);
		var idTxtRect = idTxt.GetComponent<RectTransform>();
		idTxtRect.anchorMin = new Vector2(0.5f, 1f);
		idTxtRect.anchorMax = new Vector2(0.5f, 1f);
		idTxtRect.pivot = new Vector2(0.5f, 1f);
		idTxtRect.sizeDelta = new Vector2(185, 45);
		idTxtRect.anchoredPosition = new Vector2(215, 0);

		UICustomPanel.CreateSplitter(UICore.TMP_Window.transform, new Vector2(0, -40), new(440, 2));
		
		var saveNameTxt = UIElements.CreateText(UICore.TMP_Window.transform, "Save name: "  + "Test", 18);
		var saveNameTxtRect = saveNameTxt.GetComponent<RectTransform>();
		saveNameTxtRect.anchorMin = new Vector2(0.5f, 1f);
		saveNameTxtRect.anchorMax = new Vector2(0.5f, 1f);
		saveNameTxtRect.pivot = new Vector2(0.5f, 1f);
		saveNameTxtRect.sizeDelta = new Vector2(250, 45);
		saveNameTxtRect.anchoredPosition = new Vector2(-90, -36.5f);
		
		var playerReadyTxt = UIElements.CreateText(UICore.TMP_Window.transform, "Players Ready: " + "4/4", 18);
		var playerReadyTxtRect = playerReadyTxt.GetComponent<RectTransform>();
		playerReadyTxtRect.anchorMin = new Vector2(0.5f, 1f);
		playerReadyTxtRect.anchorMax = new Vector2(0.5f, 1f);
		playerReadyTxtRect.pivot = new Vector2(0.5f, 1f);
		playerReadyTxtRect.sizeDelta = new Vector2(200, 45);
		playerReadyTxtRect.anchoredPosition = new Vector2(160, -36.5f);
		
		UICustomPanel.CreateSplitter(UICore.TMP_Window.transform, new Vector2(0, -75), new(440, 2));
		
		AddPlayer("Test User", 1);
		AddPlayer("Test User 2", 2);
		AddPlayer("Test User 3", 3);
		AddPlayer("Test User 4", 4);
		
		CreateButtons(isHost);
	}

	public static void CreateButtons(bool isHost)
	{
		var hostBtn = UIElements.CreateButton(UICore.MP_Lobby.transform,
			"Start Game", null);
		var hostRect = hostBtn.GetComponent<RectTransform>();
		hostRect.anchorMin = new Vector2(0f, 0.5f);
		hostRect.anchorMax = new Vector2(0f, 0.5f);
		hostRect.pivot = new Vector2(0f, 0.5f);
		hostRect.sizeDelta = new Vector2(233, 44);
		hostRect.anchoredPosition = new Vector2(0, 344);
		
		var joinBtn = UIElements.CreateButton(UICore.MP_Lobby.transform,
			"Ready Up", null);
		var joinRect = joinBtn.GetComponent<RectTransform>();
		joinRect.anchorMin = new Vector2(0, 0.5f);
		joinRect.anchorMax = new Vector2(0f, 0.5f);
		joinRect.pivot = new Vector2(0f, 0.5f);
		joinRect.sizeDelta = new Vector2(233, 45);
		joinRect.anchoredPosition = new Vector2(0, 295);
		
		var typeBtn = UIElements.CreateButton(UICore.MP_Lobby.transform, "Invite via Steam", null);
		var typeRect = typeBtn.GetComponent<RectTransform>();
		typeRect.anchorMin = new Vector2(0f, 0.5f);
		typeRect.anchorMax = new Vector2(0f, 0.5f);
		typeRect.pivot = new Vector2(0f, 0.5f);
		typeRect.sizeDelta = new Vector2(233, 44);
		typeRect.anchoredPosition = new Vector2(0, 246);
		
		var copyIdBtn = UIElements.CreateButton(UICore.MP_Lobby.transform, "Copy server ID", null);
		var copyIdRect = copyIdBtn.GetComponent<RectTransform>();
		copyIdRect.anchorMin = new Vector2(0f, 0.5f);
		copyIdRect.anchorMax = new Vector2(0f, 0.5f);
		copyIdRect.pivot = new Vector2(0f, 0.5f);
		copyIdRect.sizeDelta = new Vector2(233, 44);
		copyIdRect.anchoredPosition = new Vector2(0, 197);
		
		var backBtn = UIElements.CreateButton(UICore.MP_Lobby.transform, "Back to menu", () => UICore.ShowPanel(UICore.MP_Main.gameObject, true));
		var backRect = backBtn.GetComponent<RectTransform>();
		backRect.anchorMin = new Vector2(0f, 0.5f);
		backRect.anchorMax = new Vector2(0f, 0.5f);
		backRect.pivot = new Vector2(0f, 0.5f);
		backRect.sizeDelta = new Vector2(233, 44);
		backRect.anchoredPosition = new Vector2(0, 99);
	}

	public static void AddPlayer(string username, int index)
	{
		
	}
}