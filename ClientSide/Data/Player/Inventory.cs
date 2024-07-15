using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace CMS21Together.ClientSide.Data.Player;

[HarmonyPatch]
public static class Inventory
{
    public static List<ModItem> items = new List<ModItem>();
    public static List<ModGroupItem> groupItems = new List<ModGroupItem>();
    private static bool loadSkip;

    public static void Reset()
    {
        items.Clear();
        groupItems.Clear();
        loadSkip = false;
    }
    
    
    
    [HarmonyPatch(typeof(Il2Cpp.Inventory), "Add", new Type[] { typeof(Item), typeof(bool) })]
    [HarmonyPrefix]
    public static void AddItemHook(Item item, bool showPopup = false)
    {
        if(!Client.Instance.isConnected) return;
            
        var newItem = new ModItem(item);
        if (!items.Any(s => s.UID == newItem.UID))
        {
            items.Add(newItem);
            ClientSend.ItemPacket(newItem, InventoryAction.add);
        }
    }
    
    [HarmonyPatch(typeof(Il2Cpp.Inventory), "AddGroup")]
    [HarmonyPrefix]
    public static void AddGroupItemHook(GroupItem group)
    {
        if(!Client.Instance.isConnected) return;
            
        var newItem = new ModGroupItem(group);
        if (!groupItems.Any(s => s.UID == newItem.UID))
        {
            groupItems.Add(newItem);
            ClientSend.GroupItemPacket(newItem, InventoryAction.add);
        }
    }
    
    [HarmonyPatch(typeof(Il2Cpp.Inventory), "Delete")]
    [HarmonyPrefix]
    public static void RemoveItemHook(Item item,  Il2Cpp.Inventory __instance)
    {
        if(!Client.Instance.isConnected) return;

        if(item == null) return;
            
        if (items.Any(s => s.UID == item.UID))
        {
            var itemToRemove = items.First(s => s.UID == item.UID);
            ClientSend.ItemPacket(itemToRemove, InventoryAction.remove);
            items.Remove(itemToRemove);
        }

            
    }

    [HarmonyPatch(typeof(Il2Cpp.Inventory), "DeleteGroup")]
    [HarmonyPrefix]
    public static void RemoveGroupItemHook(long UId)
    {
        if(!Client.Instance.isConnected) return;

        if (groupItems.Any(s => s.UID == UId))
        {
            var itemToRemove = groupItems.First(s => s.UID == UId);
            ClientSend.GroupItemPacket(itemToRemove, InventoryAction.remove);
            groupItems.Remove(itemToRemove);
        }
    }

    [HarmonyPatch(typeof(Il2Cpp.Inventory), "Load")]
    [HarmonyPrefix]
    public static bool LoadHook(Il2Cpp.Inventory __instance)
    {
        if (!Client.Instance.isConnected) return true;

        if (!Server.Instance.isRunning)
        {
            if (loadSkip)
            {
                ClientSend.ItemPacket(null, InventoryAction.resync);
                ClientSend.GroupItemPacket(null, InventoryAction.resync);
            }
            else
            {
                loadSkip = true;
                return false;
            }
        }
        
        NewInventoryData inventoryData = Singleton<GameManager>.Instance.GameDataManager.CurrentProfileData.inventoryData;
        foreach (GroupItem group in  inventoryData.groups)
        {
            if (group != null)
            {
                var newItem = new ModGroupItem(group);
                groupItems.Add(newItem);
                ClientSend.GroupItemPacket(newItem, InventoryAction.add);
            }
        }
        MelonLogger.Msg($"[Inventory->LoadHook] Loaded {groupItems.Count} groupItem.");
            
        foreach (Item item in  inventoryData.items)
        {
            if (item != null)
            {
                var newItem = new ModItem(item);
                items.Add(newItem);
                ClientSend.ItemPacket(newItem, InventoryAction.add);
            }
        }
        MelonLogger.Msg($"[Inventory->LoadHook] Loaded {items.Count} Item.");
        return true;
    }

    public static IEnumerator HandleItem(ModItem item, InventoryAction action)
    {
        yield return GameData.GameReady();
        
        switch (action)
        {
            case InventoryAction.add:
                if (!items.Any(i => i.UID == item.UID))
                {
                    items.Add(item);
                    GameData.Instance.localInventory.Add(item.ToGame());
                }
                break;
            case InventoryAction.remove:
                if (items.Any(i => i.UID == item.UID))
                {
                    items.Remove(item);
                    GameData.Instance.localInventory.Delete(item.ToGame());
                }
                break;
        }
    }
    public static IEnumerator HandleGroupItem(ModGroupItem item, InventoryAction action)
    {
        yield return GameData.GameReady();
        switch (action)
        {
            case InventoryAction.add:
                if (!groupItems.Any(i => i.UID == item.UID))
                {
                    groupItems.Add(item);
                    GameData.Instance.localInventory.AddGroup(item.ToGame());
                }
                break;
            case InventoryAction.remove:
                if (groupItems.Any(i => i.UID == item.UID))
                {
                    groupItems.Remove(item);
                    GameData.Instance.localInventory.DeleteGroup(item.UID);
                }
                break;
        }
    }
}

[Serializable]
public enum InventoryAction
{
    add,
    remove,
    resync
}