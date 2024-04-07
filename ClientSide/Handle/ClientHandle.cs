using System.Collections.Generic;
using System.Linq;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Car;
using CMS21Together.ClientSide.Data.GarageInteraction;
using CMS21Together.ClientSide.Data.PlayerData;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Handle
{
    public class ClientHandle : MonoBehaviour
    {
        #region Lobby and connection
        
            public static void Welcome(Packet _packet)
            {
                string _msg = _packet.ReadString();
                int _myId = _packet.ReadInt();

                MelonLogger.Msg($"Message from Server:{_msg}");
                Client.Instance.Id = _myId;

                ClientSend.WelcomeReceived();
                _packet.Dispose();
            }
        
            public static void KeepAlive(Packet _packet)
            {
               // MelonLogger.Msg("Start Keeping Alive!");
                ClientData.isServerAlive = true;
                ClientData.needToKeepAlive = true;
            }
            
            public static void KeepAliveConfirmation(Packet _packet)
            {
                // Le serveur a confirmé la réception du paquet "Keep Alive"
                ClientData.isServerAlive = true;
            }
            

            public static void Disconnect(Packet _packet)
            {
                string _msg = _packet.ReadString();
                int id = _packet.ReadInt();
                
                
                if(id == Client.Instance.Id)
                    MelonLogger.Msg($"You were disconnected from the server :{_msg}");
                else
                    MelonLogger.Msg($"Message from server : {_msg}");
                
                if (id != Client.Instance.Id && id != 1)
                {
                    if(ClientData.players.ContainsKey(id))
                        ClientData.players[id].Disconnect();
                }
                else
                {
                    if (!ModSceneManager.isInMenu())
                        GameManager.Instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("Menu", SceneType.Menu, true, false));
                    else
                        Client.Instance.Disconnect();
                    
                    Application.runInBackground = false;
                }
                
                _packet.Dispose();
            }

            public static void ReadyState(Packet _packet)
            {
                bool _ready = _packet.ReadBool();
                int _id = _packet.ReadInt();

                ClientData.players[_id].isReady = _ready;
                _packet.Dispose();
            }
            
            public static void PlayerInfo(Packet _packet)
            {
                Player info = _packet.Read<Player>();
                ClientData.players[info.id] = info;
                
                MelonLogger.Msg($"Received {info.username}, {info.id} info from server.");
                _packet.Dispose();
            }
            public static void PlayersInfo(Packet _packet)
            {
                Dictionary<int, Player> info = _packet.Read<Dictionary<int, Player>>();
                if(info != null)
                    ClientData.players = info;
                else
                    MelonLogger.Msg("Received player info is null!");
                _packet.Dispose();
            }
            
            public static void StartGame(Packet _packet)
            {
                SavesManager.LoadSave(0, null, true);
                ModUI.Instance.showModUI = false;
                
                _packet.Dispose();
            }
            
            public static void SpawnPlayer(Packet _packet)
            {
                Player _player = _packet.Read<Player>();
                int _id = _packet.ReadInt();
                ClientData.players[_id] = _player;
                if (ClientData.players.TryGetValue(_id, out var player))
                {
                    if(!ClientData.PlayersGameObjects.ContainsKey(player.id))
                        ClientData.SpawnPlayer(_player);
                }
                
                ClientSend.SendInitialPosition(new Vector3Serializable(GameData.localPlayer.transform.position));
                
                _packet.Dispose();
            }
            
        #endregion

        #region PlayerData
        
            public static void playerInitialPos(Packet _packet)
            {
                int _id = _packet.ReadInt();
                Vector3Serializable _position = _packet.Read<Vector3Serializable>();
                Movement.SetInitialPosition(_id, _position);
                _packet.Dispose();
            }

            public static void playerPosition(Packet _packet)
            {
                int _id = _packet.ReadInt();
                Vector3Serializable _position = _packet.Read<Vector3Serializable>();
                Movement.UpdatePlayerPosition(_id, _position);
                _packet.Dispose();
            }
                
            public static void playerRotation(Packet _packet)
            {
                int _id = _packet.ReadInt();
                QuaternionSerializable _rotation = _packet.Read<QuaternionSerializable>();
                Rotation.UpdatePlayerRotation(_id, _rotation);
                _packet.Dispose();
            }
                
            public static void playerSceneChange(Packet _packet)
            {
                int id = _packet.ReadInt();
                GameScene scene = _packet.Read<GameScene>();
                    
                MelonLogger.Msg("Received Scene Update :" + scene);
                if (ClientData.players.TryGetValue(id, out var player))
                {
                    MelonLogger.Msg("pass1");
                    player.scene = scene;
                    if (player.scene != ModSceneManager.currentScene())
                    {
                       // MelonLogger.Msg($"Destroying {player.username} gameObject.");
                        Destroy(ClientData.PlayersGameObjects[id]);
                    }
                    else
                    {
                        if (ClientData.PlayersGameObjects.TryGetValue(id, out var instance))
                        {
                            if (instance == null)
                            {
                               // MelonLogger.Msg($"Instance Null for {player.username} with id : {player.id} or {id} , spawning...");
                                ClientData.SpawnPlayer(player);
                            }
                        }
                    }
                }
                _packet.Dispose();
            }
        
            public static void playerStats(Packet _packet)
            {
                int value = _packet.ReadInt();
                ModStats type = _packet.Read<ModStats>();

                switch (type)
                {
                    case ModStats.money:
                        ClientData.playerMoney = value;
                        GlobalData.PlayerMoney = value;
                        break;
                    case ModStats.scrap:
                        ClientData.playerScrap = value;
                        GlobalData.PlayerScraps = value;
                        break;
                    case ModStats.exp:
                        ClientData.playerExp = value;
                        GlobalData.PlayerExp = value;
                        break;
                }
                
                _packet.Dispose();
            }
            
            public static void InventoryItem(Packet _packet)
            {
                ModItem _item = _packet.Read<ModItem>();
                Item _itemGame = _item.ToGame(_item);
                bool status = _packet.ReadBool();

                if (status)
                {
                    if (!ModInventory.handledItem.Any(s => s.UID == _item.UID) )
                    {
                        ModInventory.handledItem.Add(_item);
                        GameData.Instance.localInventory.Add(_itemGame);
                    }
                }
                else
                {
                    if (ModInventory.handledItem.Any(s => s.UID == _item.UID))
                    {
                        ModInventory.handledItem.Remove(_item);
                        GameData.Instance.localInventory.Delete(_itemGame);
                    }
                }
                _packet.Dispose();
            }
            
            public static void InventoryGroupItem(Packet _packet)
            {
                ModGroupItem _item = _packet.Read<ModGroupItem>();
                bool status = _packet.ReadBool();
                
                if (status)
                {
                    if (!ModInventory.handledGroupItem.Any(s => s.UID == _item.UID))
                    {
                        ModInventory.handledGroupItem.Add(_item);
                        GameData.Instance.localInventory.AddGroup(_item.ToGame(_item));
                    }
                }
                else
                {
                    if (ModInventory.handledGroupItem.Any(s => s.UID == _item.UID))
                    {
                        int index = ModInventory.handledGroupItem.FindIndex(s => s.UID == _item.UID);
                        ModInventory.handledGroupItem.Remove(ModInventory.handledGroupItem[index]);
                        GameData.Instance.localInventory.DeleteGroup(_item.UID);
                    }
                }
                _packet.Dispose();
            }

        #endregion

        #region Garage Interaction

            public static void Lifter(Packet _packet)
            {
                ModLifterState state = _packet.Read<ModLifterState>();
                int carLoaderID = _packet.ReadInt();

                MelonLogger.Msg("Received lifter info!");
                MelonLogger.Msg("State :" + state + "ID: " + carLoaderID);
                
                if (ClientData.LoadedCars.Any(s => s.Value.carLoaderID == carLoaderID - 1))
                {
                    var lifter = GameData.Instance.carLoaders[carLoaderID-1].lifter;
                    MelonLogger.Msg("Passed Lifter.");
                    ModLifterLogic.listenToLifter = false;
                    if((int)state > (int)lifter.currentState)
                        lifter.Action((0));
                    else
                    {
                        lifter.Action((1));
                    }
                    ClientData.LoadedCars[carLoaderID-1].CarLifterState = (int)state;
                    ModLifterLogic.listenToLifter = true;
                }

                
                _packet.Dispose();
            }
            
            public static void TireChange(Packet _packet)
            {
                ModGroupItem _item = _packet.Read<ModGroupItem>();
                bool instant = _packet.ReadBool();
                bool connect = _packet.ReadBool();
                bool resetAction = _packet.ReadBool();

                if (!resetAction)
                {
                    
                    MelonLogger.Msg("CL: Received New Tire Change : " + 
                                    " action: " + connect + " instant: " + instant);
                    ModTireChangerLogic.listenToTC = false;
                    GameData.Instance.tireChanger.SetGroupOnTireChanger(_item.ToGame(_item), instant, connect);
                    ModTireChangerLogic.listenToTC = true;
                }
                else
                {
                    MelonLogger.Msg("Reset TireChange!");
                    GameData.Instance.tireChanger.ResetActions();
                }
                
                _packet.Dispose();
            }   
            
            public static void WheelBalancer(Packet _packet)
            {
                ModWheelBalancerActionType aType = _packet.Read<ModWheelBalancerActionType>();
                ModGroupItem _item = null;
                if(aType == ModWheelBalancerActionType.start || aType == ModWheelBalancerActionType.setGroup)
                     _item = _packet.Read<ModGroupItem>();

                if (aType == ModWheelBalancerActionType.remove)
                {
                    GameData.Instance.wheelBalancer.ResetActions();
                }
                else
                {
                        MelonCoroutines.Start(ModWheelBalancerLogic.ResetWBListen());
                        
                        MelonLogger.Msg("CL: Received WheelBalance!");
                        
                        // ReSharper disable once PossibleNullReferenceException
                        GameData.Instance.wheelBalancer.SetGroupOnWheelBalancer(_item.ToGame(_item), true);
                }
                
                _packet.Dispose();
            }   
            
            public static void EngineStandAngle(Packet _packet)
            {
                int angle = _packet.ReadInt();
                
                ModEngineStandLogic.listenToEngineStandLogic = false;
                GameData.Instance.engineStand.SetEngineStandAngle(angle);
                ModEngineStandLogic.listenToEngineStandLogic = true;
                
                _packet.Dispose();
            }   

        #endregion

        #region CarData

            public static void CarInfo(Packet _packet)
            {
                bool removed = _packet.ReadBool();
                ModCar car = _packet.Read<ModCar>();
                
                MelonLogger.Msg($"CL: Received new car from server!");

                CarLoader carLoader = GameData.Instance.carLoaders[car.carLoaderID];

                bool checkCondition = ClientData.LoadedCars.Any(s =>
                    s.Value.carLoaderID == car.carLoaderID && s.Value.carID == car.carID);
                
                if (!removed)
                {
                    if(!checkCondition)
                    ClientData.LoadedCars.Add(car.carLoaderID, car);
                    ModCar _car  = ClientData.LoadedCars.First(s 
                        => s.Value.carLoaderID == car.carLoaderID && s.Value.carID == car.carID).Value;
                    
                    MelonCoroutines.Start(CarUpdate.CarSpawnFade(_car, carLoader));
                    MelonLogger.Msg("CL: Loading new car...");
                }
                else
                {
                    if (checkCondition)
                    {
                        ClientData.LoadedCars.Remove(ClientData.LoadedCars.First(s => 
                                s.Value.carLoaderID == car.carLoaderID && s.Value.carID == car.carID).Key);
                        carLoader.DeleteCar();
                        MelonLogger.Msg("CL: Removing car...");
                    }
                }

                _packet.Dispose();
            }
            
            public static void CarPart(Packet _packet)
            {
                ModPartScript carPart = _packet.Read<ModPartScript>();
                int carLoaderID = _packet.ReadInt();
                
                
                MelonCoroutines.Start(CarUpdate.HandleNewPart(carPart, carLoaderID));
                
                _packet.Dispose();
            }
            
            public static void CarPosition(Packet _packet)
            {
                int carLoaderID = _packet.ReadInt();
                int carPosition = _packet.ReadInt();
            
                ClientData.LoadedCars.First(s => s.Value.carLoaderID == carLoaderID).Value.carPosition = carPosition;
                GameData.Instance.carLoaders[carLoaderID].ChangePosition(carPosition);
                _packet.Dispose();
            }
            
            public static void CarParts(Packet _packet)
            {
                List<ModPartScript> carParts = _packet.Read<List<ModPartScript>>();
                int carLoaderID = _packet.ReadInt();
                
                ModCar car = ClientData.LoadedCars.First(s => s.Value.carLoaderID == carLoaderID).Value;
                
                if (carParts == null || carParts.Count == 0)
                {
                    MelonLogger.Msg("Receives parts list is empty!!");
                    _packet.Dispose();
                    return;
                }

                foreach (ModPartScript part in carParts)
                {
                    MelonCoroutines.Start(CarUpdate.HandleNewPart(part, carLoaderID));
                }
                
                switch (carParts[0].type)
                {
                    case ModPartType.other:
                        car.receivedOtherParts = true;
                        break;
                    case ModPartType.engine:
                        car.receivedEngineParts = true;
                        break;
                    case ModPartType.suspension:
                        car.receivedSuspensionParts = true;
                        break;
                    case ModPartType.driveshaft:
                        car.receivedDriveshaftParts = true;
                        break;
                }

                _packet.Dispose();
            }
            public static void BodyPart(Packet _packet)
            {
                ModCarPart carPart = _packet.Read<ModCarPart>();
                int carLoaderID = _packet.ReadInt();
                
                
                MelonCoroutines.Start(CarUpdate.HandleNewBodyPart(carPart, carLoaderID));
                
                _packet.Dispose();
            }
            
            public static void BodyParts(Packet _packet)
            {
                List<ModCarPart> carParts = _packet.Read<List<ModCarPart>>();
                int carLoaderID = _packet.ReadInt();

                
                foreach (ModCarPart part in carParts)
                {
                    MelonCoroutines.Start(CarUpdate.HandleNewBodyPart(part, carLoaderID));
                }
                
                var car = ClientData.LoadedCars.First(s => s.Value.carLoaderID == carLoaderID).Value;
                car.receivedBodyParts = true;
                
                _packet.Dispose();
            }

        #endregion
    }
}