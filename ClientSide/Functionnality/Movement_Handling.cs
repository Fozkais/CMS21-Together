using System.Collections.Generic;
using CMS21MP.DataHandle;
using UnityEngine;

namespace CMS21MP.ClientSide.Functionnality
{
    public static class Movement_Handling
    {
        public static Dictionary<int, List<Vector3>> MovUpdateQueue = new Dictionary<int, List<Vector3>>();
        public static Dictionary<int, List<Quaternion>> RotUpdateQueue = new Dictionary<int, List<Quaternion>>();
        private static Vector3 lastPos = new Vector3(0,0,0);
        private static Quaternion lastRot = new Quaternion(0, 0, 0, 0);

        public static void HandleMovement()
        {
            SendPositionToServer();
            SendRotationToServer();
            UpdatePlayerMovement();
            UpdatePlayerRotation();
        }

        public static void SendPositionToServer()
        {
            Vector3 playerPos = MainMod.localPlayer.transform.position;

            if (Vector3.Distance(playerPos, lastPos) > .05f)
            {
                lastPos = playerPos;
                Vector3 newPlayerPos = new Vector3(playerPos.x, playerPos.y - .8f, playerPos.z);
                ClientSend.PlayerMovement(newPlayerPos);
            }
        }

        public static void SendRotationToServer()
        {
            Quaternion playerRot = MainMod.localPlayer.transform.rotation;
            if (Quaternion.Angle(lastRot, playerRot) > .05f)
            {
                lastRot = playerRot;
                ClientSend.PlayerRotation(playerRot);

                
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