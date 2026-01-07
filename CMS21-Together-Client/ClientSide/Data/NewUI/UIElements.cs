using System;
using CMS.MainMenu.Controls;
using CMS.UI.Controls;
using CMS.UI.Logic.Navigation;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace CMS21Together.ClientSide.Data.NewUI;

public static class UIElements
{
	public static Text CreateText(Transform parent, string content, int fontSize = 16, TextAnchor anchor = TextAnchor.MiddleLeft)
	{
		var obj = UICore.CreateElement(UICore.templateText, parent);
		var text = obj.GetComponent<Text>();
		text.text = content;
		text.fontSize = fontSize;
		text.alignment = anchor;
		return text;
	}

	public static MainMenuButton CreateButton(Transform parent, string label, Action onClick)
	{
		var obj = UICore.CreateElement(UICore.templateButton, parent);
		var btn = obj.GetComponent<MainMenuButton>();

		Object.Destroy(btn.GetComponentInChildren<TextLocalize>());
		btn.SetText(label);
		btn.text.fontSize -= 2;
		btn.OnClick.RemoveAllListeners();
		if (onClick == null)
		{
			btn.SetDisabled(true, true);
			btn.SetLocked();
		}
		else
			btn.OnClick.AddListener(onClick);
		btn.OnMouseHover = DelegateSupport.ConvertDelegate<Il2CppSystem.Action<int>>(
			new Action<int>((i) =>
			{
				btn.transform.parent.GetComponent<ListNavigationManager>()?.DeselectCurrent();
				btn.Select();
			})
		);
		return btn;
	}

	public static InputField CreateInput(Transform parent, string defaultText = "")
	{
		var obj = UICore.CreateElement(UICore.templateInputField, parent);
		var input = obj.GetComponentInChildren<InputField>();
		input.text = defaultText;
		return input;
	}
	
	public static Image CreateImage(Transform parent, Sprite sprite)
	{
		var obj = UICore.CreateElement(UICore.templateImage, parent);
		var input = obj.GetComponent<Image>();
		var inputCanvas = obj.GetComponent<Canvas>();
		input.sprite = sprite;
		inputCanvas.overrideSorting = true;
		inputCanvas.sortingOrder = 1;
		input.material = UICore.templateImage.GetComponent<Image>().material;
		return input;
	}

	public static StringSelector CreateSelector(Transform parent, List<string> options)
	{
		var obj = UICore.CreateElement(UICore.templateSelector, parent);
		var dd = obj.GetComponent<StringSelector>();
		dd.options = options;
		return dd;
	}
}