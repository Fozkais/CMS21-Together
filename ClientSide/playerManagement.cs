using System.Collections.Generic;
using System.Linq;
using CMS21MP.DataHandle;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide
{
    public class playerManagement : MonoBehaviour
    {
        private Vector3 lastPos = new Vector3(0,0,0);
        private Quaternion lastRot = new Quaternion(0, 0, 0, 0);
        public static List<Item> InventoryHandler = new List<Item>();
        
        // Handler
        public List<Item> currentInventory = new List<Item>();
        public static List<long> ItemsUID = new List<long>();
        public List<Item> currentInventoryHandler = new List<Item>();

        public void playerInfoUpdate()
        {
            SendPositionToServer();
            SendRotationToServer();
            AddItem();
            RemoveItem();
        }

        private void SendPositionToServer()
        {
            Vector3 playerPos = MainMod.localPlayer.transform.position;
            Quaternion playerRot = MainMod.localPlayer.transform.rotation;

            if (Vector3.Distance(playerPos, lastPos) > .05f)
            {
                lastPos = playerPos;
                Vector3 newPlayerPos = new Vector3(playerPos.x, playerPos.y - .8f, playerPos.z);
                ClientSend.PlayerMovement(newPlayerPos);
            }
        }

        private void SendRotationToServer()
        {
            Quaternion playerRot = MainMod.localPlayer.transform.rotation;
            if (Quaternion.Angle(lastRot, playerRot) > .05f)
            {
                lastRot = playerRot;
                ClientSend.PlayerRotation(playerRot);
            }
        }
        private void AddItem()
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
                    MelonLogger.Msg($"Founded a new Item ! ID:{currentInventory[i].ID}, UID:{currentInventory[i].UID}");
                    ItemsUID.Add(currentInventory[i].UID);
                    InventoryHandler.Add(currentInventory[i]);
                    ClientSend.PlayerInventory(currentInventory[i], true); // Add new Item
                }
            }
        }

        private void RemoveItem()
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
                    MelonLogger.Msg($"Founded a Removed Item ! ID:{currentInventoryHandler[i].ID}, UID:{currentInventoryHandler[i].UID}");
                    ItemsUID.Remove(currentInventoryHandler[i].UID);
                    InventoryHandler.Remove(currentInventoryHandler[i]);
                    ClientSend.PlayerInventory(currentInventoryHandler[i], false);
                }
            }
        }
    }
}