using System;
using UnityEngine;

namespace CMS21MP
{
    public class playerInputManagement : MonoBehaviour
    {
        private Vector3 lastPos = new Vector3(0,0,0);
        public void playerPosUpdate()
        {
            SendPositionToServer();
        }

        private void SendPositionToServer()
        {
            Vector3 playerPos = GameObject.Find("First Person Controller").transform.position;

            if (Vector3.Distance(playerPos, lastPos) > 1)
            {
                lastPos = playerPos;
                ClientSend.PlayerMovement(playerPos);
            }
        }
    }
}