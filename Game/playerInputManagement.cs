using System;
using UnityEngine;

namespace CMS21MP
{
    public class playerInputManagement : MonoBehaviour
    {
        public void inputFixedUpdate()
        {
            SendInputToServer();
        }

        private void SendInputToServer()
        {
            // Upgrade to get player input settings
            bool[] _inputs = new bool[]
            {
                Input.GetKey(KeyCode.Z),
                Input.GetKey(KeyCode.S),
                Input.GetKey(KeyCode.Q),
                Input.GetKey(KeyCode.D)
            };
            ClientSend.PlayerMovement(_inputs);
        }
    }
}