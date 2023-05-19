using System;
using System.Collections.Generic;
using System.Linq;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.DataHandle
{
    public class ServerData
    {
        public static Dictionary<int, List<ModItem>> ItemAddQueue = new Dictionary<int, List<ModItem>>();
        public static Dictionary<int, List<ModItem>> ItemRemoveQueue = new Dictionary<int, List<ModItem>>();
        public static Dictionary<int, List<ModItemGroup>> GroupItemAddQueue = new Dictionary<int, List<ModItemGroup>>();
        public static Dictionary<int, List<ModItemGroup>> GroupItemRemoveQueue = new Dictionary<int, List<ModItemGroup>>();
        public static Inventory serverInventory = new Inventory();

        public static List<carData> carList = new List<carData>();
        public static List<carData> carListHandle = new List<carData>();

        public static int serverMoney = GlobalData.PlayerMoney;

        public static void UpdateItems()
        {
            if (ItemRemoveQueue.Count > 0)
            {
                foreach (KeyValuePair<int, List<ModItem>> element in ItemRemoveQueue)
                {
                    for (int i = 0; i < element.Value.Count; i++)
                    {
                        ModItem item = new ModItem();
                        Item newItem = new Item();
                        if (element.Value[i] != null)
                        {
                            item.ToGame(element.Value[i], newItem);
                            serverInventory.items.Remove(newItem);
                            ServerSend.SendItem(element.Key, element.Value[i], false, 0);
                            ItemRemoveQueue[element.Key].Remove(element.Value[i]);
                        }
                    }
                }
            }

            if (ItemAddQueue.Count > 0)
            {
                foreach (KeyValuePair<int, List<ModItem>> element in ItemAddQueue)
                {
                    for (int i = 0; i < element.Value.Count; i++)
                    {
                        ModItem item = new ModItem();
                        Item newItem = new Item();
                        if (element.Value[i] != null)
                        {
                            item.ToGame(element.Value[i], newItem);
                            serverInventory.items.Add(newItem);
                            ServerSend.SendItem(element.Key, element.Value[i], true, 0);
                            ItemAddQueue[element.Key].Remove(element.Value[i]);
                        }
                    }
                }
            }
        }
        public static void UpdateGroupItems()
        {
            if (GroupItemRemoveQueue.Count > 0)
            {
                foreach (KeyValuePair<int, List<ModItemGroup>> element in GroupItemRemoveQueue)
                {
                    for (int i = 0; i < element.Value.Count; i++)
                    {
                        ModItemGroup modItemGroup = new ModItemGroup();
                        GroupItem groupItem = new GroupItem();
                        if (element.Value[i] != null)
                        {
                            modItemGroup.ToGame(element.Value[i], groupItem);
                            serverInventory.groups.Remove(groupItem);
                            ServerSend.SendGroupItem(element.Key, element.Value[i], false, 0);
                            GroupItemRemoveQueue[element.Key].Remove(element.Value[i]);
                        }
                    }
                }
            }

            if (GroupItemAddQueue.Count > 0)
            {
                foreach (KeyValuePair<int, List<ModItemGroup>> element in GroupItemAddQueue)
                {
                    for (int i = 0; i < element.Value.Count; i++)
                    {
                        ModItemGroup modItemGroup = new ModItemGroup();
                        GroupItem groupItem = new GroupItem();
                        if (element.Value[i] != null)
                        {
                            modItemGroup.ToGame(element.Value[i], groupItem);
                            serverInventory.groups.Add(groupItem);
                            ServerSend.SendGroupItem(element.Key, element.Value[i], true, 0);
                            GroupItemAddQueue[element.Key].Remove(element.Value[i]);
                        }
                    }
                }
            }
        }
        
    }
}