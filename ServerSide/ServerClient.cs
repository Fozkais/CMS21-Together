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

        public IEnumerator isClientAlive()
        {
            if(!ServerData.isRunning)
                yield break;
            
            if (Alive)
            {
                yield return new WaitForSeconds(2);
                Alive = false;
            }
            else
            {
                yield return new WaitForSeconds(3);
                if (!Alive)
                {
                    if(!ServerData.isRunning)
                        yield break;

                    if (ServerData.players.ContainsKey(id))
                    {
                        MelonLogger.Msg($"SV:  Client[{id}], username:{ServerData.players[id].username} asn't responded for to long!");
                        ServerSend.DisconnectClient(id, $"{ServerData.players[id].username} as disconnected!");
                    }
                    if(Server.clients.ContainsKey(id))
                        Server.clients[id].Disconnect(id);
                    
                    yield break;
                }
            }

            if (ServerData.players.TryGetValue(id, out var player))
            {
                if (player != null)
                    MelonCoroutines.Start(isClientAlive());
            }
        }


        public void SendToLobby(string _playerName)
        {
            ServerData.players[id] = new Player(id, _playerName, new Vector3(0, 0, 0));
            MelonLogger.Msg($"SV: New player ! {_playerName}, ID:{id}");
            
            MelonCoroutines.Start(isClientAlive());
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