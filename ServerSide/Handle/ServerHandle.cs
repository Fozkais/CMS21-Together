using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CMS21Together.ClientSide.Data;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ServerSide.Handle
{
    public class ServerHandle
    {
        #region Lobby and Connection
        
            public static void Empty(int _fromclient, Packet _packet)
            {
            }
            
            public static void WelcomeReceived(int _fromClient, Packet _packet)
            {
                int _clientIdCheck = _packet.ReadInt();
                string _username = _packet.ReadString();
                ReadOnlyDictionary<string, bool> content = _packet.Read<ReadOnlyDictionary<string, bool>>();
                string modVersion = _packet.ReadString();
                string gameVersion = _packet.ReadString();

                if (gameVersion != ContentManager.Instance.gameVersion)
                {
                    ServerSend.DisconnectClient(_fromClient, $"Game is not on same version as Server ! ({ContentManager.Instance.gameVersion})");
                    return;
                }

                if (modVersion != MainMod.ASSEMBLY_MOD_VERSION)
                {
                    ServerSend.DisconnectClient(_fromClient, $"Mod is not on same version as Server ! ({MainMod.ASSEMBLY_MOD_VERSION}))");
                    return;
                }

                var a = ApiCalls.API_M1(content, ContentManager.Instance.OwnedContents);
                ServerSend.ContentInfo(a);
                
                if (!ClientData.Instance.GameReady)
                {
                    MelonLogger.Msg($" SV: {Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected succesfully and is now {_username}.");

                    if (_fromClient != _clientIdCheck)
                    {
                        MelonLogger.Msg($" SV: Player \"{_username}\" (ID:{_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
                    }
                    Server.clients[_fromClient].SendToLobby(_username);
                }
                else
                {
                    ServerSend.DisconnectClient(_fromClient, "Player couldn't connect a already started game!");
                }

            }
            
            public static void keepAlive(int _fromclient, Packet _packet)
            {
                ServerSend.keepAlive(_fromclient);
                Server.clients[_fromclient].Alive = true;
                ServerSend.SendKeepAliveConfirmation(_fromclient);

                // Mettre à jour la dernière activité du client
                ServerData.lastClientActivity[_fromclient] = DateTime.Now;
            }
            
            public static void Disconnect(int _fromClient, Packet _packet)
            {
                int id = _packet.ReadInt();
                
                Server.clients[_fromClient].Disconnect(id);
                MelonLogger.Msg($" SV: {Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} " + $"has disconnected.");
            }
            
            public static void ReadyState(int _fromClient, Packet _packet)
            {
                bool _ready = _packet.ReadBool();
                int _id = _packet.ReadInt();

                ServerData.players[_id].isReady = _ready;

                ServerSend.SendReadyState(_fromClient,_ready, _id);
            }
            
        #endregion

        #region PlayerData
        
            public static void playerInitialPosition(int _fromClient, Packet _packet)
            {
                Vector3Serializable _position = _packet.Read<Vector3Serializable>();
                ServerData.players[_fromClient].position = _position;
                    
                ServerSend.SendInitialPosition(_fromClient, _position);
            }

            public static void playerPosition(int _fromClient, Packet _packet)
            {
                Vector3Serializable _position = _packet.Read<Vector3Serializable>();
                ServerData.players[_fromClient].position = _position;
                
                ServerSend.SendPosition(_fromClient, _position);
            }
                
            public static void playerRotation(int _fromClient, Packet _packet)
            {
                QuaternionSerializable _rotation = _packet.Read<QuaternionSerializable>();
                ServerData.players[_fromClient].rotation = _rotation;
                    
                ServerSend.SendRotation(_fromClient, _rotation);
            }
                
            public static void playerSceneChange(int _fromClient, Packet _packet)
            {
                GameScene scene = _packet.Read<GameScene>();
                MelonLogger.Msg($"SV:  { ServerData.players[_fromClient].username} changed scene! : " + scene.ToString());

                ServerData.players[_fromClient].scene = scene;
                ServerSend.SendPlayerSceneChange(_fromClient, scene);
            }

            public static void playerStats(int _fromClient, Packet _packet)
            {
                int value = _packet.ReadInt();
                ModStats type = _packet.Read<ModStats>();

                ServerSend.SendStats(_fromClient, value, type);

            }

        
            public static void InventoryItem(int _fromClient, Packet _packet)
            {
                bool resync = _packet.ReadBool();
                

                if (!resync)
                {
                    ModItem item = _packet.Read<ModItem>();
                    bool status = _packet.ReadBool();
                    
                    if (status)
                    {
                        if (!ServerData.itemInventory.Any(s => s.UID == item.UID))
                        {
                            ServerData.itemInventory.Add(item);
                        }
                    }
                    else
                    {
                        if (ServerData.itemInventory.Any(s => s.UID == item.UID))
                        {
                            int index = ServerData.itemInventory.FindIndex(s => s.UID == item.UID);
                            ServerData.itemInventory.Remove(ServerData.itemInventory[index]);
                        }
                    }
                    ServerSend.SendInventoryItem(_fromClient, item, status);
                    return;
                }

                foreach (ModItem _modItem in ServerData.itemInventory)
                {
                    ServerSend.SendInventoryItem(_fromClient, _modItem, true, true);
                }
            }
            
            public static void InventoryGroupItem(int _fromClient, Packet _packet)
            {
                bool resync = _packet.ReadBool();

                if (!resync)
                {
                    ModGroupItem _item = _packet.Read<ModGroupItem>();
                    bool status = _packet.ReadBool();
                    if (status)
                    {
                        if (!ServerData.groupItemInventory.Any(s => s.UID == _item.UID))
                        {
                            //MelonLogger.Msg("SV: Added to GroupInventory!");
                            ServerData.groupItemInventory.Add(_item);
                        }
                    }
                    else
                    {
                        if (ServerData.groupItemInventory.Any(s => s.UID == _item.UID))
                        {
                           // MelonLogger.Msg("SV: Removed from GroupInventory!");
                            int index = ServerData.groupItemInventory.FindIndex(s => s.UID == _item.UID);
                            ServerData.groupItemInventory.Remove(ServerData.groupItemInventory[index]);
                        }
                    }
                    ServerSend.SendInventoryGroupItem(_fromClient, _item, status);
                    return;
                }
                
                MelonLogger.Msg("SV: SendingResync!");
                
                foreach (ModGroupItem _modGroupItem in ServerData.groupItemInventory)
                {
                    ServerSend.SendInventoryGroupItem(_fromClient, _modGroupItem, true, true);
                }
            }

        #endregion

        #region GarageInteraction

            public static void Lifter(int _fromClient, Packet _packet)
            {
                ModLifterState state = _packet.Read<ModLifterState>();
                int carLoaderID = _packet.ReadInt();

                ServerSend.SendLifter(_fromClient, state, carLoaderID);
            }
            
            public static void TireChanger(int _fromClient, Packet _packet)
            {
                ModGroupItem item = _packet.Read<ModGroupItem>();
                bool instant = _packet.ReadBool();
                bool connect = _packet.ReadBool();
                bool resetAction = _packet.ReadBool();

                ServerSend.SendTireChange(_fromClient, item, instant, connect, resetAction);
            }
            
            public static void WheelBalancer(int _fromClient, Packet _packet)
            {
                ModWheelBalancerActionType aType = _packet.Read<ModWheelBalancerActionType>();
                ModGroupItem item;
                
                if (aType == ModWheelBalancerActionType.start || aType == ModWheelBalancerActionType.setGroup)
                {
                    item = _packet.Read<ModGroupItem>();
                    ServerSend.WheelBalancer(_fromClient, aType, item);
                    return;
                }
                ServerSend.WheelBalancer(_fromClient, aType);
            }
            
            public static void EngineStandAngle(int _fromClient, Packet _packet)
            {
                int newAngle = _packet.ReadInt();

                ServerData.engineStand.angle = newAngle;
                ServerSend.SendEngineAngle(_fromClient, newAngle);
            }
            
            public static void SetEngineOnStand(int _fromClient, Packet _packet)
            {
                ModItem engineItem = _packet.Read<ModItem>();

                MelonLogger.Msg("Received engine");
                
                ServerData.engineStand.engine = engineItem;
                ServerSend.SendEngineOnStand(_fromClient, engineItem);
            }
            
            public static void SetGroupEngineOnStand(int _fromClient, Packet _packet)
            {
                ModGroupItem engineItem = _packet.Read<ModGroupItem>();
                Vector3Serializable pos = _packet.Read<Vector3Serializable>();
                QuaternionSerializable rot = _packet.Read<QuaternionSerializable>();

                MelonLogger.Msg($"Received engineGroup: {pos.toVector3()} , {rot.toQuaternion()}");
                
                ServerData.engineStand.Groupengine = engineItem;
                ServerData.engineStand.position = pos;
                ServerData.engineStand.rotation = rot;
                ServerSend.SendEngineGroup(_fromClient, engineItem, pos, rot);
            }
            
            public static void ResyncEngineStand(int _fromClient, Packet _packet)
            {
                MelonLogger.Msg("Received EngineStandResync");

                if (ServerData.engineStand.Groupengine != null)
                {
                    ServerSend.SendEngineGroup(_fromClient, ServerData.engineStand.Groupengine, ServerData.engineStand.position,ServerData.engineStand.rotation,true);
                    foreach (ModPartScript part in ServerData.engineStand.engineStandParts.Values)
                    {
                        ServerSend.PartScript(_fromClient, -1, part, true);
                    }
                }
            }
            
            
            
            public static void TakeOffEngineFromStand(int _fromClient, Packet _packet)
            {
                ServerData.engineStand.engine = null;
                ServerData.engineStand.engineStandParts.Clear();
                
                ServerSend.TakeOffEngineFromStand(_fromClient);
            }
            
            public static void EngineCrane(int _fromClient, Packet _packet)
            {
                var action = _packet.ReadBool();
                if (action == false)
                {
                    ModGroupItem items = _packet.Read<ModGroupItem>();
                    ServerSend.EngineCrane(_fromClient, -1,items);
                }
                else
                {
                    int carLoaderID = _packet.ReadInt();
                    ServerSend.EngineCrane(_fromClient, carLoaderID);
                }
                
            }
            
            public static void SpringClampGroup(int _fromClient, Packet _packet)
            {
                ModGroupItem item  = _packet.Read<ModGroupItem>();
                bool instant = _packet.ReadBool();
                bool mount = _packet.ReadBool();

                ServerSend.SendSpringClampGroup(_fromClient, item, instant, mount);
            }
            
            public static void SpringClampClear(int _fromClient, Packet _packet)
            {
                ServerSend.SendSpringClampClear(_fromClient);
            }
            
            public static void OilBin(int _fromclient, Packet _packet)
            {
                int loaderID = _packet.ReadInt();

                ServerSend.SendOilBin(_fromclient, loaderID);
            }
            
            public static void ToolsMove(int _fromClient, Packet _packet)
            {
                ModIOSpecialType tool = _packet.Read<ModIOSpecialType>();
                ModCarPlace place = _packet.Read<ModCarPlace>();
                bool playSound = _packet.ReadBool();

                ServerData.toolsPosition[tool] = place;
                ServerSend.SendToolsMove(_fromClient, tool, place, playSound);
            }
            

        #endregion

        #region CarData

            public static void CarResync(int _fromClient, Packet _packet)
            {
                var phase = _packet.ReadBool();
                if (phase)
                {
                    MelonCoroutines.Start(ResendAllCar(_fromClient));
                }
                else
                {
                    var carToResync  = _packet.Read<List<(int, string)>>();
                    ServerData.players[_fromClient].carToResync = carToResync;
                }
            }

            private static IEnumerator ResendAllCar(int clientID)
            {
                yield return new WaitForSeconds(1.5f);
                MelonLogger.Msg("SV : Resending Car to client!");
                var player = ServerData.players[clientID];
                yield return new WaitForSeconds(1.5f);
                
                foreach (ModCar _car in ServerData.LoadedCars.Values)
                {
                    if (player.carToResync.Contains((_car.carLoaderID, _car.carID)))
                    {
                        ServerSend.CarInfo(clientID, new ModCar(_car), false, true);
                        
                        List<ModPartScript> otherParts = new List<ModPartScript>();
                        for (int i = 0; i < _car.partInfo.OtherParts.Count; i++)
                        {
                            for (int j = 0; j < _car.partInfo.OtherParts[i].Count; j++)
                            {
                                otherParts.Add(_car.partInfo.OtherParts[i][j]);
                            }
                        }
                        List<ModPartScript> suspensionPart = new List<ModPartScript>();
                        for (int i = 0; i < _car.partInfo.SuspensionParts.Count; i++)
                        {
                            for (int j = 0; j < _car.partInfo.SuspensionParts[i].Count; j++)
                            {
                                suspensionPart.Add(_car.partInfo.SuspensionParts[i][j]);
                            }
                        }
                        
                        ServerSend.PartScripts(clientID, otherParts, _car.carLoaderID, ModPartType.other,true);
                        ServerSend.PartScripts(clientID,  _car.partInfo.EngineParts.Values.ToList(), _car.carLoaderID, ModPartType.engine,true);
                        ServerSend.PartScripts(clientID,  _car.partInfo.DriveshaftParts.Values.ToList(), _car.carLoaderID, ModPartType.driveshaft,true);
                        ServerSend.PartScripts(clientID, suspensionPart, _car.carLoaderID, ModPartType.suspension,true);
                        ServerSend.BodyParts(clientID,  _car.partInfo.BodyParts.Values.ToList(), _car.carLoaderID, true);
                    }
                    player.carToResync.Remove((_car.carLoaderID, _car.carID));
                }
            }

            public static void CarInfo(int _fromClient, Packet _packet)
            {
                bool removed = _packet.ReadBool();
                ModCar car = _packet.Read<ModCar>();
                
                MelonLogger.Msg($"Received new car info: {car.carID}");
                
                if (!removed)
                {
                    if(!ServerData.LoadedCars.ContainsKey(car.carLoaderID))
                        ServerData.LoadedCars.Add(car.carLoaderID, car);
                }
                else
                {
                    if (ServerData.LoadedCars.Any(s =>
                            s.Value.carLoaderID == car.carLoaderID && s.Value.carID == car.carID))
                    {
                        ServerData.LoadedCars.Remove(ServerData.LoadedCars.First(s => s.Value.carLoaderID == car.carLoaderID 
                            && s.Value.carID == car.carID).Key);
                    }
                }
                
                ServerSend.CarInfo(_fromClient, car, removed);
            }
            public static void CarPart(int _fromClient, Packet _packet)
            {
                ModPartScript carPart = _packet.Read<ModPartScript>();
                int carLoaderID = _packet.ReadInt();
                
                if (carLoaderID == -1)
                {
                    ServerData.engineStand.engineStandParts[carPart.partID] = carPart;
                    ServerSend.PartScript(_fromClient, -1, carPart); 
                    return;
                }
            
                if (ServerData.LoadedCars.Any(s => s.Value.carLoaderID == carLoaderID))
                {
                    var car = ServerData.LoadedCars.First(s => s.Value.carLoaderID == carLoaderID).Value;
                    CarPartHandle.HandlePart(carPart, car);
                
                }
            
                ServerSend.PartScript(_fromClient, carLoaderID, carPart);
            }

            public static void CarParts(int _fromClient, Packet _packet)
            {
                List<ModPartScript> carParts = _packet.Read<List<ModPartScript>>();
                int carLoaderID = _packet.ReadInt();
                ModPartType modPartType = _packet.Read<ModPartType>();

                MelonLogger.Msg($"SV: Received Parts : {modPartType} , {carParts.Count} ");
            
                if (ServerData.LoadedCars.Any(s => s.Value.carLoaderID == carLoaderID))
                {
                    ModCar car = ServerData.LoadedCars.First(s => s.Value.carLoaderID == carLoaderID).Value;
                    foreach (ModPartScript part in carParts)
                    {
                        CarPartHandle.HandlePart(part, car);
                    }
                }
                ServerSend.PartScripts(_fromClient, carParts, carLoaderID, modPartType);
            }

            public static void BodyPart(int _fromClient, Packet _packet)
            {
                ModCarPart carPart = _packet.Read<ModCarPart>();
                int carLoaderID = _packet.ReadInt();
                
                if (ServerData.LoadedCars.Any(s => s.Value.carLoaderID == carLoaderID))
                {
                    var car = ServerData.LoadedCars.First(s => s.Value.carLoaderID == carLoaderID).Value;
                    if (car.partInfo != null)
                    {
                        if (car.partInfo.BodyParts == null)
                            car.partInfo.BodyParts = new Dictionary<int, ModCarPart>();
                    
                        car.partInfo.BodyParts[carPart.carPartID] = carPart;
                    }
                }
                ServerSend.BodyPart(_fromClient, carLoaderID, carPart);
            }

            public static void BodyParts(int _fromClient, Packet _packet)
            {
                List<ModCarPart> carParts = _packet.Read<List<ModCarPart>>();
                int carLoaderID = _packet.ReadInt();
                
                MelonLogger.Msg($"SV: Received BodyParts :  {carParts.Count} ");
            
                if (ServerData.LoadedCars.Any(s => s.Value.carLoaderID == carLoaderID))
                {
                    ModCar car = ServerData.LoadedCars.First(s => s.Value.carLoaderID == carLoaderID).Value;
                    if (car.partInfo != null)
                    {
                        if ( car.partInfo.BodyParts == null)
                            car.partInfo.BodyParts = new Dictionary<int, ModCarPart>();
                        
                        foreach (ModCarPart carPart in carParts)
                        {
                            car.partInfo.BodyParts[carPart.carPartID] =  carPart;
                        }
                    }
                }
            
                ServerSend.BodyParts(_fromClient, carParts, carLoaderID);
            }
            
            public static void CarPosition(int _fromClient, Packet _packet)
            {
                int carLoaderID = _packet.ReadInt();
                int carPosition = _packet.ReadInt();
            
                ServerData.LoadedCars.First(s => s.Value.carLoaderID == carLoaderID).Value.carPosition = carPosition;
            
                ServerSend.CarPosition(_fromClient, carLoaderID, carPosition);
            }

        #endregion


    }
}