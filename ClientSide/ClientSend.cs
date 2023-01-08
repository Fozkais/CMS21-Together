using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide
{
    public static class ClientSend
    {
        private static void SendTCPData(Packet _packet)
        {
            _packet.WriteLength();
            Client.instance.tcp.SendData(_packet);
        }
        
        private static void SendUDPData(Packet _packet)
        {
            _packet.WriteLength();
            Client.instance.udp.SendData(_packet);
        }
        
        public static void WelcomeReceived()
        {
            using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
            {
                _packet.Write(Client.instance.myId);
                _packet.Write(ModGUI.instance.usernameField);
                
                SendTCPData(_packet);
            }
            
        }

        public static void PlayerMovement(Vector3 _position)
        {
            using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
            {
                _packet.Write(Client.instance.myId);
                _packet.Write(_position);
                _packet.Write(GameManager.players[Client.instance.myId].transform.rotation);
                
               // MelonLogger.Msg($"Sending playerPos to server!");
                SendUDPData(_packet);
            }
        }
    }
    
}