using System;
using CMS21MP.ClientSide;
using CMS21MP.DataHandle;
using Il2Cpp;
using UnityEngine;

namespace CMS21MP.ClientSide
{
    public class playerManagement : MonoBehaviour
    {
        private Vector3 lastPos = new Vector3(0,0,0);
        private Quaternion lastRot = new Quaternion(0, 0, 0, 0);
        private Inventory lastInventory = new Inventory();
        public void playerInfoUpdate()
        {
            SendPositionToServer();
            SendRotationToServer();
            SendInventoryToServer();
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

        private void SendInventoryToServer()
        {
            Inventory inventorySize = GameScript.Get().GetComponent<Inventory>();
            foreach (Item element in inventorySize.items)
            {
                if (lastInventory.items.Contains(element))
                {
                    return;
                }
                else
                {
                    ClientSend.PlayerInventory(element);
                    lastInventory.items.Add(element);
                }
            }
        }
    }
}