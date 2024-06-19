using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.ServerSide.Data;
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
        private static bool isUpdating;
        public static bool initialSkip;
        
        
        /*public static void UpdateInventory()
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
        }*/

        /*[HarmonyPatch(typeof(Inventory), "Load")]
        [HarmonyPostfix]
        public static void LoadHook(Inventory __instance)
        {
            if (!Client.Instance.isConnected) return;
            
            foreach (GroupItem group in __instance.GetGroups())
            {
                var newItem = new ModGroupItem(group);
                handledGroupItem.Add(newItem);
                ClientSend.SendInventoryGroupItem(newItem, true);
            }

            foreach (Item item in __instance.GetItems())
            {
                var newItem = new ModItem(item);
                handledItem.Add(newItem);
                ClientSend.SendInventoryItem(newItem, true);
            }
        }

        public static void AddItem()
        {
            if(GameData.Instance == null) return;
            
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
            if(GameData.Instance == null) return;
            
            var localInventory = GameData.Instance.localInventory.groups;
            for (int i = 0; i < localInventory.Count; i++)
            {
                var item = localInventory._items[i];
                if (handledGroupItem.Count == 0)
                {
                   // MelonLogger.Msg("Adding GroupItem");
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
            if(GameData.Instance == null) return;
            
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
            if(GameData.Instance == null) return;
            
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
                    //MelonLogger.Msg("Remove GroupItem!");
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
        }*/

        [HarmonyPatch(typeof(Inventory), "Add", new Type[] { typeof(Item), typeof(bool) })]
        [HarmonyPrefix]
        public static void AddItemHook(Item item, bool showPopup = false)
        {
            if(!Client.Instance.isConnected) return;
            
            var newItem = new ModItem(item);
            if (!handledItem.Any(s => s.UID == newItem.UID))
            {
                handledItem.Add(newItem);
                ClientSend.SendInventoryItem(newItem, true);
                
                MelonLogger.Msg($"Add item.");
            }
        }


        [HarmonyPatch(typeof(Inventory), "AddGroup")]
        [HarmonyPrefix]
        public static void AddGroupItemHook(GroupItem group)
        {
            if(!Client.Instance.isConnected) return;
            
            var newItem = new ModGroupItem(group);
            if (!handledGroupItem.Any(s => s.UID == newItem.UID))
            {
                handledGroupItem.Add(newItem);
                ClientSend.SendInventoryGroupItem(newItem, true);
                
                MelonLogger.Msg($"Add groupItem.");
            }
        }


        [HarmonyPatch(typeof(Inventory), "Delete")]
        [HarmonyPrefix]
        public static void RemoveItemHook(Item item, Inventory __instance)
        {
            if(!Client.Instance.isConnected) return;

            if(item == null) return;
            
            if (handledItem.Any(s => s.UID == item.UID))
            {
                var itemToRemove = handledItem.First(s => s.UID == item.UID);
                ClientSend.SendInventoryItem(itemToRemove, false);
                handledItem.Remove(itemToRemove);
                
                MelonLogger.Msg($"Remove item.");
            }

            
        }

        [HarmonyPatch(typeof(Inventory), "DeleteGroup")]
        [HarmonyPrefix]
        public static void RemoveGroupItemHook(long UId)
        {
            if(!Client.Instance.isConnected) return;

            if (handledGroupItem.Any(s => s.UID == UId))
            {
                var itemToRemove = handledGroupItem.First(s => s.UID == UId);
                ClientSend.SendInventoryGroupItem(itemToRemove, false);
                handledGroupItem.Remove(itemToRemove);

                MelonLogger.Msg($"Remove groupItem.");
            }
        }
        
        [HarmonyPatch(typeof(Inventory), "Load")]
        [HarmonyPrefix]
        public static bool LoadHook(Inventory __instance)
        {
            if(!Client.Instance.isConnected) return true;

            if (!ServerData.isRunning && initialSkip) 
            {
                ClientSend.SendInventoryItem(null, false, true);
                ClientSend.SendInventoryGroupItem(null, false, true);
            }
            else if (!ServerData.isRunning && !initialSkip)
            {
                initialSkip = true;
                return false;
            }
            
            MelonLogger.Msg("Loading Inventory.");

            NewInventoryData inventoryData =
                Singleton<GameManager>.Instance.GameDataManager.CurrentProfileData.inventoryData;
            
            foreach (GroupItem group in  inventoryData.groups)
            {
                if (group != null)
                {
                    var newItem = new ModGroupItem(group);
                    handledGroupItem.Add(newItem);
                    ClientSend.SendInventoryGroupItem(newItem, true);
                }
            }
            MelonLogger.Msg($"Loaded {handledGroupItem.Count} groupItem.");
            
            foreach (Item item in  inventoryData.items)
            {
                if (item != null)
                {
                    var newItem = new ModItem(item);
                    handledItem.Add(newItem);
                    ClientSend.SendInventoryItem(newItem, true);
                }
            }
            MelonLogger.Msg($"Loaded {handledItem.Count} item.");
            return true;
        }
        
        
        public static IEnumerator HandleItem(ModItem _item, bool status)
        {
            while (!GameData.DataInitialized)
                yield return new WaitForSeconds(0.2f);
            
            Item _itemGame = _item.ToGame(_item);
            if (status)
            {
                if (!ModInventory.handledItem.Any(s => s.UID == _item.UID) )
                {
                    ModInventory.handledItem.Add(_item);
                    GameData.Instance.localInventory.Add(_itemGame);
                }
            }
            else
            {
                if (ModInventory.handledItem.Any(s => s.UID == _item.UID))
                {
                    ModInventory.handledItem.Remove(_item);
                    GameData.Instance.localInventory.Delete(_itemGame);
                }
            }
            
        }
        
        public static IEnumerator HandleGroupItem(ModGroupItem _item, bool status)
        {
            while (!GameData.DataInitialized)
                yield return new WaitForSeconds(0.2f);
            
            if (status)
            {
                if (!ModInventory.handledGroupItem.Any(s => s.UID == _item.UID))
                {
                    ModInventory.handledGroupItem.Add(_item);
                    GameData.Instance.localInventory.AddGroup(_item.ToGame(_item));
                }
            }
            else
            {
                if (ModInventory.handledGroupItem.Any(s => s.UID == _item.UID))
                {
                    int index = ModInventory.handledGroupItem.FindIndex(s => s.UID == _item.UID);
                    ModInventory.handledGroupItem.Remove(ModInventory.handledGroupItem[index]);
                    GameData.Instance.localInventory.DeleteGroup(_item.UID);
                }
            }
            
        }
    }
}