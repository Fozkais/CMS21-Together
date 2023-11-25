using System.Collections;
using System.Collections.Generic;
using CMS21MP.ClientSide;
using CMS21MP.ClientSide.Data;
using CMS21MP.CustomData;
using CMS21MP.SharedData;
using Il2Cpp;
using MelonLoader;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CMS21MP.ServerSide.DataHandle
{
    public class ServerSend
    {

        #region Functions
            private static void SendTCPData(int _toClient, Packet _packet)//, SteamId steamId)
            {
                _packet.WriteLength();
                if(Server.clients.ContainsKey(_toClient))
                    Server.clients[_toClient].tcp.SendData(_packet);
            }

            private static void SendUDPData(int _toClient, Packet _packet)//, SteamId steamId)
            {
                _packet.WriteLength();
                if(Server.clients.ContainsKey(_toClient))
                    Server.clients[_toClient].udp.SendData(_packet);
            }

            private static void SendTCPDataToAll(Packet _packet)
            {
                _packet.WriteLength();
                foreach (ServerClient client in Server.clients.Values)
                {
                    client.tcp.SendData(_packet);
                }
            }

            private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
            {
                _packet.WriteLength();

                    foreach (ServerClient client in Server.clients.Values)
                    {
                        if (client.id != _exceptClient)
                            client.tcp.SendData(_packet);
                    }
            }

            private static void SendUDPDataToAll(Packet _packet)
            {
                _packet.WriteLength();
                foreach (ServerClient client in Server.clients.Values)
                {
                        client.udp.SendData(_packet);
                }
            }

            private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
            {
                _packet.WriteLength();
                foreach (ServerClient client in Server.clients.Values)
                {
                    if (client.id != _exceptClient)
                        client.udp.SendData(_packet);
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

                    SendTCPData(_toClient, _packet);
                }
                using (Packet _packet = new Packet((int)PacketTypes.welcome))
                {
                    _packet.Write("UDP Welcome!");
                    _packet.Write(_toClient);

                    SendUDPData(_toClient, _packet);
                }
            }

            public static void DisconnectClient(int id, string _msg)
            {
                using (Packet _packet = new Packet((int)PacketTypes.disconnect))
                {
                    _packet.Write(_msg);
                    _packet.Write(id);

                    SendTCPDataToAll(_packet);
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
                
                /*if(!GameData.DataInitialzed)
                    MelonCoroutines.Start(GameData.InitializeGameData());*/
                
                while (GameData.carLoaders.Length <= 3)
                    yield return  new WaitForEndOfFrame();
                
                yield return new WaitForSeconds(1.5f);
                MelonLogger.Msg("Sending Spawn Player Packets");
                
                foreach (ServerClient _client in Server.clients.Values)
                    {
                        if (ServerData.players.ContainsKey(_client.id))
                        {
                            if (_client.id != Client.Instance.Id)
                            {
                                ServerSend.SpawnPlayer(_client.id, ServerData.players[_client.id]);
                            }
                            ServerSend.SpawnPlayer(Client.Instance.Id, ServerData.players[_client.id]);
                        }
                    }
            }
        #endregion


        #region Player Info
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
            
            public static void SendPlayerSceneChange(int fromClient, string scene)
            {
                using (Packet _packet = new Packet((int)PacketTypes.playerSceneChange))
                {
                    _packet.Write(scene);
                    _packet.Write(fromClient);

                    SendTCPDataToAll(fromClient, _packet);
                }
            }

        #endregion

        #region Car
            public static void CarInfo(int _fromClient, ModCar _car, bool resync=false)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carInfo))
                {
                    _packet.Write(_car);

                    if(!resync)
                        SendTCPDataToAll(_fromClient, _packet);
                    else
                        SendTCPData(_fromClient, _packet);
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
            
            public static void PartScripts(int _fromClient, List<ModPartScript> tempCarScripts, int carCarLoaderID, bool resync=false)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carParts))
                {
                   // MelonLogger.Msg($"Sending: {tempCarScripts.Count} parts");
                    _packet.Write(tempCarScripts);
                    _packet.Write(carCarLoaderID);

                    if(!resync)
                        SendTCPDataToAll(_fromClient, _packet);
                    else
                        SendTCPData(_fromClient, _packet);
                }
            }

            public static void BodyParts(int _fromClient, List<ModCarPart> tempCarParts, int carCarLoaderID, bool resync=false)
            {
                using (Packet _packet = new Packet((int)PacketTypes.bodyParts))
                {
                  //  MelonLogger.Msg($"Sending: {tempCarParts.Count} bodyParts");
                    _packet.Write(tempCarParts);
                    _packet.Write(carCarLoaderID);

                    if (!resync)
                        SendTCPDataToAll(_fromClient, _packet);
                    else
                        SendTCPData(_fromClient, _packet);
                }
            }
            

        #endregion

        #region Inventory
            public static void SendInventoryItem(int _fromClient, ModItem _item, bool _status, bool resync=false)
            {
                using (Packet _packet = new Packet((int)PacketTypes.inventoryItem))
                {
                    _packet.Write(_item);
                    _packet.Write(_status);

                    if(!resync)
                        SendTCPDataToAll(_fromClient, _packet);
                    else
                        SendTCPData(_fromClient, _packet);
                }
            }
            public static void SendInventoryGroupItem(int _fromClient, ModGroupItem _item, bool _status, bool resync=false)
            {
                using (Packet _packet = new Packet((int)PacketTypes.inventoryGroupItem))
                {
                    _packet.Write(_item);
                    _packet.Write(_status);

                    if(!resync)
                        SendTCPDataToAll(_fromClient, _packet);
                    else
                        SendTCPData(_fromClient, _packet);
                }
            }

        

        #endregion

        #region Garage Interaction
            public static void SendLifterPos(int _fromClient, int action, int pos, int loaderid)
            {
                using (Packet _packet = new Packet((int)PacketTypes.lifterPos))
                {
                    _packet.Write(action);
                    _packet.Write(pos);
                    _packet.Write(loaderid);

                    SendTCPDataToAll(_fromClient, _packet);
                }
            }
            public static void SendTireChange(int fromClient, ModGroupItem item, bool instant, bool connect)
            {
                using (Packet _packet = new Packet((int)PacketTypes.tireChanger))
                {
                    _packet.Write(item);
                    _packet.Write(instant);
                    _packet.Write(connect);

                    SendTCPDataToAll(fromClient, _packet);
                }
            }

        

        #endregion

        public static void SendTireChanger_ResetAction(int fromClient)
        {
            using (Packet _packet = new Packet((int)PacketTypes.tireChanger_ResetAction))
            {
                SendTCPDataToAll(fromClient, _packet);
            }
        }

        public static void SendWheelBalancer(int fromClient, ModGroupItem item)
        {
            using (Packet _packet = new Packet((int)PacketTypes.wheelBalancer))
            {
                _packet.Write(item);

                SendTCPDataToAll(fromClient, _packet);
            }
        }

        public static void SendWheelBalancer_ResetAction(int fromClient)
        {
            using (Packet _packet = new Packet((int)PacketTypes.wheelBalancer_ResetAction))
            {
                SendTCPDataToAll(fromClient, _packet);
            }
        }

        public static void keepAlive(int fromclient)
        {
            using (Packet _packet = new Packet((int)PacketTypes.keepAlive))
            {
                SendTCPData(fromclient, _packet);
            }
        }
    }
}
