using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21MP.ClientSide.DataHandle;
using CMS21MP.CustomData;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide.Data
{
    public static class ModInventory
    {
        public static List<Item> currentItems = new List<Item>();
        public  static List<Item> ItemsHandler = new List<Item>();
        public static List<Item> currentItemsHandler = new List<Item>();
        
        public static List<GroupItem> currentGroupItems = new List<GroupItem>();
        public static List<GroupItem> GroupItemsHandler = new List<GroupItem>();
        public static List<GroupItem> currentGroupItemsHandler = new List<GroupItem>();
        public static List<long> ItemsUID = new List<long>();

        
        private static bool isUpdating;
        public static void UpdateInventory()
        {
            if (SceneChecker.isInGarage())
            {
                if(!isUpdating)
                    MelonCoroutines.Start(InventoryUpdating(0.25f));
            }
        }

        private static IEnumerator InventoryUpdating(float updateRate)
        {
            isUpdating = true;
            yield return new WaitForSeconds(updateRate);
            //HandleNewItem();
            //HandleNewGroupItem();
            AddItem();
            AddItemGroupNew();
            RemoveItem();
            RemoveGroupItem();
            isUpdating = false;
        }
        
         public static void AddItem()
        {
            if (ItemsUID.Count == 0)
                ItemsUID.Add(1);
            
            var localInventory = ClientData.localInventory.items;

            currentItems.Clear();
            foreach (Item item in localInventory)
            {
                currentItems.Add(item);
            }

            for (int i = 0; i < currentItems.Count; i++) 
            {
                if (!ItemsUID.Contains(currentItems[i].UID)) 
                {
                    ItemsUID.Add(currentItems[i].UID);
                    ItemsHandler.Add(currentItems[i]);
                    ClientSend.SendInventoryItem(new ModItem(currentItems[i]), true);
                }
            }
        }
        public static void AddItemGroupNew()
        {
            if (ItemsUID.Count == 0)
                ItemsUID.Add(1);

            var localInventory = ClientData.localInventory.groups;

            currentGroupItems.Clear();
            foreach (GroupItem item in localInventory)
            {
                currentGroupItems.Add(item);
            }

            foreach (GroupItem item in currentGroupItems) 
            {
                if (!ItemsUID.Contains(item.UID)) 
                {
                    ItemsUID.Add(item.UID);
                    ModGroupItem modItem = new ModGroupItem(item);
                    GroupItemsHandler.Add(item);
                    ClientSend.SendInventoryGroupItem(modItem, true);
                }
            }
        }
        
        public static void RemoveItem()
        {
            currentItemsHandler.Clear();
            foreach (Item item in ItemsHandler)
            {
                currentItemsHandler.Add(item);
            }

            for (int i = 0; i < currentItemsHandler.Count; i++)
            {
                if (!ClientData.localInventory.items.Contains(currentItemsHandler[i]))
                {
                    ItemsUID.Remove(currentItemsHandler[i].UID);
                    ItemsHandler.Remove(currentItemsHandler[i]);
                    ClientSend.SendInventoryItem(new ModItem(currentItemsHandler[i]), false);
                }
            }
        }
        public static void RemoveGroupItem()
        {
            currentGroupItemsHandler.Clear();
            foreach (GroupItem item in GroupItemsHandler)
            {
                currentGroupItemsHandler.Add(item);
            }

            for (int i = 0; i < currentGroupItemsHandler.Count; i++)
            {
                if (!ClientData.localInventory.groups.Contains(currentGroupItemsHandler[i]))
                {
                    ItemsUID.Remove(currentGroupItemsHandler[i].UID);
                    GroupItemsHandler.Remove(currentGroupItemsHandler[i]);
                    ClientSend.SendInventoryGroupItem(new ModGroupItem(currentGroupItemsHandler[i]), false);
                }
            }
        }
        
    }
}