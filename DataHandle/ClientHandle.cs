using CMS21MP.ClientSide;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.DataHandle
{
    public class ClientHandle : MonoBehaviour
    {
        public static void Welcome(Packet _packet)
        {
            string _msg = _packet.ReadString();
            int _myId = _packet.ReadInt();

            MelonLogger.Msg($"Message from Server:{_msg}");
            Client.Instance.Id = _myId;

            ClientSend.WelcomeReceived();
        }
        
    }
}