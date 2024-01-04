using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using CMS21Together.BothSide;
using CMS21Together.ServerSide.DataHandle;
using CMS21Together.ServerSide.Transport;
using UnityEngine;
using MelonLoader;

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

        public IEnumerator isClientAlive()
        {
            if(!ServerData.isRunning)
                yield break;
            
            if (Alive)
            {
                yield return new WaitForSeconds(5);
                Alive = false;
            }
            else
            {
                yield return new WaitForSeconds(8);
                if (!Alive)
                {
                    if(!ServerData.isRunning)
                        yield break;

                    if (ServerData.players.ContainsKey(id))
                    {
                        MelonLogger.Msg($"SV:  Client[{id}], username:{ServerData.players[id].username} no longer alive! Disconnecting...");
                        ServerSend.DisconnectClient(id, $"{ServerData.players[id].username} as disconnected!");
                    }
                    
                    if(Server.clients.ContainsKey(id))
                        Server.clients[id].Disconnect(id);
                }
            }
            if(ServerData.players[id] != null)
                MelonCoroutines.Start(isClientAlive());
        }


        public void SendToLobby(string _playerName)
        {
            ServerData.players[id] = new Player(id, _playerName, new Vector3(0, 0, 0));
            MelonLogger.Msg($"SV: New player ! {_playerName}, ID:{id}");

            /*for (int i = 1; i < Server.clients.Count; i++)
            {
                var client = Server.clients[i];
                if (ServerData.players.ContainsKey(client.id))
                {
                    ServerData.players[client.id] = ServerData.players[id];   broken / useless ?
                }
            }
            foreach (ServerClient _client in Server.clients.Values)
            {
                if (ServerData.players.ContainsKey(_client.id))
                {
                    if (_client.id != id)
                    {
                        ServerData.players[_client.id] = ServerData.players[id];
                       // ServerSend.SendPlayersInfo(ServerData.players[id]);
                    }
                }
            }*/
            
            ServerSend.SendPlayersInfo(ServerData.players);
        }

        public void Disconnect(int _id)
        {
            MelonLogger.Msg($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");
            ServerSend.DisconnectClient(_id, $"{ ServerData.players[_id].username} as disconnected!");
            if (Server.clients.ContainsKey(_id))
                Server.clients.Remove(_id);
            
            if(ServerData.players.ContainsKey(_id))
                ServerData.players.Remove(_id);
            tcp.Disconnect();
            
            udp.Disconnect();
        }
    }
}