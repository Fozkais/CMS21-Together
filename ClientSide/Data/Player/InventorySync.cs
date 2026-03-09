using System.Collections;
using System.Linq;
using CMS.UI.Logic;
using CMS.UI.Windows;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.Shared.Data.Vanilla;
using CMS21Together.Shared.Data.Vanilla.Cars;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Player;

[HarmonyPatch]
public static class InventorySync
{
	private static bool isSyncing = false;
	private static bool isServerInitial = true;
	
	public static void Reset()
	{
		isSyncing = false;
		isServerInitial = true;
	}

	[HarmonyPatch(typeof(Inventory), nameof(Inventory.Load))]
	[HarmonyPrefix]
	public static bool LoadFix(Inventory __instance)
	{
		if (!Client.Instance.isConnected) return true;

		if (!Server.Instance.isRunning || !isServerInitial)
		{
			MelonLogger.Msg("Ask Full inventory resync!");
			Singleton<GameManager>.Instance.TempInventory.ClearListOfItems();
			GameData.Instance.localInventory.DeleteAllInventory();
			ClientSend.RequestInventoryResync();
			return false;
		}
		
		MelonLogger.Msg("Initial Server Inventory Sync !");
		isServerInitial = false;
		
		GameInventory instance = GameInventory.Instance;
		bool flag = instance != null;
		NewInventoryData inventoryData = Singleton<GameManager>.Instance.GameDataManager.CurrentProfileData.inventoryData;
		List<Item> list = inventoryData.items;
		if (list == null)
		{
			__instance.CreateItemsIfNeeded(0);
		}
		else
		{
			__instance.CreateItemsIfNeeded(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				Item item = list[i];
				if (item != null)
				{
					if (flag && !instance.IsBody(item.ID))
					{
						item.Dent = 1f;
					}
					__instance.items.Add(new Item(item));
				}
			}
		}
		List<GroupItem> list2 = inventoryData.groups;
		if (list2 == null)
		{
			__instance.CreateGroupsIfNeeded(0);
			return false;
		}
		__instance.CreateGroupsIfNeeded(list2.Count);
		for (int j = 0; j < list2.Count; j++)
		{
			GroupItem groupItem = list2[j];
			List<Item> itemList = groupItem.ItemList;
			if (itemList != null && itemList.Count != 0)
			{
				for (int k = 0; k < itemList.Count; k++)
				{
					Item item2 = itemList[k];
					if (item2 != null && flag && !instance.IsBody(item2.ID))
					{
						item2.Dent = 1f;
					}
				}
				__instance.groups.Add(new GroupItem(groupItem));
			}
		}
		return false;
	}
	
	///			   ///
	///  Item	   ///
	///			   ///

	[HarmonyPatch(typeof(ShopBuyWindow), nameof(ShopBuyWindow.BuyItem))]
	[HarmonyPrefix]
	public static bool BuyItemFix(ShopBuyWindow __instance)
	{
		if (!Client.Instance.isConnected) return true;

		if (GlobalData.PlayerMoney < __instance.currentPrice)
		{
			UIManager.Get().ShowInfoWindow("GUI_BrakKasy");
			return false;
		}
		ShopBuyItemType shopBuyItemType = __instance.type;
		if (shopBuyItemType <= ShopBuyItemType.LicensePlate)
		{
			string text;
			if (GameInventory.Instance.IsLicensePlate(__instance.itemID))
			{
				text = "LicensePlate";
			}
			else
			{
				text = Helper.WhatIsMyID(__instance.itemID);
			}
			Color color = (GameInventory.Instance.GetItemProperty(text).IsBody ? GlobalData.DEFAULT_ITEM_COLOR : Color.white);
			for (int i = 0; i < __instance.currentAmount; i++)
			{
				Item item = new Item(text);
				item.Condition = 1f;
				item.Color = new ModCustomColor(color).ToGame();
				item.IsExamined = true;
				item.PaintType = PaintType.Unpainted;
				item.WashFactor = 1f;
				var wheelData = item.WheelData;
				wheelData.Width = __instance.currentWidth;
				wheelData.Size = __instance.currentSize;
				wheelData.Profile = __instance.currentProfile;
				wheelData.ET = __instance.currentET;
				item.WheelData = wheelData;
				Item item2 = item;
				if (GameInventory.Instance.IsLicensePlate(__instance.itemID))
				{
					item2.LPData.Name = __instance.itemID;
					item2.LPData.Custom = string.Empty;
				}
				//Singleton<GameManager>.Instance.Inventory.Add(item2, false); Handled by server
				ClientSend.RequestAddItem(new ModItem(item2), GameInventory.Instance.GetItemProperty(item2.ID).Price);
			}
			if (__instance.shopType == ShopType.Main)
			{
				Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_buy_parts", __instance.currentAmount);
			}
			//GlobalData.AddPlayerMoney((int)(-(int)__instance.currentPrice)); Handled by server
		
			__instance.Hide(false);
		}
		
		return false;
	}

	[HarmonyPatch(typeof(Inventory), nameof(Inventory.Add), typeof(Item), typeof(bool))]
	[HarmonyPrefix]
	public static bool AddItemFix(Inventory __instance, Item item, bool showPopup)
	{
		if (!Client.Instance.isConnected) return true;

		if (isSyncing)
		{
			isSyncing = false;
			return true;
		}
		MelonLogger.Msg("Add Item Free");
		ClientSend.RequestAddItem(new ModItem(item), 0);
		return false;
	}
	
	[HarmonyPatch(typeof(Inventory), nameof(Inventory.Delete))]
	[HarmonyPrefix]
	public static bool DeleteItemFix(Inventory __instance, Item item)
	{
		if (!Client.Instance.isConnected) return true;

		if (isSyncing)
		{
			isSyncing = false;
			return true;
		}
		MelonLogger.Msg("Delete Item Free");
		ClientSend.RequestItemDelete(new ModItem(item));
		return false;
	}
	
	public static IEnumerator AddItem(ModItem item)
	{
		while (!GameLoadHook.IsGameReady())
			yield return new WaitForSeconds(0.1f);
		
		isSyncing = true;
		GameData.Instance.localInventory.Add(item.ToGame());
		
		UIManager.Get().ShowPopup("PopUp_NewItem", GameInventory.Instance.GetItemLocalizeName(item.ID), PopupType.Buy);
		SoundManager.Get().PlaySFXOneShot("Popup");
	}
	
	public static IEnumerator DeleteItem(ModItem item)
	{
		while (!GameLoadHook.IsGameReady())
			yield return new WaitForSeconds(0.1f);
		
		isSyncing = true;
		GameData.Instance.localInventory.Delete(item.ToGame());
	}
	
	///			   ///
	/// Group Item ///
	///			   ///

	
	[HarmonyPatch(typeof(Inventory), nameof(Inventory.AddGroup))]
	[HarmonyPrefix]
	public static bool AddGroupItemFix(Inventory __instance, GroupItem group)
	{
		if (!Client.Instance.isConnected) return true;

		if (isSyncing)
		{
			isSyncing = false;
			return true;
		}
		MelonLogger.Msg("Add GroupItem ");
		ClientSend.RequestAddGroupItem(new ModGroupItem(group));
		return false;
	}
	
	[HarmonyPatch(typeof(Inventory), nameof(Inventory.DeleteGroup))]
	[HarmonyPrefix]
	public static bool DeleteGroupItemFix(Inventory __instance, long UId)
	{
		if (!Client.Instance.isConnected) return true;

		if (isSyncing)
		{
			isSyncing = false;
			return true;
		}
		MelonLogger.Msg("Delete GroupItem ");
		ClientSend.RequestGroupItemDelete(UId);
		return false;
	}

	public static IEnumerator AddGroupItem(ModGroupItem item)
	{
		while (!GameLoadHook.IsGameReady())
			yield return new WaitForSeconds(0.1f);
		
		isSyncing = true;
		GameData.Instance.localInventory.AddGroup(item.ToGame());
	}
	
	public static IEnumerator DeleteGroupItem(long UId)
	{
		while (!GameLoadHook.IsGameReady())
			yield return new WaitForSeconds(0.1f);
		
		isSyncing = true;
		GameData.Instance.localInventory.DeleteGroup(UId);
	}
}