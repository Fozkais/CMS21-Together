using System.Collections;
using System.Collections.Generic;
using CMS21MP.ClientSide;
using CMS21MP.ClientSide.Data;
using CMS21MP.CustomData;
using CMS21MP.SharedData;
using MelonLoader;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CMS21MP.ServerSide.DataHandle
{
    public class ServerSend
    {

        #region Functions
            private static void SendTCPData(int _toClient, Packet _packet, SteamId steamId)
            {
                _packet.WriteLength();
                if (MainMod.usingSteamAPI)
                {
                    PacketHandling.SendPacket(_packet, steamId);
                    MelonLogger.Msg("Send packet?");
                }
                else
                {
                    Server.clients[_toClient].tcp.SendData(_packet);
                }
            }

            private static void SendUDPData(int _toClient, Packet _packet, SteamId steamId)
            {
                _packet.WriteLength();
                if (MainMod.usingSteamAPI)
                {
                    PacketHandling.SendPacket(_packet, steamId);
                }
                else
                {
                    Server.clients[_toClient].udp.SendData(_packet);
                }
            }

            private static void SendTCPDataToAll(Packet _packet)
            {
                _packet.WriteLength();
                if (MainMod.usingSteamAPI)
                {
                   // PacketHandling.SendPacket(_packet, CallbackHandler.lobbyID);
                }
                else
                {
                    for (int i = 1; i < Server.MaxPlayers; i++)
                    {
                        Server.clients[i].tcp.SendData(_packet);
                    }
                }
            }

            private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
            {
                _packet.WriteLength();
                if (MainMod.usingSteamAPI)
                {
                  //  PacketHandling.SendPacket(_packet, CallbackHandler.lobbyID);
                }
                else
                {
                    for (int i = 1; i < Server.MaxPlayers; i++)
                    {
                        if (i != _exceptClient)
                            Server.clients[i].tcp.SendData(_packet);
                    }
                }
            }

            private static void SendUDPDataToAll(Packet _packet)
            {
                _packet.WriteLength();
                if (MainMod.usingSteamAPI)
                {
                  //  PacketHandling.SendPacket(_packet, CallbackHandler.lobbyID);
                }
                else
                {
                    for (int i = 1; i <= Server.MaxPlayers; i++)
                    {
                        Server.clients[i].udp.SendData(_packet);
                    }
                }
            }

            private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
            {
                _packet.WriteLength();
                if (MainMod.usingSteamAPI)
                {
                  //  PacketHandling.SendPacket(_packet, CallbackHandler.lobbyID);
                }
                else
                {
                    for (int i = 1; i <= Server.MaxPlayers; i++)
                    {
                        if (i != _exceptClient)
                        {
                            Server.clients[i].udp.SendData(_packet);
                        }
                    }
                }
            }
        #endregion

        #region Lobby
        
            public static void Welcome( SteamId steamId, int _toClient=-1, string _msg="welcome to the server!")
            {
                using (Packet _packet = new Packet((int)PacketTypes.welcome))
                {
                    _packet.Write(_msg);
                    _packet.Write(_toClient);

                    SendTCPData(_toClient, _packet, steamId);
                }
                using (Packet _packet = new Packet((int)PacketTypes.welcome))
                {
                    _packet.Write("UDP Welcome!");
                    _packet.Write(_toClient);

                    SendUDPData(_toClient, _packet, steamId);
                }
            }

            public static void DisconnectClient(int id, string _msg)
            {
                using (Packet _packet = new Packet((int)PacketTypes.disconnect))
                {
                    _packet.Write(_msg);

                   // SendTCPData(id, _packet);
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
                MelonCoroutines.Start(DelaySpawnPlayer());
            }
            
            public static void SpawnPlayer(int id,Player player) 
            {
                using (Packet _packet = new Packet((int)PacketTypes.spawnPlayer))
                {
                    _packet.Write(player);
                    _packet.Write(id);

                    SendTCPDataToAll(_packet);
                }
            }

            public static IEnumerator DelaySpawnPlayer()
            {
                MelonLogger.Msg("Waiting for Scene Change");
                while (SceneManager.GetActiveScene().name != "garage")
                    yield return  new WaitForEndOfFrame();
                
                if(!GameData.DataInitialzed)
                    GameData.InitializeGameData();
                
                while (GameData.carLoaders.Length <= 3)
                    yield return  new WaitForEndOfFrame();
                
                MelonLogger.Msg("Sending Spawn Player Packets");
                yield return new WaitForSeconds(1.5f);
                
                foreach (ServerClient _client in Server.clients.Values)
                    {
                        if (_client.player != null)
                        {
                            if (_client.id != Client.Instance.Id)
                            {
                                ServerSend.SpawnPlayer(_client.id, _client.player);
                            }
                            ServerSend.SpawnPlayer(Client.Instance.Id, _client.player);
                        }
                    }
            }
        #endregion


        #region Movement and Rotation
            public static void SendPosition(int fromClient, Vector3Serializable position)
            {
                using (Packet _packet = new Packet((int)PacketTypes.playerPosition))
                {
                    _packet.Write(fromClient);
                    _packet.Write(position);

                    SendUDPDataToAll(fromClient, _packet);
                    //MelonLogger.Msg("sending position to all clients...");
                }
            }
            
            public static void SendRotation(int fromClient, QuaternionSerializable rotation)
            {
                using (Packet _packet = new Packet((int)PacketTypes.playerRotation))
                {
                    _packet.Write(fromClient);
                    _packet.Write(rotation);

                    SendUDPDataToAll(fromClient, _packet);
                }
            }

        #endregion

        #region Car
            public static void CarInfo(int _fromClient, ModCar _car)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carInfo))
                {
                    _packet.Write(_car);

                    SendTCPDataToAll(_fromClient, _packet);
                }
            }
            public static void CarPosition(int _fromClient, int _carLoaderID, int _carPosition)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carPosition))
                {
                    _packet.Write(_carLoaderID);
                    _packet.Write(_carPosition);

                    SendTCPDataToAll(_fromClient, _packet);
                }
            }
            public static void CarPart(int _fromClient, int _carLoaderID, ModPartScript _carPart)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carPart))
                {
                    _packet.Write(_carLoaderID);
                    _packet.Write(_carPart);

                    SendTCPDataToAll(_fromClient, _packet);
                }
            }
            public static void BodyPart(int _fromClient, int _carLoaderID, ModCarPart _carPart)
            {
                using (Packet _packet = new Packet((int)PacketTypes.bodyPart))
                {
                    _packet.Write(_carLoaderID);
                    _packet.Write(_carPart);

                    SendTCPDataToAll(_fromClient, _packet);
                }
            }
            

        #endregion

    }
}
