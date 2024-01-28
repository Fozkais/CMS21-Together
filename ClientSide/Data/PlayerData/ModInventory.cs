using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.ClientSide.Handle;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.PlayerData
{
    public static class ModInventory
    {
        public static List<ModItem> handledItem = new List<ModItem>();
        public static List<ModGroupItem> handledGroupItem = new List<ModGroupItem>();


        private static bool isUpdating;
        public static void UpdateInventory()
        {
            if (ModSceneManager.isInGarage())
            {
                if(!isUpdating)
                    MelonCoroutines.Start(InventoryUpdating(0.25f));
            }
        }

        private static IEnumerator InventoryUpdating(float updateRate)
        {
            isUpdating = true;
            yield return new WaitForSeconds(updateRate);
            AddItem();
            AddItemGroupNew();
            RemoveItem();
            RemoveGroupItem();
            isUpdating = false;
        }
        
         public static void AddItem()
        {
            var localInventory = GameData.Instance.localInventory.items;
            for (int i = 0; i < localInventory.Count; i++)
            {
                var item = localInventory._items[i];
                if(handledItem.Count == 0)
                {
                    //MelonLogger.Msg("Adding Item");
                    var newItem = new ModItem(item);
                    handledItem.Add(newItem);
                    ClientSend.SendInventoryItem(newItem, true);
                }
                else if(!handledItem.Any(s => s.UID == item.UID))
                {
                   // MelonLogger.Msg("Adding Item");
                    var newItem = new ModItem(item);
                    handledItem.Add(newItem);
                    ClientSend.SendInventoryItem(newItem, true);
                }
            }
            
        }
        public static void AddItemGroupNew()
        {
            var localInventory = GameData.Instance.localInventory.groups;
            for (int i = 0; i < localInventory.Count; i++)
            {
                var item = localInventory._items[i];
                if (handledGroupItem.Count == 0)
                {
                  //  MelonLogger.Msg("Adding GroupItem");
                    var newItem = new ModGroupItem(item);
                    handledGroupItem.Add(newItem);
                    ClientSend.SendInventoryGroupItem(newItem, true);
                }
                else if (!handledGroupItem.Any(s => s.UID == item.UID))
                {
                  //  MelonLogger.Msg("Adding GroupItem");
                    var newItem = new ModGroupItem(item);
                    handledGroupItem.Add(newItem);
                    ClientSend.SendInventoryGroupItem(newItem, true);
                }
            }
        }
        
        public static void RemoveItem()
        {
            var localInventory = GameData.Instance.localInventory.items;
            List<Item> items = new List<Item>();
            foreach (var _item in localInventory)
            {
                items.Add(_item);
            }
            for (int i = 0; i < handledItem.Count; i++)
            {
                var item = handledItem[i];
                
                if (localInventory._items.Count == 0)
                {
                  //  MelonLogger.Msg("Local Inventory is empty");
                  //  MelonLogger.Msg("Remove Item!");
                    ClientSend.SendInventoryItem(item, false);
                    handledItem.Remove(item);
                }   
                else  if(!items.Any(s =>  s.UID == item.UID))
                {
                   // MelonLogger.Msg("Remove Item");
                    ClientSend.SendInventoryItem(item, false);
                    handledItem.Remove(item);
                }

                }
        }
        public static void RemoveGroupItem()
        {
            var localInventory = GameData.Instance.localInventory.groups;
            List<GroupItem> items = new List<GroupItem>();
            foreach (var _item in localInventory)
            {
                items.Add(_item);
            }
            
            for (int i = 0; i < handledGroupItem.Count; i++)
            {
                var item = handledGroupItem[i];

                if (localInventory.Count == 0)
                {
                   // MelonLogger.Msg("Local Inventory is empty");
                   // MelonLogger.Msg("Remove Item!");
                    ClientSend.SendInventoryGroupItem(item, false);
                    handledGroupItem.Remove(item);
                    return;
                }
                else if(!items.Any(s => s.UID == item.UID))
                {
                    //MelonLogger.Msg("Remove GroupItem");
                    ClientSend.SendInventoryGroupItem(item, false);
                    handledGroupItem.Remove(item);
                }
                    
            }
        }
    }
}