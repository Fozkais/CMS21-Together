using CMS21MP.ServerSide;
using Il2Cpp;
using UnityEngine;

namespace CMS21MP.DataHandle
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
        private static void SendTCPDataToAll(int _exceptClient,Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                if(i != _exceptClient)
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
            using(Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);
                
                SendTCPData(_toClient, _packet);
            }
        }

        public static void SpawnPlayer(int _toClient, Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.username);
                _packet.Write(_player.position);
                _packet.Write(_player.rotation);
                
                SendTCPData(_toClient, _packet);
            }
        }

        public static void PlayerPosition(int clientId, Vector3 position)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
            {
                _packet.Write(clientId);
                _packet.Write(position);
                
                SendUDPDataToAll(clientId,_packet);
            }
        }

        public static void PlayerRotation(int clientId, Quaternion rotation)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
            {
                _packet.Write(clientId);
                _packet.Write(rotation);
                
                SendUDPDataToAll(clientId, _packet);
            }
        }

        public static void PlayerConnected(int _connectedPlayers, int _maxPlayers)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerConnected))
            {
                _packet.Write(_connectedPlayers);
                _packet.Write(_maxPlayers);
                
                SendTCPDataToAll(_packet);
            }
        }

        public static void PlayerDisconnect(int id)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerDisconnect))
            {
                _packet.Write(id);

                SendTCPDataToAll(_packet);
            }
            
        }

        public static void PlayerInventory(int clientId, Item item)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerInventory))
            {
                _packet.Write(item.ID);
                _packet.Write(item.Condition);
                _packet.Write(item.Quality);

                SendTCPDataToAll(clientId,_packet);
            }
        }
    }
}