using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.ClientSide.Handle;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.PlayerData
{
    [HarmonyPatch]
    public class ModInventory 
    {
        public static List<ModItem> handledItem = new List<ModItem>();
        public static List<ModGroupItem> handledGroupItem = new List<ModGroupItem>();
        

        [HarmonyPatch(typeof(Inventory), "Add", new Type[] { typeof(Item), typeof(bool) })]
        [HarmonyPrefix]
        public static void AddItemHook(Item item, bool showPopup = false)
        {
            var newItem = new ModItem(item);
            if (!handledItem.Contains(newItem))
            {
                handledItem.Add(newItem);
                ClientSend.SendInventoryItem(newItem, true);
            }
        }


        [HarmonyPatch(typeof(Inventory), "AddGroup")]
        [HarmonyPrefix]
        public static void AddGroupItemHook(GroupItem group)
        {
            var newItem = new ModGroupItem(group);
            if (!handledGroupItem.Contains(newItem))
            {
                handledGroupItem.Add(newItem);
                ClientSend.SendInventoryGroupItem(newItem, true);
            }
        }


        [HarmonyPatch(typeof(Inventory), "Delete")]
        [HarmonyPrefix]
        public static void RemoveItemHook(Item item, Inventory __instance)
        {
            var itemToRemove = handledItem.First(s => s.UID == item.UID);
            ClientSend.SendInventoryItem(itemToRemove, false);
            handledItem.Remove(itemToRemove);
            
        }

        [HarmonyPatch(typeof(Inventory), "DeleteGroup")]
        [HarmonyPrefix]
        public static void RemoveGroupItemHook(long UId)
        {
            var itemToRemove = handledGroupItem.First(s => s.UID == UId);
            ClientSend.SendInventoryGroupItem(itemToRemove, false);
            handledGroupItem.Remove(itemToRemove);
        }
        
        [HarmonyPatch(typeof(Inventory), "Load")]
        [HarmonyPostfix]
        public static void LoadHook(Inventory __instance)
        {
            foreach (GroupItem group in  __instance.GetGroups())
            {
                var newItem = new ModGroupItem(group);
                handledGroupItem.Add(newItem);
                ClientSend.SendInventoryGroupItem(newItem, true);
            }
            
            foreach (Item item in  __instance.GetItems())
            {
                var newItem = new ModItem(item);
                handledItem.Add(newItem);
                ClientSend.SendInventoryItem(newItem, true);
            }
        }
    }
}