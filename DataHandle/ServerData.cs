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
        public static Dictionary<int, List<Item>> AddItemQueue = new Dictionary<int, List<Item>>();
        public static Dictionary<int, List<Item>> RemoveItemQueue = new Dictionary<int, List<Item>>();
        public static Inventory serverInventory = new Inventory();
        

        public static int serverMoney = GlobalData.PlayerMoney;

        public static void UpdateInventory()
        {
            if (RemoveItemQueue.Count > 0)
            {
                foreach (KeyValuePair<int, List<Item>> element in RemoveItemQueue)
                {
                    for (int i = 0; i < element.Value.Count; i++)
                    {
                        serverInventory.items.Remove(element.Value[i]);
                        ServerSend.PlayerInventory(element.Key,element.Value[i], false);
                        RemoveItemQueue[element.Key].Remove(element.Value[i]);
                    }
                }
            }

            if (AddItemQueue.Count > 0)
            {
                foreach (KeyValuePair<int, List<Item>> element in AddItemQueue)
                {
                    for (int i = 0; i < element.Value.Count; i++)
                    {
                        serverInventory.items.Add(element.Value[i]);
                        ServerSend.PlayerInventory(element.Key,element.Value[i], true);
                        AddItemQueue[element.Key].Remove(element.Value[i]);
                    }
                }
            }
        }
        
    }
}