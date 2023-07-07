using System;
using System.Net;
using System.Net.Sockets;
using CMS21MP.ServerSide.DataHandle;
using CMS21MP.ServerSide.Transport;
using UnityEngine;
using MelonLoader;

namespace CMS21MP.ServerSide
{
    public class ServerClient
    {
        public int id;
        public Player player;
        public bool isHosting = false;
        
        public ServerTCP tcp;
        public ServerUDP udp;

        public ServerClient(int _clientId)
        {
            id = _clientId;
            tcp = new ServerTCP(id);
            udp = new ServerUDP(id);
        }


        public void SendIntoGame(string _playerName)
        {
            player = new Player(id, _playerName, new Vector3(0, 0, 0));

            for (int i = 1; i < Server.clients.Count; i++)
            {
                var client = Server.clients[i];
                if (client.player != null)
                    ServerSend.SendPlayersInfo(client.player);
            }
            
            /*foreach (ServerClient _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    if (_client.id != id)
                    {
                        /*ServerSend.SpawnPlayer(id, _client.player); TODO: Send spawn packet
                        ServerSend.SendPlayersInfo(player);
                    }
                    /*ServerSend.SpawnPlayer(_client.id, player); #1#
                }
            }*/
        }

        public void Disconnect(int _id)
        {
            MelonLogger.Msg($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

            if(Server.clients.ContainsKey(_id))
                Server.clients.Remove(_id);

            player = null;
            tcp.Disconnect();
            udp.Disconnect();
        }
    }
}