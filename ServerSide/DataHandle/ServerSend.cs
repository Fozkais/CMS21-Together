using System.Collections.Generic;
using CMS21MP.ClientSide;
using CMS21MP.SharedData;

namespace CMS21MP.ServerSide.DataHandle
{
    public class ServerSend
    {
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        private static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].udp.SendData(_packet);
        }

        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }

        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                    Server.clients[i].tcp.SendData(_packet);
            }
        }

        private static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }

        private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].udp.SendData(_packet);
                }
            }
        }

        public static void Welcome(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)PacketTypes.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
            using (Packet _packet = new Packet((int)PacketTypes.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                SendUDPData(_toClient, _packet);
            }
        }

        public static void DisconnectClient(int id, string _msg)
        {
            using (Packet _packet = new Packet((int)PacketTypes.disconnect))
            {
                _packet.Write(_msg);

                SendTCPData(id, _packet);
            }
        }
        
        public static void SendReadyState(int _fromClient,bool _ready, int _id)
        {
            using (Packet _packet = new Packet((int)PacketTypes.readyState))
            {
                _packet.Write(_ready);
                _packet.Write(_id);

                SendTCPDataToAll(_fromClient, _packet);
            }
        }

        public static void SendPlayersInfo(Player info) 
        {
            using (Packet _packet = new Packet((int)PacketTypes.playerInfo))
            {
                _packet.Write(info);
                // TODO: Handle Disconnection (remove player from dict)

                SendTCPDataToAll(_packet);
            }
        }

        public static void StartGame()
        {
            using (Packet _packet = new Packet((int)PacketTypes.startGame))
            {
                SendTCPDataToAll(Client.Instance.Id, _packet);
            }
        }
    }
}
