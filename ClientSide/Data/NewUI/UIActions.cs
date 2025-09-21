using System;
using CMS.MainMenu.Controls;
using CMS21Together.ClientSide.Data.CustomUI;
using CMS21Together.Shared.Data;
using UnityEngine.Events;
using Object = Il2CppSystem.Object;

namespace CMS21Together.ClientSide.Data.NewUI;

public static class UIActions
{
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
			if (UIUtils.GetSaveName(save_index) != "New game")
				UICore.ShowCustomPanel(UICore.MP_Host.transform, UICustomPanelType.SaveInfo);
			else
				UICore.ShowCustomPanel(UICore.MP_Host.transform, UICustomPanelType.CreateSave);
		};
		return action;
	}
}