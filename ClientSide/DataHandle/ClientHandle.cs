using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21MP.ClientSide.Data;
using CMS21MP.CustomData;
using CMS21MP.ServerSide;
using CMS21MP.SharedData;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CMS21MP.ClientSide.DataHandle
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

            public static void Disconnect(Packet _packet)
            {
                string _msg = _packet.ReadString();
                
                MelonLogger.Msg($"Message from Server:{_msg}");
                var name = SceneManager.GetActiveScene().name;
                if(name == "garage" || name == "Junkyard" || name == "Auto_salon")
                    NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("Menu", SceneType.Menu, true, false));
                else
                    Client.Instance.Disconnect();
                _packet.Dispose();
            }

            public static void ReadyState(Packet _packet)
            {
                bool _ready = _packet.ReadBool();
                int _id = _packet.ReadInt();

                ClientData.serverPlayers[_id].isReady = _ready;
                _packet.Dispose();
            }
            
            public static void PlayersInfo(Packet _packet)
            {
                Player info = _packet.Read<Player>();
                ClientData.serverPlayers[info.id] = info;
                
                MelonLogger.Msg($"Received {info.username} info from server.");
                _packet.Dispose();
            }
            
            public static void StartGame(Packet _packet)
            {
                SaveSystem.LoadSave(0, "client", true);
                _packet.Dispose();
            }
            
            public static void SpawnPlayer(Packet _packet) // TODO: Check this to be broken (other than host dont have correct info on Player instance)
            {
                /*if(!GameData.DataInitialzed)
                    MelonCoroutines.Start(GameData.InitializeGameData());*/
                
                Player _player = _packet.Read<Player>();
                int _id = _packet.ReadInt();
                ClientData.serverPlayers[_id] = _player;
                MelonLogger.Msg($"Received {_player.username} spawn info from server.");
                if (ClientData.serverPlayers.TryGetValue(_id, out var player))
                {
                    if(!ClientData.serverPlayerInstances.ContainsKey(player.id))
                        ClientData.SpawnPlayer(_player, _id);
                }
                _packet.Dispose();
            }
            
        #endregion

        #region Player Infos

            public static void playerPosition(Packet _packet)
            {
                int _id = _packet.ReadInt();
                Vector3Serializable _position = _packet.Read<Vector3Serializable>();
                Movement.UpdatePlayersPosition(_id, _position);
                _packet.Dispose();
            }
            
            public static void playerRotation(Packet _packet)
            {
                int _id = _packet.ReadInt();
                QuaternionSerializable _rotation = _packet.Read<QuaternionSerializable>();
                Movement.UpdatePlayersRotation(_id, _rotation);
                _packet.Dispose();
            }
            
            public static void playerSceneChange(Packet _packet)
            {
                string scene = _packet.ReadString();
                int id = _packet.ReadInt();
                
                MelonLogger.Msg("Received Scene Update :" + scene);

                if (ClientData.serverPlayers.TryGetValue(id, out var player))
                {
                    player.scene = scene;
                    if (player.scene != SceneManager.GetActiveScene().name)
                    {
                        MelonLogger.Msg($"Destroying {player.username} instance");
                        Destroy(ClientData.serverPlayerInstances[id]);
                    }
                    else
                    {
                        if (ClientData.serverPlayerInstances.TryGetValue(id, out var instance))
                        {
                            if (instance == null)
                            {
                                MelonLogger.Msg($"Instance Null for {player.username} with id : {player.id} or {id} , spawning...");
                                ClientData.SpawnPlayer(player, id);
                            }
                        }
                    }
                }
                
                
                
                _packet.Dispose();
            }

        #endregion

        #region Car

        public static void CarInfo(Packet _packet)
        {
            ModCar car = _packet.Read<ModCar>();
            
            
            MelonLogger.Msg($"Received car info from server. ID:{car.carID} Version:{car.carVersion} Pos:{car.carPosition}, Scene:{car.carScene}");
            var carLoader = ClientData.carLoaders[car.carLoaderID];
            if (ClientData.carOnScene.Any(s => s.carID == car.carID && s.carVersion == car.carVersion && s.carPosition == car.carPosition))
            {
                ClientData.carOnScene.Remove(ClientData.carOnScene.Find(s => s.carID == car.carID && s.carVersion == car.carVersion && s.carPosition == car.carPosition));
                carLoader.DeleteCar();
                MelonLogger.Msg("Removing car...");
            }
            else
            {
                MelonLogger.Msg("Loading new car...");
                ClientData.carOnScene.Add(car);
                
                carLoader.gameObject.GetComponentInChildren<CarDebug>().LoadCar(car.carID, car.carVersion);
               // carLoader.StartCoroutine(carLoader.gameObject.GetComponentInChildren<CarDebug>()
                //    .RunLoadCar(car.carID, car.carVersion));

                carLoader.placeNo = car.carPosition; // TODO: Change this to a better way
                carLoader.PlaceAtPosition();
                carLoader.ChangePosition(car.carPosition);
                MelonCoroutines.Start(StartFadeOut());
            }
            _packet.Dispose();
        }
        
        private static IEnumerator StartFadeOut()
        {
            ScreenFader.Get().ShortFadeIn();
            yield return new WaitForSeconds(6);
            ScreenFader.Get().ShortFadeOut();
        }
        
        public static void CarPosition(Packet _packet)
        {
            int carLoaderID = _packet.ReadInt();
            int carPosition = _packet.ReadInt();
            
            ClientData.carOnScene.Find(s => s.carLoaderID == carLoaderID).carPosition = carPosition;
            ClientData.carLoaders[carLoaderID].ChangePosition(carPosition);
            _packet.Dispose();
        }
        
        public static void CarPart(Packet _packet)
        {
            int carLoaderID = _packet.ReadInt();
            ModPartScript carPart = _packet.Read<ModPartScript>();

            //MelonLogger.Msg("Received part for car with id : " + carLoaderID);
            if (ClientData.carOnScene.Find(s => s.carLoaderID == carLoaderID) != null)
            {
                
                MelonCoroutines.Start(Car.HandleNewPart(carLoaderID, carPart));
            }
            _packet.Dispose();
        }
        
        public static void BodyPart(Packet _packet)
        {
            int carLoaderID = _packet.ReadInt();
            ModCarPart carPart = _packet.Read<ModCarPart>();

            if (ClientData.carOnScene.Find(s => s.carLoaderID == carLoaderID) != null)
            {
                MelonCoroutines.Start(Car.HandleNewPart(carLoaderID, null, carPart));
            }
            _packet.Dispose();
        }

        #endregion

        #region Inventory

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
                        ClientData.localInventory.Add(_itemGame);
                    }
                }
                else
                {
                    if (ModInventory.handledItem.Any(s => s.UID == _item.UID))
                    {
                        ModInventory.handledItem.Remove(_item);
                        ClientData.localInventory.Delete(_itemGame);
                    }
                }
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
                        ClientData.localInventory.AddGroup(_item.ToGame(_item));
                    }
                }
                else
                {
                    if (ModInventory.handledGroupItem.Any(s => s.UID == _item.UID))
                    {
                        int index = ModInventory.handledGroupItem.FindIndex(s => s.UID == _item.UID);
                        ModInventory.handledGroupItem.Remove(ModInventory.handledGroupItem[index]);
                        ClientData.localInventory.DeleteGroup(_item.UID);
                    }
                }
            }

        #endregion

        #region Garage Interaction

        public static void LifterPos(Packet _packet)
        {
            int _action = _packet.ReadInt();
            int _pos = _packet.ReadInt();
            int _loaderId = _packet.ReadInt();
            
            MelonLogger.Msg("Received New lifter pos to : " + _loaderId + 
                            " action: " + _action + " pos: " + _pos);

            if (ClientData.carOnScene.Any(s => s.carLoaderID == _loaderId))
            {
                if (ClientData.carOnScene[_loaderId].CarLifterState != _pos)
                {
                    MelonCoroutines.Start(GarageInteraction.PauseUpdating());
                    ClientData.carLoaders[_loaderId].lifter.Action(_action);
                    ClientData.carOnScene[_loaderId].CarLifterState = _pos;
                }
            }
            
        }            

        #endregion
    }
}