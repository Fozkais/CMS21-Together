using System;
using CMS.MainMenu.Controls;
using CMS.UI.Controls;
using CMS21Together.ClientSide.Data.CustomUI;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
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
		var obj = UICore.CreateElement(CustomUIManager.templateText, parent);
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
		return btn;
	}

	public static InputField CreateInput(Transform parent, string defaultText = "")
	{
		var obj = UICore.CreateElement(CustomUIManager.templateInputField, parent);
		var input = obj.GetComponentInChildren<InputField>();
		input.text = defaultText;
		return input;
	}

	public static StringSelector CreateSelector(Transform parent, IEnumerable<string> options)
	{
		var obj = UICore.CreateElement(CustomUIManager.templateSelector, parent);
		var dd = obj.GetComponent<StringSelector>();
		dd.options = new List<string>(options);
		return dd;
	}
}