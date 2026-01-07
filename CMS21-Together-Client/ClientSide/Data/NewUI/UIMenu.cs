using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using UnityEngine;
using UnityEngine.UI;

namespace CMS21Together.ClientSide.Data.NewUI;

public static class UIMenu
{
	public static int save_btn_index;
	
	public static void SetupMainMenu()
	{
		var playRect = UICore.templateButton.GetComponent<RectTransform>();
		playRect.anchorMin = new Vector2(0.5f, 0.5f);
		playRect.anchorMax = new Vector2(0.5f, 0.5f);
		playRect.pivot = new Vector2(1f, 0.5f);
		playRect.sizeDelta = new Vector2(180, 44);
		playRect.anchoredPosition = new Vector2(64, 40);
		
		var multiBtn = UIElements.CreateButton(UICore.templateButton.transform.parent,
			"",  () => { UICore.ShowPanel(UICore.MP_Main.gameObject); });

		var multiRect = multiBtn.GetComponent<RectTransform>();
		multiRect.anchorMin = new Vector2(0.5f, 0.5f);
		multiRect.anchorMax = new Vector2(0.5f, 0.5f);
		multiRect.pivot = new Vector2(0f, 0.5f);
		multiRect.sizeDelta = new Vector2(45, 44);
		multiRect.anchoredPosition = new Vector2(71, 40);

		var b1_texture = DataHelper.LoadCustomTexture("CMS21Together.Assets.peoples.png");
		var b1_sprite = Sprite.Create(b1_texture, new Rect(0, 0, b1_texture.width, b1_texture.height), new Vector2(0, 0));
		var imgObj = UIElements.CreateImage(multiBtn.transform, b1_sprite);
		var imgRect = imgObj.GetComponent<RectTransform>();
		imgRect.anchorMin = new Vector2(0.5f, 0.5f);
		imgRect.anchorMax = new Vector2(0.5f, 0.5f);
		imgRect.pivot = new Vector2(0f, 0.5f);
		imgRect.sizeDelta = new Vector2(40, 40);
		imgRect.anchoredPosition = new Vector2(-20, 0);
		multiBtn.GetComponentInChildren<Text>().gameObject.SetActive(false);
	}

	public static void SetupMultiplayerMenu()
	{
		var hostBtn = UIElements.CreateButton(UICore.MP_Main.transform,
			"Host a game", () => { UICore.ShowPanel(UICore.MP_Host.gameObject); });
		var hostRect = hostBtn.GetComponent<RectTransform>();
		hostRect.anchorMin = new Vector2(0f, 0.5f);
		hostRect.anchorMax = new Vector2(0f, 0.5f);
		hostRect.pivot = new Vector2(0f, 0.5f);
		hostRect.sizeDelta = new Vector2(233, 44);
		hostRect.anchoredPosition = new Vector2(0, 344);
		
		var joinBtn = UIElements.CreateButton(UICore.MP_Main.transform,
			"Join a game", () => { UICore.ShowCustomPanel(UICore.MP_Host.transform, UICustomPanelType.JoinMenu, null, 0); });
		var joinRect = joinBtn.GetComponent<RectTransform>();
		joinRect.anchorMin = new Vector2(0, 0.5f);
		joinRect.anchorMax = new Vector2(0f, 0.5f);
		joinRect.pivot = new Vector2(0f, 0.5f);
		joinRect.sizeDelta = new Vector2(233, 45);
		joinRect.anchoredPosition = new Vector2(0, 295);
		
		var typeBtn = UIElements.CreateButton(UICore.MP_Main.transform, "Network type: " + ClientData.UserData.selectedNetworkType, null);
		var typeRect = typeBtn.GetComponent<RectTransform>();
		typeRect.anchorMin = new Vector2(0f, 0.5f);
		typeRect.anchorMax = new Vector2(0f, 0.5f);
		typeRect.pivot = new Vector2(0f, 0.5f);
		typeRect.sizeDelta = new Vector2(233, 44);
		typeRect.anchoredPosition = new Vector2(0, 246);
		typeBtn.OnClick.AddListener(UIActions.ChangeNetworkType(typeBtn));
		if (ApiCalls.useSteam)
			typeBtn.SetLocked(false);
		
		var settingBtn = UIElements.CreateButton(UICore.MP_Main.transform, "Mod settings", null);
		var settingRect = settingBtn.GetComponent<RectTransform>();
		settingRect.anchorMin = new Vector2(0, 0.5f);
		settingRect.anchorMax = new Vector2(0, 0.5f);
		settingRect.pivot = new Vector2(0f, 0.5f);
		settingRect.sizeDelta = new Vector2(233, 44);
		settingRect.anchoredPosition = new Vector2(0, 148);
		
		var backBtn = UIElements.CreateButton(UICore.MP_Main.transform, "Back to menu", () => UICore.ShowPanel(UICore.V_Main.gameObject));
		var backRect = backBtn.GetComponent<RectTransform>();
		backRect.anchorMin = new Vector2(0f, 0.5f);
		backRect.anchorMax = new Vector2(0f, 0.5f);
		backRect.pivot = new Vector2(0f, 0.5f);
		backRect.sizeDelta = new Vector2(233, 44);
		backRect.anchoredPosition = new Vector2(0, 99);

		UICore.MP_Main.SetActive(false);
	}

	public static void SetupHostMenu()
	{

		Vector2 b_pos = new Vector2(0, 344);
		
		int i = 0;
		while (i < 4)
		{
			var saveBtn = UIElements.CreateButton(UICore.MP_Host.transform, UIUtils.GetSaveName(i + 4), null);
			var saveRect = saveBtn.GetComponent<RectTransform>();
			saveRect.anchorMin = new Vector2(0f, 0.5f);
			saveRect.anchorMax = new Vector2(0f, 0.5f);
			saveRect.pivot = new Vector2(0f, 0.5f);
			saveRect.sizeDelta = new Vector2(233, 44);
			saveRect.anchoredPosition = b_pos;
			saveBtn.OnClick.AddListener(UIActions.LoadGame(saveBtn, i + 4));
			saveBtn.SetLocked(false);
			b_pos.y -= 49;
			i++;
		}
		save_btn_index = 4;
		
		var prevBtn = UIElements.CreateButton(UICore.MP_Host.transform,
			"Previous", null);
		var prevRect = prevBtn.GetComponent<RectTransform>();
		prevRect.anchorMin = new Vector2(0f, 0.5f);
		prevRect.anchorMax = new Vector2(0f, 0.5f);
		prevRect.pivot = new Vector2(0f, 0.5f);
		prevRect.sizeDelta = new Vector2(115f, 44);
		prevRect.anchoredPosition = new Vector2(0, 148);
		
		var nextBtn = UIElements.CreateButton(UICore.MP_Host.transform, "Next", null);
		prevBtn.OnClick.AddListener( UIActions.PreviousSaves(prevBtn, nextBtn));
		prevBtn.SetLocked();
		
		var nextRect = nextBtn.GetComponent<RectTransform>();
		nextRect.anchorMin = new Vector2(0f, 0.5f);
		nextRect.anchorMax = new Vector2(0f, 0.5f);
		nextRect.pivot = new Vector2(0f, 0.5f);
		nextRect.sizeDelta = new Vector2(115f, 44);
		nextRect.anchoredPosition = new Vector2(118f, 148);
		nextBtn.OnClick.AddListener( UIActions.NextSaves(nextBtn, prevBtn));
		nextBtn.SetLocked(false);
		
		var backBtn = UIElements.CreateButton(UICore.MP_Host.transform, "Back to menu",
			() =>
			{
				UICore.last_index_pressed = 0;
				if (UICore.TMP_Window) Object.Destroy(UICore.TMP_Window);
				UICore.ShowPanel(UICore.MP_Main.gameObject);
			});
		var backRect = backBtn.GetComponent<RectTransform>();
		backRect.anchorMin = new Vector2(0f, 0.5f);
		backRect.anchorMax = new Vector2(0f, 0.5f);
		backRect.pivot = new Vector2(0f, 0.5f);
		backRect.sizeDelta = new Vector2(233, 44);
		backRect.anchoredPosition = new Vector2(0, 99);

		UICore.MP_Host.SetActive(false);
	}

}