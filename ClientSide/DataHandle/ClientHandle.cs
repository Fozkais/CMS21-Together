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
                
            }

            public static void ReadyState(Packet _packet)
            {
                bool _ready = _packet.ReadBool();
                int _id = _packet.ReadInt();

                ClientData.serverPlayers[_id].isReady = _ready;
            }
            
            public static void PlayersInfo(Packet _packet)
            {
                Player info = _packet.Read<Player>();
                ClientData.serverPlayers[info.id] = info;
                
                MelonLogger.Msg($"Received {info.username} info from server.");
            }
            
            public static void StartGame(Packet _packet)
            {
                SaveSystem.LoadSave(0, "client", true);
            }
            
            public static void SpawnPlayer(Packet _packet)
            {
                if(!GameData.DataInitialzed)
                    GameData.InitializeGameData();
                
                Player _player = _packet.Read<Player>();
                int _id = _packet.ReadInt();
                ClientData.serverPlayers[_id] = _player;
                MelonLogger.Msg($"Received {_player.username} spawn info from server.");
                if (ClientData.serverPlayers.TryGetValue(_id, out var player))
                {
                    if(!ClientData.serverPlayerInstances.ContainsKey(player.id))
                        ClientData.SpawnPlayer(_player, _id);
                }
            }
            
        #endregion

        #region Movement and Rotation

            public static void playerPosition(Packet _packet)
            {
                int _id = _packet.ReadInt();
                Vector3Serializable _position = _packet.Read<Vector3Serializable>();
                Movement.UpdatePlayersPosition(ClientData.serverPlayers[_id], _position);
                
                MelonLogger.Msg("Received position from server.");
            }
            
            public static void playerRotation(Packet _packet)
            {
                int _id = _packet.ReadInt();
                QuaternionSerializable _rotation = _packet.Read<QuaternionSerializable>();
                Movement.UpdatePlayersRotation(ClientData.serverPlayers[_id], _rotation);
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
            }
            else
            {
                ClientData.carOnScene.Add(car);
                
                carLoader.gameObject.GetComponentInChildren<CarDebug>().LoadCar(car.carID, car.carVersion);

                carLoader.placeNo = car.carPosition;
                carLoader.PlaceAtPosition();
                carLoader.ChangePosition(car.carPosition);
            }
        }
        
        public static void CarPosition(Packet _packet)
        {
            int carLoaderID = _packet.ReadInt();
            int carPosition = _packet.ReadInt();
            
            ClientData.carOnScene.Find(s => s.carLoaderID == carLoaderID).carPosition = carPosition;
            ClientData.carLoaders[carLoaderID].ChangePosition(carPosition);
        }
        
        public static void CarPart(Packet _packet)
        {
            int carLoaderID = _packet.ReadInt();
            ModPartScript carPart = _packet.Read<ModPartScript>();

            if (ClientData.carOnScene.Find(s => s.carLoaderID == carLoaderID) != null)
            {
                Car.HandleNewPart(carLoaderID, carPart);
            }
        }
        
        public static void CarPartSize(Packet _packet)
        {
            int carLoaderID = _packet.ReadInt();
            int engineSize = _packet.ReadInt();
            int suspensionSize = _packet.ReadInt();
            int otherSize = _packet.ReadInt();
            var car = ClientData.carOnScene.Find(s => s.carLoaderID == carLoaderID);
            
            MelonLogger.Msg("Received car part size from server.");
            MelonLogger.Msg("Engine:" + engineSize + " Suspension:" + suspensionSize + " Other:" + otherSize);

            if (car != null)
            {
                car.partInfo.enginePartsCount = engineSize;
                car.partInfo.suspensionPartsCount = suspensionSize;
                car.partInfo.otherPartsCount = otherSize;
            }
        }

        #endregion
    }
}