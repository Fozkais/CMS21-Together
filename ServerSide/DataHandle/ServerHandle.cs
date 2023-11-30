using System.Collections.Generic;
using System.Linq;
using CMS21MP.ClientSide.Data;
using CMS21MP.CustomData;
using CMS21MP.ServerSide.Data;
using CMS21MP.SharedData;
using Il2Cpp;
using MelonLoader;

namespace CMS21MP.ServerSide.DataHandle
{
    public static class ServerHandle
    {

        #region Lobby and Connection
        
            public static void Empty(int _fromclient, Packet _packet)
            {
            }
            public static void WelcomeReceived(int _fromClient, Packet _packet)
            {
                int _clientIdCheck = _packet.ReadInt();
                string _username = _packet.ReadString();

                if (!ClientData.asGameStarted)
                {
                    MelonLogger.Msg($" SV: {Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected succesfully and is now {_username}.");

                    if (_fromClient != _clientIdCheck)
                    {
                        MelonLogger.Msg($" SV: Player \"{_username}\" (ID:{_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
                    }
                    Server.clients[_fromClient].SendToLobby(_username);
                    MelonCoroutines.Start( Server.clients[_fromClient].isClientAlive());
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
               // MelonLogger.Msg($"Client:[{_fromclient}] is Alive!");
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
        
        #region Player Information
        
            public static void playerPosition(int _fromClient, Packet _packet)
            {
                Vector3Serializable _position = _packet.Read<Vector3Serializable>();
                ServerData.players[_fromClient].position = _position;
                
                ServerSend.SendPosition(_fromClient, _position);
               // MelonLogger.Msg("Received position from:" + Server.clients[_fromClient].player.username);
            }
            
            public static void playerRotation(int _fromClient, Packet _packet)
            {
                QuaternionSerializable _rotation = _packet.Read<QuaternionSerializable>();
                ServerData.players[_fromClient].rotation = _rotation;
                
                ServerSend.SendRotation(_fromClient, _rotation);
            }
            
            public static void playerSceneChange(int _fromClient, Packet _packet)
            {
                string scene = _packet.ReadString();

                ServerData.players[_fromClient].scene = scene;
                ServerSend.SendPlayerSceneChange(_fromClient, scene);
            }
        
        #endregion

        #region Car

        public static void CarInfo(int _fromClient, Packet _packet)
        {
            ModCar car = _packet.Read<ModCar>();
            bool resync = _packet.ReadBool();

            if (!resync)
            {
                if (ServerData.carOnScene.Any(s => s.Value.carLoaderID == car.carLoaderID && s.Value.carID == car.carID))
                {
                    ServerData.carOnScene.Remove(ServerData.carOnScene.First(s => s.Value.carLoaderID == car.carLoaderID 
                                                                                  && s.Value.carID == car.carID).Key);
                }
                else
                {
                    ServerData.carOnScene.Add(car.carLoaderID, car);
                }
                ServerSend.CarInfo(_fromClient, car);
            }
            else
            {
                foreach (ModCar _car in ServerData.carOnScene.Values)
                {
                    ServerSend.CarInfo(_fromClient, _car, true);
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
                    List<ModPartScript> enginePart = new List<ModPartScript>();
                    for (int i = 0; i < _car.partInfo.EngineParts.Count; i++)
                    {
                        enginePart.Add(_car.partInfo.EngineParts[i]);
                    }
                    
                    List<ModCarPart> carParts = new List<ModCarPart>();
                    for (int i = 0; i < _car.partInfo.BodyParts.Count; i++)
                    {
                        carParts.Add(_car.partInfo.BodyParts[i]);
                    }
                    
                    ServerSend.PartScripts(_fromClient, otherParts, _car.carLoaderID, true);
                    ServerSend.PartScripts(_fromClient, enginePart, _car.carLoaderID, true);
                    ServerSend.PartScripts(_fromClient, suspensionPart, _car.carLoaderID, true);
                    ServerSend.BodyParts(_fromClient, carParts, _car.carLoaderID, true);
                    
                }
                
            }

        }
        
        public static void CarPosition(int _fromClient, Packet _packet)
        {
            int carLoaderID = _packet.ReadInt();
            int carPosition = _packet.ReadInt();
            
            ServerData.carOnScene.First(s => s.Value.carLoaderID == carLoaderID).Value.carPosition = carPosition;
            
            ServerSend.CarPosition(_fromClient, carLoaderID, carPosition);
        }

        public static void CarPart(int _fromClient, Packet _packet)
        {
            int carLoaderID = _packet.ReadInt();
            ModPartScript carPart = _packet.Read<ModPartScript>();
            GameScene scene = _packet.Read<GameScene>();
            
            if (ServerData.carOnScene.Any(s => s.Value.carLoaderID == carLoaderID))
            {
                // MelonLogger.Msg("Added PartScript to Buffer");
                var car = ServerData.carOnScene.First(s => s.Value.carLoaderID == carLoaderID).Value;

                CarPartHandle.HandlePart(carPart, car);
                
            }
            
            ServerSend.CarPart(_fromClient, carLoaderID, carPart, scene);
        }
        public static void BodyPart(int _fromClient, Packet _packet)
        {
            int carLoaderID = _packet.ReadInt();
            ModCarPart carPart = _packet.Read<ModCarPart>();
            GameScene scene = _packet.Read<GameScene>();
            
            if (ServerData.carOnScene.Any(s => s.Value.carLoaderID == carLoaderID))
            {
                // MelonLogger.Msg("Added PartScript to Buffer");
                var car = ServerData.carOnScene.First(s => s.Value.carLoaderID == carLoaderID).Value;

                if (car.partInfo != null)
                {
                    var partInfo = car.partInfo.BodyParts;
                    if (partInfo == null)
                        partInfo = new Dictionary<int, ModCarPart>();
                    
                    partInfo[carPart.carPartID] = carPart;
                }
            }
            else
            {
                MelonLogger.Msg("SV: Car Wont Exist!");
            }
            
            ServerSend.BodyPart(_fromClient, carLoaderID, carPart, scene);
        }
        
        public static void PartScripts(int _fromClient, Packet _packet)
        {
            List<ModPartScript> carParts = _packet.Read< List<ModPartScript>>();
            int carLoaderID = _packet.ReadInt();
            
            if (ServerData.carOnScene.Any(s => s.Value.carLoaderID == carLoaderID))
            {
                ModCar car = ServerData.carOnScene.First(s => s.Value.carLoaderID == carLoaderID).Value;
                foreach (ModPartScript part in carParts)
                {
                    CarPartHandle.HandlePart(part, car);
                }
            }
            ServerSend.PartScripts(_fromClient, carParts, carLoaderID);
        }
        public static void BodyParts(int _fromClient, Packet _packet)
        {
            List<ModCarPart> carParts = _packet.Read<List<ModCarPart>>();
            int carLoaderID = _packet.ReadInt();
            
            if (ServerData.carOnScene.Any(s => s.Value.carLoaderID == carLoaderID))
            {
                ModCar car = ServerData.carOnScene.First(s => s.Value.carLoaderID == carLoaderID).Value;
                foreach (ModCarPart carPart in carParts)
                {
                    if (car.partInfo != null)
                    {
                        var partInfo = car.partInfo.BodyParts;
                        if (partInfo == null)
                            partInfo = new Dictionary<int, ModCarPart>();
                    
                        partInfo[carPart.carPartID] =  carPart;
                    }
                }
            }
            
            ServerSend.BodyParts(_fromClient, carParts, carLoaderID);
        }
        

        #endregion

        #region Inventory

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
                            MelonLogger.Msg("SV: Added to GroupInventory!");
                            ServerData.groupItemInventory.Add(_item);
                        }
                    }
                    else
                    {
                        if (ServerData.groupItemInventory.Any(s => s.UID == _item.UID))
                        {
                            MelonLogger.Msg("SV: Removed from GroupInventory!");
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

        #region Garage Interaction

            public static void LifterPos(int _fromClient, Packet _packet)
            {
                int action = _packet.ReadInt();
                int pos = _packet.ReadInt();
                int _Loaderid = _packet.ReadInt();

                MelonLogger.Msg("Received Lifter: " + _Loaderid + " action : " + action + " pos: " +  pos);
                ServerSend.SendLifterPos(_fromClient, action, pos, _Loaderid);
            }
            
            public static void TireChanger(int _fromClient, Packet _packet)
            {
                ModGroupItem item = _packet.Read<ModGroupItem>();
                bool instant = _packet.ReadBool();
                bool connect = _packet.ReadBool();

                ServerSend.SendTireChange(_fromClient, item, instant, connect);
            }
            
            public static void TireChanger_ResetAction(int _fromClient, Packet _packet)
            {
                ServerSend.SendTireChanger_ResetAction(_fromClient);
            }
            public static void WheelBalancer(int _fromClient, Packet _packet)
            {
                ModGroupItem item = _packet.Read<ModGroupItem>();

                ServerSend.SendWheelBalancer(_fromClient, item);
            }
            
            public static void WheelBalancer_UpdateWheel(int _fromClient, Packet _packet)
            {
                ModGroupItem item = _packet.Read<ModGroupItem>();

                ServerSend.SendWheelBalancer(_fromClient, item);
            }
        
            public static void  WheelBalancer_ResetAction(int _fromClient, Packet _packet)
            {
                ServerSend.SendWheelBalancer_ResetAction(_fromClient);
            }
        

        #endregion
        
    }
}