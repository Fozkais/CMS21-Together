using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS.MainMenu.Controls;
using CMS.MainMenu.Sections;
using CMS.UI.Controls;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CMS21Together.ClientSide.Data.NewUI;

public static class UICore
{
	public static GameObject templateButton;
	public static GameObject templateText;
	public static GameObject templateInputField;
	public static GameObject templateSelector;
	public static GameObject templateImage;

	public static GameObject Active_Panel;
	
	public static GameObject UI_Main;
	public static GameObject V_Main;
	public static GameObject MP_Main;
	public static GameObject MP_Host;
	public static GameObject MP_Lobby;
	
	public static GameObject TMP_Window;
	public static GameObject TMP_Info_Window;

	private static bool update_notice = false;
	public static int last_index_pressed;

	public static void InitializeUI(string sceneName)
	{
		if (sceneName != "Menu") return;
		
		templateImage = GameObject.Find("Logo");
		templateButton = GameObject.Find("MainMenuButton");
		templateInputField = GameObject.Find("Main").transform.GetChild(8).gameObject;
		templateText = templateButton.GetComponentInChildren<Text>().gameObject;
		templateSelector = GameObject.Find("MainMenuWindows").transform.GetChild(3).GetChild(0).gameObject
									 .GetComponentInChildren<StringSelector>().gameObject;

		UI_Main = GameObject.Find("MainButtons").transform.parent.gameObject;
		V_Main = GameObject.Find("MainButtons").GetComponent<MainSection>().gameObject;
		MP_Main = CreateNewPanel("MP_Main");
		MP_Host = CreateNewPanel("MP_Host");
		MP_Lobby = CreateNewPanel("MP_Lobby");
		
		
		LoadCustomlogo();
		GameObject.Find("Logo").GetComponent<RectTransform>().sizeDelta = new Vector2(250, 250);
		UIMenu.SetupMainMenu();
		UIMenu.SetupMultiplayerMenu();
		UIMenu.SetupHostMenu();

		Active_Panel = UI_Main;
		MelonCoroutines.Start(CheckModVersion());
	}

	private static IEnumerator CheckModVersion()
	{
		if (update_notice)
			yield break;
		yield return new WaitForEndOfFrame();
		yield return new WaitForSeconds(1);

		switch (ContentManager.Instance.IsNewVersionAvailable(MainMod.ASSEMBLY_MOD_VERSION))
		{
			case VersionStatus.Outdated:
				UICustomPanel.CreateInfoPanel("A new version of the mod is available !");
				update_notice = true;
				break;
			case VersionStatus.Latest:
				MelonLogger.Msg("Mod is up-to-date !");
				break;
			case VersionStatus.Dev:
				UICustomPanel.CreateInfoPanel("You are using a development build !");
				break;
		}
	}

	private static void DestroyChildren(Transform parent)
	{
		var toDestroy = new List<GameObject>();
		for (int i = 0; i < parent.childCount; i++)
			toDestroy.Add(parent.GetChild(i).gameObject);
		foreach (var go in toDestroy)
			Object.Destroy(go);
	}
	
	private static void LoadCustomlogo()
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
	
	public static void ShowPanel(GameObject panelToShow, bool destroyChildren=false)
	{
		if (destroyChildren)
			DestroyChildren(Active_Panel.transform);

		Active_Panel =  panelToShow;
		if (TMP_Window)
			Object.Destroy(TMP_Window);
		if (TMP_Info_Window)
			Object.Destroy(TMP_Info_Window);
		
		V_Main.gameObject.SetActive(false);
		MP_Main.gameObject.SetActive(false);
		MP_Host.gameObject.SetActive(false);
		MP_Lobby.gameObject.SetActive(false);

		panelToShow.SetActive(true);
	}

	private static GameObject CreateNewPanel(string name)
	{
		GameObject panel = Object.Instantiate(UICore.V_Main, UICore.V_Main.transform.parent, false);
		panel.transform.position = new Vector3(panel.transform.position.x, 0, panel.transform.position.z);
		DestroyChildren(panel.transform);
		panel.name = name;
		return panel;
	}
	public static GameObject CreateElement(GameObject template, Transform parent)
	{
		var obj = Object.Instantiate(template, parent, false);
		obj.SetActive(true);
		var rect = obj.GetComponent<RectTransform>();
		
		rect.localScale = Vector3.one;
		rect.localPosition = Vector3.zero;
		rect.offsetMin = Vector2.zero;
		rect.offsetMax = Vector2.zero;
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.anchoredPosition = new Vector2(0, 100);
		rect.sizeDelta = new Vector2(336, 65);
		
		return obj;
	}

	public static void ShowCustomPanel(Transform currentPanel, UICustomPanelType panelType, MainMenuButton btn, int index)
	{
		switch (panelType)
		{
			case UICustomPanelType.SaveInfo:
				UICustomPanel.CreateSaveInfoPanel(btn, index);
				break;
			case UICustomPanelType.CreateSave:
				UICustomPanel.CreateNewSavePanel(btn, index);
				break;
			case UICustomPanelType.JoinAsHostMenu:
				UICustomPanel.JoinAsHostPanel(btn, index);
				break;
			case UICustomPanelType.JoinMenu:
				UICustomPanel.JoinPanel();
				break;
			case UICustomPanelType.LobbyMenu:
				break;
		}
	}
}