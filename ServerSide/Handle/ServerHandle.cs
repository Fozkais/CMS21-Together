using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CMS21Together.ClientSide.Data;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;

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
                Dictionary<string, bool> content = _packet.Read<Dictionary<string, bool>>();
                string modVersion = _packet.ReadString();

                if (modVersion != MainMod.ASSEMBLY_MOD_VERSION)
                {
                    ServerSend.DisconnectClient(_fromClient, $"Please match Server Version to connect ({MainMod.ASSEMBLY_MOD_VERSION})");
                    return;
                }

                foreach (var hostContent in ContentManager.Instance.Contents)
                {
                    if (hostContent.Value != content[hostContent.Key])
                    {
                        ServerSend.DisconnectClient(_fromClient, "DLC content mismatch!");
                        return;
                    }
                }
                
                if (!ClientData.asGameStarted)
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
                MelonLogger.Msg("Received new scene! : " + scene.ToString());

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
                ModItem item = _packet.Read<ModItem>();
                bool status = _packet.ReadBool();
                bool resync = _packet.ReadBool();
                

                if (!resync)
                {
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
                            ServerData.itemInventory.Remove(item);
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
                ModGroupItem _item = _packet.Read<ModGroupItem>();
                bool status = _packet.ReadBool();
                bool resync = _packet.ReadBool();

                if (!resync)
                {
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
                
                foreach (ModGroupItem _modGroupItem in ServerData.groupItemInventory)
                {
                    MelonLogger.Msg("SV: SendingResync!");
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

                ServerSend.SendEngineAngle(_fromClient, newAngle);
            }
            

        #endregion

        #region CarData

            public static void CarInfo(int _fromClient, Packet _packet)
            {
                ModCar car = _packet.Read<ModCar>();

                if (ServerData.LoadedCars.Any(s =>
                        s.Value.carLoaderID == car.carLoaderID && s.Value.carID == car.carID))
                {
                    ServerData.LoadedCars.Remove(ServerData.LoadedCars.First(s => s.Value.carLoaderID == car.carLoaderID 
                        && s.Value.carID == car.carID).Key);
                }
                else
                {
                    ServerData.LoadedCars.Add(car.carLoaderID, car);
                }
                ServerSend.CarInfo(_fromClient, car);
            }
            public static void CarPart(int _fromClient, Packet _packet)
            {
                ModPartScript carPart = _packet.Read<ModPartScript>();
                int carLoaderID = _packet.ReadInt();
                
            //    MelonLogger.Msg("SV:ReceivedPart!");
            
                if (ServerData.LoadedCars.Any(s => s.Value.carLoaderID == carLoaderID))
                {
                    var car = ServerData.LoadedCars.First(s => s.Value.carLoaderID == carLoaderID).Value;
                    CarPartHandle.HandlePart(carPart, car);
                
                }
            
                ServerSend.CarPart(_fromClient, carLoaderID, carPart);
            }

            public static void CarParts(int _fromClient, Packet _packet)
            {
                List<ModPartScript> carParts = _packet.Read< List<ModPartScript>>();
                int carLoaderID = _packet.ReadInt();
            
                if (ServerData.LoadedCars.Any(s => s.Value.carLoaderID == carLoaderID))
                {
                    ModCar car = ServerData.LoadedCars.First(s => s.Value.carLoaderID == carLoaderID).Value;
                    foreach (ModPartScript part in carParts)
                    {
                        CarPartHandle.HandlePart(part, car);
                    }
                }
                ServerSend.PartScripts(_fromClient, carParts, carLoaderID);
            }

            public static void BodyPart(int _fromClient, Packet _packet)
            {
                ModCarPart carPart = _packet.Read<ModCarPart>();
                int carLoaderID = _packet.ReadInt();
            
              //  MelonLogger.Msg("SV:ReceivedBodyPart!");
                
                if (ServerData.LoadedCars.Any(s => s.Value.carLoaderID == carLoaderID))
                {
                    var car = ServerData.LoadedCars.First(s => s.Value.carLoaderID == carLoaderID).Value;

                    if (car.partInfo != null)
                    {
                        var partInfo = car.partInfo.BodyParts;
                        if (partInfo == null)
                            partInfo = new Dictionary<int, ModCarPart>();
                    
                        partInfo[carPart.carPartID] = carPart;
                    }
                }
            
                ServerSend.BodyPart(_fromClient, carLoaderID, carPart);
            }

            public static void BodyParts(int _fromClient, Packet _packet)
            {
                List<ModCarPart> carParts = _packet.Read<List<ModCarPart>>();
                int carLoaderID = _packet.ReadInt();
            
                if (ServerData.LoadedCars.Any(s => s.Value.carLoaderID == carLoaderID))
                {
                    ModCar car = ServerData.LoadedCars.First(s => s.Value.carLoaderID == carLoaderID).Value;
                    if (car.partInfo != null)
                    {
                        var partInfo = car.partInfo.BodyParts;
                        if (partInfo == null)
                            partInfo = new Dictionary<int, ModCarPart>();
                    }

                    foreach (ModCarPart carPart in carParts)
                    {
                        if (car.partInfo != null)
                            car.partInfo.BodyParts[carPart.carPartID] =  carPart;
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