using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS.MainMenu.Controls;
using CMS.MainMenu.Sections;
using CMS.UI.Controls;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;

namespace CMS21Together.ClientSide.Data.NewUI;

public static class UICore
{
	public static GameObject templateButton;
	public static GameObject templateText;
	public static GameObject templateInputField;
	public static GameObject templateSelector;
	
	public static GameObject V_Main;
	public static GameObject MP_Main;
	public static GameObject MP_Host;
	public static GameObject MP_Lobby_Parent;
	public static GameObject MP_Saves_Parent;

	public static IEnumerator InitializeUI(string sceneName)
	{
		if (sceneName != "Menu") yield break;
		
		templateButton = GameObject.Find("MainMenuButton");
		templateInputField = GameObject.Find("Main").transform.GetChild(8).gameObject;
		templateText = templateButton.GetComponentInChildren<Text>().gameObject;
		templateSelector = GameObject.Find("MainMenuWindows").transform.GetChild(3).GetChild(0).gameObject
									 .GetComponentInChildren<StringSelector>().gameObject;

		V_Main = GameObject.Find("MainButtons").GetComponent<MainSection>().gameObject;
		MP_Main = CreateNewPanel();
		MP_Host = CreateNewPanel();
		
		LoadCustomlogo();
		GameObject.Find("Logo").gameObject.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
		UIMenu.SetupMainMenu();
		yield return new WaitForEndOfFrame();
		UIMenu.SetupMultiplayerMenu();
		UIMenu.SetupHostMenu();
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
	
	public static void ShowPanel(GameObject panelToShow)
	{
		V_Main.gameObject.SetActive(false);
		MP_Main.gameObject.SetActive(false);
		MP_Host.gameObject.SetActive(false);

		panelToShow.SetActive(true);
	}

	private static GameObject CreateNewPanel()
	{
		GameObject panel = Object.Instantiate(UICore.V_Main, UICore.V_Main.transform.parent, false);
		panel.transform.position = new Vector3(panel.transform.position.x, 0, panel.transform.position.z);
		DestroyChildren(panel.transform);
		return panel;
	}
	public static GameObject CreateElement(GameObject template, Transform parent)
	{
		var obj = Object.Instantiate(template, parent, false);
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

	public static void ShowCustomPanel(Transform currentPanel, UICustomPanelType saveInfo)
	{
		var buttons = currentPanel.GetComponentsInChildren<MainMenuButton>().ToList();
		foreach (MainMenuButton button in buttons)
		{
			button.SetLocked();
		}
	}
}