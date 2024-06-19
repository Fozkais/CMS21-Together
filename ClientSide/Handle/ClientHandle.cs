using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Campaign;
using CMS21Together.ClientSide.Data.Car;
using CMS21Together.ClientSide.Data.CustomUI;
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
            
            public static void ContentsInfo(Packet _packet)
            {
                ReadOnlyDictionary<string, bool> infos = _packet.Read<ReadOnlyDictionary<string, bool>>();
                ApiCalls.API_M2(infos);
            }
        
            public static void KeepAlive(Packet _packet)
            {
               // MelonLogger.Msg("Start Keeping Alive!");
                ClientData.Instance.isServerAlive = true;
                ClientData.Instance.needToKeepAlive = true;
            }
            
            public static void KeepAliveConfirmation(Packet _packet)
            {
                // Le serveur a confirmé la réception du paquet "Keep Alive"
                ClientData.Instance.isServerAlive = true;
            }
            

            public static void Disconnect(Packet _packet)
            {
                string _msg = _packet.ReadString();
                int id = _packet.ReadInt();
                
                if(!Client.Instance.isConnected) return;
                
                if(id == Client.Instance.Id)
                    MelonLogger.Msg($"You were disconnected from the server :{_msg}");
                else
                    MelonLogger.Msg($"Message from server : {_msg}");
                
                if (id != Client.Instance.Id && id != 1)
                {
                    if(ClientData.Instance.players.ContainsKey(id))
                        ClientData.Instance.players[id].Disconnect();
                }
                else
                {
                    if (!ModSceneManager.isInMenu())
                        GameManager.Instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("Menu", SceneType.Menu, true, false));
                    else
                    {
                        Client.Instance.Disconnect();
                        CustomLobbyMenu.DisableLobby(true); 
                       // CustomUIMain.EnableMultiplayerMenu();
                    }
                    
                    Application.runInBackground = false;
                }
                
                _packet.Dispose();
            }

            public static void ReadyState(Packet _packet)
            {
                bool _ready = _packet.ReadBool();
                int _id = _packet.ReadInt();

                ClientData.Instance.players[_id].isReady = _ready;
                _packet.Dispose();
            }
            
            public static void PlayerInfo(Packet _packet)
            {
                Player info = _packet.Read<Player>();
                ClientData.Instance.players[info.id] = info;
                
                MelonLogger.Msg($"Received {info.username}, {info.id} info from server.");
                _packet.Dispose();
            }
            public static void PlayersInfo(Packet _packet)
            {
                Dictionary<int, Player> info = _packet.Read<Dictionary<int, Player>>();
                if(info != null)
                    ClientData.Instance.players = info;
                else
                    MelonLogger.Msg("Received player info is null!");
                _packet.Dispose();
            }
            
            public static void StartGame(Packet _packet)
            {
               // ModProfileData saveData = _packet.Read<ModProfileData>();

              //  SavesManager.profileData[22] = saveData.ToGame();
               // Singleton<GameManager>.Instance.GameDataManager.ProfileData = SavesManager.profileData;
                
                SavesManager.LoadSave(null, true);
                ModUI.Instance.showModUI = false;
                
                _packet.Dispose();
            }
            
            public static void SpawnPlayer(Packet _packet)
            {
                Player _player = _packet.Read<Player>();
                int _id = _packet.ReadInt();
                ClientData.Instance.players[_id] = _player;

                MelonCoroutines.Start(DelaySpawnPlayer(_id, _player));
                
                _packet.Dispose();
            }

            private static IEnumerator DelaySpawnPlayer(int _id, Player _player)
            {
                while (ModSceneManager.currentScene() != GameScene.garage)
                {
                    yield return new WaitForEndOfFrame();
                }
                
                if (ClientData.Instance.players.TryGetValue(_id, out var player))
                {
                    if(!ClientData.Instance.PlayersGameObjects.ContainsKey(player.id))
                        ClientData.Instance.SpawnPlayer(_player);
                }
                
                if(GameData.DataInitialized)
                    ClientSend.SendInitialPosition(new Vector3Serializable(GameData.Instance.localPlayer.transform.position));
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
                if (ClientData.Instance.players.TryGetValue(id, out var player))
                {
                    player.scene = scene;
                    if (player.scene != ModSceneManager.currentScene())
                    {
                       // MelonLogger.Msg($"Destroying {player.username} gameObject.");
                       if (ClientData.Instance.PlayersGameObjects.TryGetValue(id, out var instance))
                       {
                           if (instance != null)
                           {
                               Destroy(instance);
                               ClientData.Instance.PlayersGameObjects.Remove(id);
                           }
                       }
                    }
                    else
                    {
                        if (ClientData.Instance.PlayersGameObjects.TryGetValue(id, out var instance))
                        {
                            if (instance == null)
                            {
                               // MelonLogger.Msg($"Instance Null for {player.username} with id : {player.id} or {id} , spawning...");
                                ClientData.Instance.SpawnPlayer(player);
                            }
                        }
                        else
                        {
                            ClientData.Instance.SpawnPlayer(player);
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
                        ClientData.Instance.playerMoney = value;
                        GlobalData.PlayerMoney = value;
                        break;
                    case ModStats.scrap:
                        ClientData.Instance.playerScrap = value;
                        GlobalData.PlayerScraps = value;
                        break;
                    case ModStats.exp:
                        ClientData.Instance.playerExp = value;
                        GlobalData.PlayerExp = value;
                        break;
                }
                
                _packet.Dispose();
            }
            
            public static void InventoryItem(Packet _packet)
            {
                ModItem _item = _packet.Read<ModItem>();
                bool status = _packet.ReadBool();

                MelonCoroutines.Start(ModInventory.HandleItem(_item, status));
                _packet.Dispose();
            }
            
            public static void InventoryGroupItem(Packet _packet)
            {
                ModGroupItem _item = _packet.Read<ModGroupItem>();
                bool status = _packet.ReadBool();
                
                MelonLogger.Msg("Received GroupItem: " + status);
                MelonCoroutines.Start(ModInventory.HandleGroupItem(_item, status));
                
                _packet.Dispose();
            }

        #endregion

        #region Garage Interaction

            public static void Lifter(Packet _packet)
            {
                if(ModSceneManager.currentScene() != GameScene.garage) return;
                
                ModLifterState state = _packet.Read<ModLifterState>();
                int carLoaderID = _packet.ReadInt();

                MelonLogger.Msg("Received lifter info!");
                MelonLogger.Msg("State :" + state + "ID: " + carLoaderID);
                
                if (ClientData.Instance.LoadedCars.Any(s => s.Value.carLoaderID == carLoaderID - 1))
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
                    ClientData.Instance.LoadedCars[carLoaderID-1].CarLifterState = (int)state;
                    ModLifterLogic.listenToLifter = true;
                }

                
                _packet.Dispose();
            }
            
            public static void TireChange(Packet _packet)
            {
                if(ModSceneManager.currentScene() != GameScene.garage) return;
                
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
                
                if(ModSceneManager.currentScene() != GameScene.garage) return;
                
                ModWheelBalancerActionType aType = _packet.Read<ModWheelBalancerActionType>();
                ModGroupItem _item = null;
                if(aType == ModWheelBalancerActionType.start || aType == ModWheelBalancerActionType.setGroup)
                     _item = _packet.Read<ModGroupItem>();

                if (aType == ModWheelBalancerActionType.remove)
                {
                    GameData.Instance.wheelBalancer.ResetActions();
                    GameData.Instance.wheelBalancer.Clear();
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
                if(ModSceneManager.currentScene() != GameScene.garage) return;
                
                int angle = _packet.ReadInt();
                
                ModEngineStandLogic.listenToEngineStandLogic = false;
                GameData.Instance.engineStand.IncreaseEngineStandAngle(angle);
                ModEngineStandLogic.listenToEngineStandLogic = true;
                
                _packet.Dispose();
            }   
            
            public static void setEngineOnStand(Packet _packet)
            {
                
                if(ModSceneManager.currentScene() != GameScene.garage) return;
                
                ModItem item = _packet.Read<ModItem>();
                
                ModEngineStandLogic.listenToEngineStandLogic = false;
                GameData.Instance.engineStand.SetEngineOnEngineStand(item.ToGame());
                ModEngineStandLogic.listenToEngineStandLogic = true;

                ModEngineStandLogic.stopCoroutine = true;
                ClientData.Instance.engineStand.isReferenced = false;
                ClientData.Instance.engineStand.isHandled = false;
                ClientData.Instance.engineStand.engineStandParts.Clear();
                ClientData.Instance.engineStand.engineStandPartsReferences.Clear();
                ClientData.Instance.engineStand.engine = item;
                ModEngineStandLogic.stopCoroutine = false;
                ModEngineStandLogic.engineUpdating = false;
                
                _packet.Dispose();
            }   
            
            public static void setGroupEngineOnStand(Packet _packet)
            {
                ModGroupItem item = _packet.Read<ModGroupItem>();
                Vector3Serializable pos = _packet.Read<Vector3Serializable>();
                QuaternionSerializable rot = _packet.Read<QuaternionSerializable>();
                
                MelonLogger.Msg("Received new EngineStand");

                MelonCoroutines.Start(SetGroupengineOnStandCoroutine(item, pos, rot));
                
                _packet.Dispose();
            }

            private static IEnumerator SetGroupengineOnStandCoroutine(ModGroupItem item,  Vector3Serializable pos,  QuaternionSerializable rot)
            {
                if(ModSceneManager.currentScene() != GameScene.garage) yield break;
                
                while (!GameData.DataInitialized)
                    yield return new WaitForSeconds(0.2f);

                yield return new WaitForEndOfFrame();
                
                ModEngineStandLogic.listenToEngineStandLogic = false;
                GameData.Instance.engineStand.StartCoroutine(GameData.Instance.engineStand.SetGroupOnEngineStand(item.ToGame(), false));

                
                ModEngineStandLogic.stopCoroutine = true;
                ClientData.Instance.engineStand.isReferenced = false;
                ClientData.Instance.engineStand.fromServer = true;
                ClientData.Instance.engineStand.isHandled = false;
                ClientData.Instance.engineStand.engineStandParts.Clear();
                ClientData.Instance.engineStand.engineStandPartsReferences.Clear();
                ClientData.Instance.engineStand.Groupengine = item;
                ModEngineStandLogic.stopCoroutine = false;
                ModEngineStandLogic.engineUpdating = false;
                
                yield return new WaitForSeconds(1f);
                yield return new WaitForEndOfFrame();
                MelonLogger.Msg($"ReceivedPos: {pos.toVector3()}");
                MelonLogger.Msg($"ReceivedRot: {rot.toQuaternion()}");
                MelonLogger.Msg($"CurrentPos: {GameData.Instance.engineStand.engineGameObject.transform.position}");
                MelonLogger.Msg($"CurrentRot: {GameData.Instance.engineStand.engineGameObject.transform.rotation}");
                
                GameData.Instance.engineStand.engineGameObject.transform.position = pos.toVector3();
                GameData.Instance.engineStand.engineGameObject.transform.rotation = rot.toQuaternion();
            }

            public static void TakeOffEngineFromStand(Packet _packet)
            {
                if(ModSceneManager.currentScene() != GameScene.garage) return;
                
                ModEngineStandLogic.listenToEngineStandLogic = false;
                GameData.Instance.localInventory.AddGroup(GameData.Instance.engineStand.GetGroupOnEngineStand());
                GameData.Instance.engineStand.ClearEngineStand();
                ModEngineStandLogic.listenToEngineStandLogic = true;
                
                _packet.Dispose();
            }  
            
            public static void EngineCrane(Packet _packet)
            {
                if(ModSceneManager.currentScene() != GameScene.garage) return;
                
                bool action = _packet.ReadBool();
                if (!action)
                {
                    ModGroupItem item = _packet.Read<ModGroupItem>();
                    ModEngineCrane.listentoCraneAction = false;
                    NotificationCenter.Get().InsertEngineToCar(item.ToGame());
                    ModEngineCrane.listentoCraneAction = true;
                }
                else
                {
                    int carLoaderID = _packet.ReadInt();
                    ModEngineCrane.listentoCraneAction = false;
                    GameData.Instance.carLoaders[carLoaderID].UseEngineCrane();
                    ModEngineCrane.listentoCraneAction = true;
                }

                
                _packet.Dispose();
            }  
            
            public static void SpringClampGroup(Packet _packet)
            {
                ModGroupItem item = _packet.Read<ModGroupItem>();
                bool instant = _packet.ReadBool();
                bool mount = _packet.ReadBool();

                MelonCoroutines.Start(SpringClamp1(item, instant, mount));
            }

            private static IEnumerator SpringClamp1(ModGroupItem item, bool instant, bool mount)
            {
                if(ModSceneManager.currentScene() != GameScene.garage) yield break;
                
                while (!GameData.DataInitialized)
                    yield return new WaitForSeconds(0.2f);
                
                ModSpringClampLogic.listenToSpringClampLogic = false;
                
                GameData.Instance.springClampLogic.SetGroupOnSpringClamp(item.ToGame(), instant, mount);
                ModSpringClampLogic.listenToSpringClampLogic = true;
                MelonLogger.Msg("Received SpringClampGroup");
            }
            
            public static void SpringClampClear(Packet _packet)
            {
                if(ModSceneManager.currentScene() != GameScene.garage) return;
                
                ModSpringClampLogic.listenToSpringClampLogic = false;
                if(GameData.Instance.springClampLogic.GroupOnSpringClamp != null)
                    if(GameData.Instance.springClampLogic.GroupOnSpringClamp.ItemList != null)
                        GameData.Instance.springClampLogic.GroupOnSpringClamp.ItemList.Clear();
                GameData.Instance.springClampLogic.ClearSpringClamp();
                ModSpringClampLogic.listenToSpringClampLogic = true;
                
                MelonLogger.Msg(" Received Clear SpringClamp");
            }
            
            public static void OilBin(Packet _packet)
            {
                int loaderID = _packet.ReadInt();

                if(ModSceneManager.currentScene() != GameScene.garage) return;
                
                ModOilBin.listenToOilBinAction = false;
                GameData.Instance.carLoaders[loaderID].UseOilbin();
                ModOilBin.listenToOilBinAction = true;
            }
            
            
            public static void ToolsMove(Packet _packet)
            {
                ModIOSpecialType tool = _packet.Read<ModIOSpecialType>();
                ModCarPlace place = _packet.Read<ModCarPlace>();
                bool playSound = _packet.ReadBool();

                ModToolMoveManager.listenToMove = false;
                if(place == ModCarPlace.none)
                    ToolsMoveManager.m_instance.SetOnDefaultPosition((IOSpecialType)tool);
                else
                    ToolsMoveManager.m_instance.MoveTo((IOSpecialType)tool,(CarPlace)place, playSound);
                ModToolMoveManager.listenToMove = true;
            }

        #endregion

        #region CarData

            public static void CarInfo(Packet _packet)
            {
                bool removed = _packet.ReadBool();
                ModCar car = _packet.Read<ModCar>();
                
                MelonLogger.Msg($"CL: Received new car from server!");

                MelonCoroutines.Start(CarInfo(removed, car));
                _packet.Dispose();
            }

            private static IEnumerator CarInfo(bool removed, ModCar car)
            {
                while(ClientData.Instance.GameReady == false) // DO NOT REMOVE!
                    yield return new WaitForSeconds(1);
                
                MelonLogger.Msg($"GameReady processing car : {car.carID}");
                
                CarLoader carLoader = GameData.Instance.carLoaders[car.carLoaderID];

                bool checkCondition = ClientData.Instance.LoadedCars.Any(s =>
                    s.Value.carLoaderID == car.carLoaderID && s.Value.carID == car.carID);
                
                if (!removed)
                {
                    if (!checkCondition)
                    {
                        ClientData.Instance.LoadedCars.Add(car.carLoaderID, car);
                    }
                    ModCar _car  = ClientData.Instance.LoadedCars.First(s 
                        => s.Value.carLoaderID == car.carLoaderID && s.Value.carID == car.carID).Value;
                    
                    MelonCoroutines.Start(CarManagement.CarSpawnFade(_car));
                    MelonLogger.Msg($"CL: Loading:{_car.carID} , {car.carLoaderID}");
                }
                else
                {
                    if (checkCondition)
                    {
                        ClientData.Instance.LoadedCars.Remove(ClientData.Instance.LoadedCars.First(s => 
                            s.Value.carLoaderID == car.carLoaderID && s.Value.carID == car.carID).Key);
                        CarHarmonyHooks.ListenToDeleteCar = false;
                        carLoader.DeleteCar();
                        CarHarmonyHooks.ListenToDeleteCar = true;
                        MelonLogger.Msg("CL: Removing car...");
                    }
                }
            }

            public static void CarPart(Packet _packet)
            {
                ModPartScript carPart = _packet.Read<ModPartScript>();
                int carLoaderID = _packet.ReadInt();
                

                if (carLoaderID == -1)
                {
                    MelonCoroutines.Start( ModEngineStandLogic.HandleNewPart(carPart));
                    _packet.Dispose();
                    return;
                }
                
                MelonCoroutines.Start(CarUpdate.HandleNewPart(carPart, carLoaderID));
                
                _packet.Dispose();
            }
            
            public static void CarPosition(Packet _packet)
            {
                int carLoaderID = _packet.ReadInt();
                int carPosition = _packet.ReadInt();

                MelonCoroutines.Start(CarPositionCoroutine(carLoaderID, carPosition));
                _packet.Dispose();
            }

            private static IEnumerator CarPositionCoroutine(int carLoaderID, int carPosition)
            {
                while(ClientData.Instance.GameReady == false) // DO NOT REMOVE!
                    yield return new WaitForSeconds(1);

                if (ClientData.Instance.LoadedCars.Any(s => s.Value.carLoaderID == carLoaderID))
                {
                    ClientData.Instance.LoadedCars.First(s => s.Value.carLoaderID == carLoaderID).Value.carPosition = carPosition;
                    GameData.Instance.carLoaders[carLoaderID].ChangePosition(carPosition);
                }
            }
            
            public static void CarParts(Packet _packet)
            {
                List<ModPartScript> carParts = _packet.Read<List<ModPartScript>>();
                int carLoaderID = _packet.ReadInt();
                ModPartType partType = _packet.Read<ModPartType>();

                MelonLogger.Msg($"Received parts list for : {partType}!!");
                if(carParts.Count > 0)
                    MelonCoroutines.Start(CarManagement.LoadCarParts(carParts, carLoaderID));

                MelonCoroutines.Start(CarReceivedUpdate(partType, carLoaderID));

                _packet.Dispose();
            }

            private static IEnumerator CarReceivedUpdate(ModPartType carPart, int carLoaderID)
            {
                var waitforCar = MelonCoroutines.Start(CarManagement.WaitCarToBeReady(carLoaderID));
                yield return waitforCar;
                
                ModCar car = ClientData.Instance.LoadedCars.First(s => s.Value.carLoaderID == carLoaderID).Value;
                switch (carPart)
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
            }

            public static void BodyPart(Packet _packet)
            {
                ModCarPart carPart = _packet.Read<ModCarPart>();
                int carLoaderID = _packet.ReadInt();
                
                MelonLogger.Msg("Received BodyPart");
                
                MelonCoroutines.Start(CarUpdate.HandleNewBodyPart(carPart, carLoaderID));
                
                _packet.Dispose();
            }
            
            public static void BodyParts(Packet _packet)
            {
                List<ModCarPart> carParts = _packet.Read<List<ModCarPart>>();
                int carLoaderID = _packet.ReadInt();
                
                MelonLogger.Msg("Received BodyParts list !!");

                MelonCoroutines.Start(CarManagement.LoadBodyParts(carParts, carLoaderID));

                MelonCoroutines.Start(CarReceivedUpdate(carLoaderID));
                
                _packet.Dispose();
            }
            
            private static IEnumerator CarReceivedUpdate(int carLoaderID)
            {
                var waitforCar = MelonCoroutines.Start(CarManagement.WaitCarToBeReady(carLoaderID));
                yield return waitforCar;
                
                var car = ClientData.Instance.LoadedCars.First(s => s.Value.carLoaderID == carLoaderID).Value;
                car.receivedBodyParts = true;
            }

        #endregion

        #region CampaignSync

            public static void GarageUpgrade(Packet _packet)
            {
                bool interactive = _packet.ReadBool();
                string upgradeID = _packet.ReadString();
                bool on = _packet.ReadBool();

               
                
                _packet.Dispose();
            }

            public IEnumerator GarageUpgrade(bool interactive, string upgradeID,  bool on)
            {
                while (!GameData.DataInitialized)
                    yield return new WaitForSeconds(.1f);
                
                CampaignHarmonyHooks.ListenToUpgrades = false;
                var upgradeTool = GameData.Instance.garageAndToolsTab;

                if (interactive)
                    upgradeTool.SwitchInteractiveObjects(upgradeID, on);
                else
                    upgradeTool.StartCoroutine(upgradeTool.SwitchObjectsUnlock(upgradeID, on));
            }

        #endregion

    }
}