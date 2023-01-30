using System.Collections.Generic;
using CMS21MP.ClientSide;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.DataHandle
{
    public static class DataUpdating
    {
        public static Dictionary<int, List<Vector3>> MovUpdateQueue = new Dictionary<int, List<Vector3>>();
        public static Dictionary<int, List<Quaternion>> RotUpdateQueue = new Dictionary<int, List<Quaternion>>();

        public static void UpdateData()
        {
            if (MainMod.isConnected)
            {
                MainMod.localPlayer.GetComponent<playerManagement>().playerInfoUpdate();
                UpdatePlayerMovement();
                UpdatePlayerRotation();

                if (MainMod.isHosting)
                {
                    ServerData.UpdateInventory();
                }
            }

        }

        public static void UpdatePlayerMovement()
        {
            if (MovUpdateQueue.Count > 0)
            {
                foreach (KeyValuePair<int, List<Vector3>> element in MovUpdateQueue)
                {
                    for (int i = 0; i < element.Value.Count; i++)
                    {
                        ServerSend.PlayerPosition(element.Key, element.Value[i]);
                        MovUpdateQueue[element.Key].Remove(element.Value[i]);
                    }
                }
            }
        }

        public static void UpdatePlayerRotation()
        {
            if (RotUpdateQueue.Count > 0)
            {
                foreach (KeyValuePair<int, List<Quaternion>> element in RotUpdateQueue)
                {
                    for (int i = 0; i < element.Value.Count; i++)
                    {
                        ServerSend.PlayerRotation(element.Key, element.Value[i]);
                        RotUpdateQueue[element.Key].Remove(element.Value[i]);
                    }
                }
            }
        }
        
    }
}