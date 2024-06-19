using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CMS21Together.ServerSide.Data;
using CMS21Together.ClientSide;
using CMS21Together.ClientSide.Data;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CMS21Together.ServerSide.Handle
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
        
            public static void Welcome(int _toClient=-1, string _msg="welcome to the server!")
            {
                using (Packet _packet = new Packet((int)PacketTypes.welcome))
                {
                    _packet.Write(_msg);
                    _packet.Write(_toClient);

                    SendTCPData(_toClient, _packet);
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
            
            public static void ContentInfo(Dictionary<string, bool> infos)
            {
                using (Packet _packet = new Packet((int)PacketTypes.contentInfo))
                {
                    _packet.Write(new ReadOnlyDictionary<string,bool>(infos));

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
            
            public static void SendPlayerInfo(Player info) 
            {
                using (Packet _packet = new Packet((int)PacketTypes.playerInfo))
                {
                    _packet.Write(info);
                   // MelonLogger.Msg("SV: Sended a Player Info!");

                    SendTCPDataToAll(_packet);
                }
            }

            public static void SendPlayersInfo(Dictionary<int, Player> info) 
            {
                using (Packet _packet = new Packet((int)PacketTypes.playersInfo))
                {
                    _packet.Write(info);
                   // MelonLogger.Msg("SV: Sended All Players Info!");

                    SendTCPDataToAll(_packet);
                }
            }

            public static void StartGame(ModProfileData saveData)
            {
                using (Packet _packet = new Packet((int)PacketTypes.startGame))
                {
                    if(saveData != null)
                        _packet.Write(saveData);
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

                while (GameData.Instance.carLoaders.Length <= 3)
                    yield return new WaitForSeconds(0.1f);
                
                yield return new WaitForSeconds(1.5f);
                MelonLogger.Msg("Sending Spawn Player Packets");
                
                foreach (ServerClient _client in Server.clients.Values)
                    {
                        if (ServerData.players.TryGetValue(_client.id, out var player))
                        {
                            /*if (_client.id != Client.Instance.Id)
                            {
                                ServerSend.SpawnPlayer(_client.id, ServerData.players[_client.id]);
                            }*/
                            ServerSend.SpawnPlayer(_client.id, player);
                        }
                    }
            }
            
            public static void keepAlive(int fromclient)
            {
                using (Packet _packet = new Packet((int)PacketTypes.keepAlive))
                {
                    SendTCPData(fromclient, _packet);
                }
            }
            
            public static void SendKeepAliveConfirmation(int fromclient)
            {
                using (Packet packet = new Packet((int)PacketTypes.keepAliveConfirmed))
                {
                    SendTCPData(fromclient, packet);
                }
            }
            
        #endregion

        #region PlayerData
        
            public static void SendInitialPosition(int _fromClient, Vector3Serializable _position)
            {
                using (Packet _packet = new Packet((int)PacketTypes.playerInitialPos))
                {
                    _packet.Write(_fromClient);
                    _packet.Write(_position);
                    
                    SendUDPDataToAll(_fromClient, _packet);
                }
            }
        
            public static void SendPosition(int fromClient, Vector3Serializable position)
            {
                using (Packet _packet = new Packet((int)PacketTypes.playerPosition))
                {
                    _packet.Write(fromClient);
                    _packet.Write(position);
                    
                    SendUDPDataToAll(fromClient, _packet);
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
                
            public static void SendPlayerSceneChange(int fromClient, GameScene scene)
            {
                using (Packet _packet = new Packet((int)PacketTypes.playerSceneChange))
                {
                    _packet.Write(fromClient);
                    _packet.Write(scene);

                    SendTCPDataToAll(fromClient, _packet);
                }
            }

            public static void SendStats(int fromClient, int value, ModStats type)
            {
                using (Packet _packet = new Packet((int)PacketTypes.stats))
                {
                    _packet.Write(value);
                    _packet.Write(type);

                    SendTCPDataToAll(fromClient, _packet);
                }
            }
            
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

        #region GarageInteraction
            public static void SendLifter(int fromClient, ModLifterState state, int carLoaderID)
            {
                using (Packet _packet = new Packet((int)PacketTypes.lifter))
                {
                    _packet.Write(state);
                    _packet.Write(carLoaderID);

                    SendTCPDataToAll(fromClient, _packet);
                }
            }
            public static void SendTireChange(int fromClient, ModGroupItem item, bool instant, bool connect, bool resetAction)
            {
                using (Packet _packet = new Packet((int)PacketTypes.tireChanger))
                {
                    _packet.Write(item);
                    _packet.Write(instant);
                    _packet.Write(connect);
                    _packet.Write(resetAction);

                    SendTCPDataToAll(fromClient, _packet);
                }
            }
            public static void WheelBalancer(int fromClient, ModWheelBalancerActionType aType, ModGroupItem item=null)
            {
                using (Packet _packet = new Packet((int)PacketTypes.wheelBalancer))
                {
                    _packet.Write(aType);
                    if(item != null)   {_packet.Write(item);}

                    SendTCPDataToAll(fromClient, _packet);
                }
            }
            
            public static void SendEngineAngle(int fromClient, int newAngle)
            {
                using (Packet _packet = new Packet((int)PacketTypes.engineStandAngle))
                {
                    _packet.Write(newAngle);

                    SendTCPDataToAll(fromClient, _packet);
                }
            }
            
            public static void SendEngineGroup(int fromClient, ModGroupItem engineItem, Vector3Serializable pos, QuaternionSerializable rot, bool resync=false)
            {
                using (Packet _packet = new Packet((int)PacketTypes.setGroupEngineOnStand))
                {
                    _packet.Write(engineItem);
                    _packet.Write(pos);
                    _packet.Write(rot);
                        
                    if(!resync)
                        SendTCPDataToAll(fromClient, _packet);
                    else
                        SendTCPData(fromClient, _packet);
                }
                MelonLogger.Msg("Send EngineGroup");
            }
            
            public static void SendEngineOnStand(int fromClient, ModItem engineItem)
            {
                using (Packet _packet = new Packet((int)PacketTypes.setEngineOnStand))
                {
                    _packet.Write(engineItem);

                    SendTCPDataToAll(fromClient, _packet);
                }
            }
            
            public static void TakeOffEngineFromStand(int _fromClient)
            {
                using (Packet packet = new Packet((int)PacketTypes.takeOffEngineFromStand))
                {
                    SendTCPDataToAll(_fromClient, packet);
                }
            }
            
            
            public static void EngineCrane(int _fromClient, int carLoaderID=-1,ModGroupItem item=null)
            {
                using (Packet _packet = new Packet((int)PacketTypes.engineCrane))
                {
                    if (carLoaderID == -1)
                    {
                        _packet.Write(false);
                        _packet.Write(item);
                    }
                    else
                    {
                        _packet.Write(true);
                        _packet.Write(carLoaderID);
                    }

                    SendTCPDataToAll(_fromClient, _packet);
                }
            }
            
            public static void SendSpringClampGroup(int _fromClient, ModGroupItem item, bool instant, bool mount)
            {
                using (Packet _packet = new Packet((int)PacketTypes.springClampGroup))
                {
                    _packet.Write(item);
                    _packet.Write(instant);
                    _packet.Write(mount);

                    SendTCPDataToAll(_fromClient, _packet);
                }
            }
            
            public static void SendOilBin(int fromClient, int loaderID)
            {
                using (Packet _packet = new Packet((int)PacketTypes.oilBin))
                {
                    _packet.Write(loaderID);

                    SendTCPDataToAll(fromClient, _packet);
                }
            }
            
            
            public static void SendSpringClampClear(int _fromClient)
            {
                using (Packet packet = new Packet((int)PacketTypes.springClampClear))
                {
                    SendTCPDataToAll(_fromClient, packet);
                }
            }
            
            public static void SendToolsMove(int fromClient, ModIOSpecialType tool, ModCarPlace place, bool playSound)
            {
                using (Packet _packet = new Packet((int)PacketTypes.toolMove))
                {
                    _packet.Write(tool);
                    _packet.Write(place);
                    _packet.Write(playSound);
                        
                    SendTCPDataToAll(fromClient, _packet);
                }
            }
            
        #endregion

        #region CarData
        
            public static void CarInfo(int fromClient, ModCar car, bool removed, bool resync=false)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carInfo))
                {
                    _packet.Write(removed);
                    _packet.Write(car);

                    if (!resync)
                    {
                        MelonLogger.Msg($"Send new car info: {car.carID}");
                        SendTCPDataToAll(fromClient, _packet);
                    }
                    else
                    {
                        MelonLogger.Msg($"Send resync car info: {car.carID}");
                        SendTCPData(fromClient, _packet);
                    }
                }
            }
            public static void PartScript(int fromClient, int carLoaderID, ModPartScript carPart, bool resync=false)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carPart))
                {
                    _packet.Write(carPart);
                    _packet.Write(carLoaderID);

                    if (!resync)
                        SendTCPDataToAll(fromClient, _packet);
                    else
                        SendTCPData(fromClient, _packet);
                }
            }
            public static void BodyPart(int fromClient, int carLoaderID, ModCarPart carPart)
            {
                using (Packet _packet = new Packet((int)PacketTypes.bodyPart))
                {
                    _packet.Write(carPart);
                    _packet.Write(carLoaderID);
                    

                    SendTCPDataToAll(fromClient, _packet);
                }
            }
            public static void PartScripts(int fromClient, List<ModPartScript> carParts, int carLoaderID, ModPartType modPartType, bool resync = false)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carParts))
                {
                    _packet.Write(carParts);
                    _packet.Write(carLoaderID);
                    _packet.Write(modPartType);
                    
                    if(!resync)
                        SendTCPDataToAll(fromClient, _packet);
                    else
                        SendTCPData(fromClient, _packet);
                }
            }
            
            public static void BodyParts(int fromClient, List<ModCarPart> carParts, int carLoaderID, bool resync=false)
            {
                using (Packet _packet = new Packet((int)PacketTypes.bodyParts))
                {
                    _packet.Write(carParts);
                    _packet.Write(carLoaderID);
                    
                    if(!resync)
                        SendTCPDataToAll(fromClient, _packet);
                    else
                        SendTCPData(fromClient, _packet);
                }
                MelonLogger.Msg($"SV: Sent bodyParts : {carParts.Count} ");
            }
            public static void CarPosition(int fromClient, int carLoaderID, int carPosition)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carPosition))
                {
                    _packet.Write(carLoaderID);
                    _packet.Write(carPosition);

                    SendTCPDataToAll(fromClient, _packet);
                }
            }
            
            public static void CarResync(int fromClient, List<(int,string)> carOnServer)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carResync))
                {
                    _packet.Write(carOnServer);

                    SendTCPData(fromClient, _packet);
                }
            }
            
        #endregion


        #region CampaignSync

            public static void GarageUpgrade(int fromClient, bool interactive, string updgradeID, bool on)
            {
                using (Packet _packet = new Packet((int)PacketTypes.garageUpgrade))
                {
                    _packet.Write(interactive);
                    _packet.Write(updgradeID);
                    _packet.Write(on);

                    SendTCPDataToAll(fromClient, _packet);
                }
            }
        

        #endregion

    }
}