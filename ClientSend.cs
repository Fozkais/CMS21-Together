using MelonLoader;
using UnityEngine;

namespace CMS21MP
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
        public static void UDPTestReceived()
        {
            using (Packet _packet = new Packet((int)ClientPackets.udpTestReceived))
            {
                _packet.Write("Received a UDP packet.");

                SendUDPData(_packet);
            }
        }

        public static void PlayerMovement(bool[] _inputs)
        {
            using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
            {
                _packet.Write(_inputs.Length);
                foreach (bool input in _inputs)
                {
                    _packet.Write(input);
                }
                _packet.Write(GameManager.players[Client.instance.myId].transform.rotation);
                
                SendUDPData(_packet);
            }
        }
    }
    
}