using System.Net;
using CMS21MP.SharedData;

namespace CMS21MP.ClientSide.DataHandle
{
    public static class ClientSend
    {
        private static void SendTCPData(Packet _packet)
        {
            _packet.WriteLength();
            Client.Instance.tcp.SendData(_packet);
        }
        
        private static void SendUDPData(Packet _packet)
        {
            _packet.WriteLength();
            Client.Instance.udp.SendData(_packet);
        }
        
        public static void WelcomeReceived()
        {
            using (Packet _packet = new Packet((int)PacketTypes.welcome))
            {
                _packet.Write(Client.Instance.Id);
                _packet.Write(ModUI.Instance.username);
                
                SendTCPData(_packet);
            }
           // Client.Instance.udp.Connect(((IPEndPoint)Client.Instance.tcp.socket.Client.LocalEndPoint).Port);
        }

        public static void SendReadyState(bool b, int number)
        {
            using (Packet _packet = new Packet((int)PacketTypes.readyState))
            {
                _packet.Write(b);
                _packet.Write(number);
                
                SendTCPData(_packet);
            }
        }
        
        public static void Disconnect(int id)
        {
            using (Packet _packet = new Packet((int)PacketTypes.disconnect))
            {
                _packet.Write(id);
                
                SendTCPData(_packet);
            }
        }
    }
    
}