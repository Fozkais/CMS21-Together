using System.Collections.Generic;
using System.Linq;
using CMS21MP.DataHandle;
using Il2Cpp;
using MelonLoader;

namespace CMS21MP.ClientSide.Functionnality
{
    public static class Inventory
    {
        public static List<Item> currentItems = new List<Item>();
        public  static List<Item> ItemsHandler = new List<Item>();
        public static List<Item> currentItemsHandler = new List<Item>();
        
        public static List<GroupItem> currentGroupItems = new List<GroupItem>();
        public static List<GroupItem> GroupItemsHandler = new List<GroupItem>();
        public static List<GroupItem> currentGroupItemsHandler = new List<GroupItem>();
        

        public static List<long> ItemsUID = new List<long>();

        public static void HandleInventory()
        {
            AddItem();
            RemoveItem();
            AddItemGroupNew();
            RemoveGroupItemNew();
        }
        
        
        public static void AddItem()
        {
            if (ItemsUID.Count == 0)
                ItemsUID.Add(1);
            
            var localInventory = MainMod.localInventory.items;

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
                    ClientSend.SendItem(new ModItem(currentItems[i]), true);
                }
            }
        }
        public static void AddItemGroupNew()
        {
            if (ItemsUID.Count == 0)
                ItemsUID.Add(1);

            var localInventory = MainMod.localInventory.groups;

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
                    ModItemGroup modItem = new ModItemGroup(item, currentGroupItems.IndexOf(item));
                    GroupItemsHandler.Add(item);
                    ClientSend.SendGroupItem(modItem, true);
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
                if (!MainMod.localInventory.items.Contains(currentItemsHandler[i]))
                {
                    ItemsUID.Remove(currentItemsHandler[i].UID);
                    ItemsHandler.Remove(currentItemsHandler[i]);
                    ClientSend.SendItem(new ModItem(currentItemsHandler[i]), false);
                }
            }
        }
        public static void RemoveGroupItemNew()
        {
            currentGroupItemsHandler.Clear();
            foreach (GroupItem item in GroupItemsHandler)
            {
                currentGroupItemsHandler.Add(item);
            }

            for (int i = 0; i < currentGroupItemsHandler.Count; i++)
            {
                if (!MainMod.localInventory.groups.Contains(currentGroupItemsHandler[i]))
                {
                    ItemsUID.Remove(currentGroupItemsHandler[i].UID);
                    GroupItemsHandler.Remove(currentGroupItemsHandler[i]);
                    ClientSend.SendGroupItem(new ModItemGroup(currentGroupItemsHandler[i], i), false);
                }
            }
        }
    }
}