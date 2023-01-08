using System;
using CMS21MP.ClientSide;
using UnityEngine;

namespace CMS21MP.ClientSide
{
    public class playerInputManagement : MonoBehaviour
    {
        private Vector3 lastPos = new Vector3(0,0,0);
        private Quaternion lastRot = new Quaternion(0, 0, 0, 0);
        public void playerPosUpdate()
        {
            SendPositionToServer();
            SendRotationToServer();
        }

        private void SendPositionToServer()
        {
            Vector3 playerPos = GameObject.Find("First Person Controller").transform.position;
            Quaternion playerRot = GameObject.Find("First Person Controller").transform.rotation;

            if (Vector3.Distance(playerPos, lastPos) > .05f)
            {
                lastPos = playerPos;
                Vector3 newPlayerPos = new Vector3(playerPos.x, playerPos.y - .8f, playerPos.z);
                ClientSend.PlayerMovement(newPlayerPos);
            }
        }

        private void SendRotationToServer()
        {
            Quaternion playerRot = GameObject.Find("First Person Controller").transform.rotation;
            if (Quaternion.Angle(lastRot, playerRot) > .05f)
            {
                lastRot = playerRot;
                ClientSend.PlayerRotation(playerRot);
            }
        }
    }
}