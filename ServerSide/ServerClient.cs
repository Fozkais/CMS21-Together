using System.Collections;
using CMS21Together.ServerSide.Data;
using CMS21Together.ServerSide.Handle;
using CMS21Together.ServerSide.Transport;
using CMS21Together.Shared;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ServerSide
{
    public class ServerClient
    {
        public int id;
        public bool isHosting = false;
        public bool Alive = true;
        
        public ServerTCP tcp;
        public ServerUDP udp;

        public ServerClient(int _clientId)
        {
            id = _clientId;
            tcp = new ServerTCP(id);
            udp = new ServerUDP(id);
        }


        public void SendToLobby(string _playerName)
        {
            ServerData.players[id] = new Player(id, _playerName, new Vector3(0, 0, 0));
            MelonLogger.Msg($"SV: New player ! {_playerName}, ID:{id}");
            
            ServerSend.SendPlayersInfo(ServerData.players);
        }

        public void Disconnect(int _id)
        {
            MelonLogger.Msg($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");
            if (ServerData.players.TryGetValue(_id, out var player))
                ServerSend.DisconnectClient(_id, $"{ player.username} as disconnected!");
            if (Server.clients.ContainsKey(_id))
                Server.clients.Remove(_id);
            
            if(ServerData.players.ContainsKey(_id))
                ServerData.players.Remove(_id);
            
            tcp.Disconnect();
            udp.Disconnect();
        }
    }
}