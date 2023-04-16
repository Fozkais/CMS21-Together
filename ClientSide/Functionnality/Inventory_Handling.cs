using System.Collections.Generic;
using CMS21MP.DataHandle;
using Il2Cpp;
using MelonLoader;

namespace CMS21MP.ClientSide.Functionnality
{
    public static class Inventory_Handling
    {
        public static List<Item> currentInventory = new List<Item>();
        public  static List<Item> InventoryHandler = new List<Item>();
        public static List<Item> currentInventoryHandler = new List<Item>();
        public static List<long> ItemsUID = new List<long>();


        public static void HandleInventory()
        {
            AddItem();
            RemoveItem();
        }
        
        
        public static void AddItem()
        {
            if (ItemsUID.Count == 0)
                ItemsUID.Add(1);
            
            var localInventory = MainMod.localInventory.items;

            currentInventory.Clear();
            foreach (Item item in localInventory)
            {
                currentInventory.Add(item);
            }

            for (int i = 0; i < currentInventory.Count; i++) 
            {
                if (!ItemsUID.Contains(currentInventory[i].UID)) 
                {
                    // MelonLogger.Msg($"Founded a new Item ! ID:{currentInventory[i].ID}, UID:{currentInventory[i].UID}");
                    ItemsUID.Add(currentInventory[i].UID);
                    InventoryHandler.Add(currentInventory[i]);
                    ClientSend.PlayerInventory(currentInventory[i], true); // Add new Item
                }
            }
        }
        public static void RemoveItem()
        {
            currentInventoryHandler.Clear();
            foreach (Item item in InventoryHandler)
            {
                currentInventoryHandler.Add(item);
            }

            for (int i = 0; i < currentInventoryHandler.Count; i++)
            {
                if (!MainMod.localInventory.items.Contains(currentInventoryHandler[i]))
                {
                    // MelonLogger.Msg($"Founded a Removed Item ! ID:{currentInventoryHandler[i].ID}, UID:{currentInventoryHandler[i].UID}");
                    ItemsUID.Remove(currentInventoryHandler[i].UID);
                    InventoryHandler.Remove(currentInventoryHandler[i]);
                    ClientSend.PlayerInventory(currentInventoryHandler[i], false);
                }
            }
        }
    }
}