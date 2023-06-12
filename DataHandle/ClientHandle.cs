using System.Collections.Generic;
using System.Threading.Tasks;
using CMS21MP.ClientSide;
using CMS21MP.ClientSide.Functionnality;
using CMS21MP.DataHandle.CL_Handle;
using Il2Cpp;
using Il2CppSystem;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using CarPart = CMS21MP.ClientSide.Functionnality.CarPart;
using Inventory = CMS21MP.ClientSide.Functionnality.Inventory;
using Object = UnityEngine.Object;

namespace CMS21MP.DataHandle
{
    public class ClientHandle : MonoBehaviour
    {
        public static void Welcome(Packet _packet)
        {
            string _msg = _packet.ReadString();
            int _myId = _packet.ReadInt();
            int _serverMoney = _packet.ReadInt();
            
            MelonLogger.Msg($"Message from Server:{_msg}");
            Client.instance.myId = _myId;
            ClientSend.WelcomeReceived();
            if (_myId != 1)
            {
                ClientSend.AskData(_myId);
            }
            //ClientSend.PlayerScene(SceneManager.GetActiveScene().name);
            
            MainMod.isConnected = true;

            if (!MainMod.isHosting)
            {
                MainMod.localInventory.items.Clear();
                MainMod.localInventory.groups.Clear();
                GlobalData.PlayerMoney = _serverMoney;
                ClientSide.Functionnality.Stats.moneyHandler = _serverMoney;
            }
        }

        public async static void SendData(Packet _packet)
        {
            int ClientAsking = _packet.ReadInt();
            MelonLogger.Msg($"User:{ClientAsking} asked data.");
            
            foreach (Item item in Inventory.currentItems)
            {
                ClientSend.SendItem(new ModItem(item), true, ClientAsking);
            }
            foreach (GroupItem item in Inventory.currentGroupItems)
            {
                ClientSend.SendGroupItem(new ModItemGroup(item, Inventory.currentGroupItems.IndexOf(item)), true, ClientAsking);
            }

            foreach (KeyValuePair<int, carData> car in CarSpawn.CarHandle)
            {
                ClientSend.SpawnCars(car.Value, ClientAsking);
            }

            await Task.Delay(1000);

            foreach (KeyValuePair<int, Dictionary<int, List<PartScriptInfo>>> parts in MPGameManager.PartsHandle)
            {
                foreach (List<PartScriptInfo> part in parts.Value.Values)
                {
                    foreach (PartScriptInfo _part in part)
                    {
                        ClientSend.carParts(_part, ClientAsking);
                    }
                }
            }
            foreach (KeyValuePair<int, Dictionary<int, PartScriptInfo>> parts in MPGameManager.EnginePartsHandle)
            {
                foreach (PartScriptInfo _part in parts.Value.Values)
                {
                    ClientSend.carParts(_part, ClientAsking);
                }
            }
            foreach (KeyValuePair<int, Dictionary<int, List<PartScriptInfo>>> parts in MPGameManager.SuspensionPartsHandle)
            {
                foreach (List<PartScriptInfo> part in parts.Value.Values)
                {
                    foreach (PartScriptInfo _part in part)
                    {
                        ClientSend.carParts(_part, ClientAsking);
                    }
                }
            }
            foreach (KeyValuePair<int, Dictionary<int, carPartsData>> parts in BodyPart.BodyPartsHandle)
            {
                foreach (carPartsData part in parts.Value.Values)
                {
                    ClientSend.bodyParts(part, ClientAsking);
                }
            }
        }

        public static void versionMismatch(Packet _packet)
        {
            string version = _packet.ReadString();
            Client.instance.Disconnect();
            MelonLogger.Msg($"Host uses version:{version} installed:{MainMod.ASSEMBLY_MOD_VERSION}");
        }

        public static void DLC(Packet _packet)
        {
            Dictionary<string, bool> _dlc = _packet.ReadDLCState();
            Client.instance.Disconnect();
            Client.forceDisconnected = true;
            foreach (KeyValuePair<string, bool> dlc in _dlc)
            {
                if (dlc.Value)
                {
                    MelonLogger.Msg($"{dlc.Value} need to be removed");
                }
                else
                {
                    MelonLogger.Msg($"{dlc.Value} need to be installed");
                }
            }
        }

        public static void PlayerConnected(Packet _packet)
        {
            int _connected = _packet.ReadInt();
            int _maxConnected = _packet.ReadInt();

            MainMod.playerConnected = _connected;
            MainMod.maxPlayer = _maxConnected;
        }
        
        public static void SpawnPlayer(Packet _packet)
        {
            int _id = _packet.ReadInt();
            string _username = _packet.ReadString();
            Vector3 _position = _packet.ReadVector3();
            Quaternion _rotation = _packet.ReadQuaternion();

            if (!PlayerManager.players.ContainsKey(_id))
            {
                PlayerManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
            }
        }


        public static void PlayerPosition(Packet _packet)
        {
            int _id = _packet.ReadInt();
            Vector3 _position = _packet.ReadVector3();

           // MelonLogger.Msg($"received new pos for player{_id} !");
           if (PlayerManager.players.ContainsKey(_id) && GameObject.Find(PlayerManager.players[_id].username) != null)
           {
               PlayerManager.players[_id].transform.position = _position;
           }
        }
        public static void PlayerRotation(Packet _packet)
        {
            int _id = _packet.ReadInt();
            Quaternion _rotation = _packet.ReadQuaternion();
            
            if (PlayerManager.players.ContainsKey(_id))
            {
                if (GameObject.Find(PlayerManager.players[_id].username) != null)
                {
                    PlayerManager.players[_id].transform.rotation = _rotation;
                }
            }
        }

        public static void ItemReceive(Packet _packet)
        {
            int _playerId = _packet.ReadInt();
            ModItem _Moditem = _packet.ReadModItem();
            bool status = _packet.ReadBool();
            
            InventoryHandle.ItemReceive(_Moditem, status);
            
        }
        
        public static void GroupItemReceive(Packet _packet)
        {
            int _playerId = _packet.ReadInt();
            ModItemGroup _Moditem = _packet.ReadModItemGroup();
            bool status = _packet.ReadBool();
            
            InventoryHandle.GroupItemReceive(_Moditem, status);
        }

        public static void PlayerDisconnect(Packet _packet)
        {
            int _id = _packet.ReadInt();

            if (Client.instance.myId != _id)
            {
                if (PlayerManager.players.ContainsKey(_id) && GameObject.Find(PlayerManager.players[_id].username) != null)
                {
                    Object.Destroy(GameObject.Find(PlayerManager.players[_id].username));
                    PlayerManager.players.Remove(_id);
                }
                if(Movement.MovUpdateQueue.ContainsKey(_id))
                    Movement.MovUpdateQueue.Remove(_id);
                if(Movement.RotUpdateQueue.ContainsKey(_id))
                    Movement.RotUpdateQueue.Remove(_id);

                MainMod.playerConnected -= 1;
            }
            else
            {
                Client.instance.Disconnect();
                foreach (KeyValuePair<int, PlayerInfo> element in PlayerManager.players)
                {
                    if (element.Value != MainMod.localPlayer.GetComponent<PlayerInfo>())
                    {
                        Destroy(element.Value.gameObject);
                    }
                    else
                    {
                        Destroy(MainMod.localPlayer.GetComponent<PlayerInfo>());
                        Destroy(MainMod.localPlayer.GetComponent<MPGameManager>());
                    }
                }

                MainMod.isPrefabSet = false;
                PlayerManager.players.Clear();
            }


        }

        public static void Stats(Packet _packet)
        {
            int stats = _packet.ReadInt();
            bool status = _packet.ReadBool();
            int type = _packet.ReadInt();
            
            StatsHandle.UpdateStats(stats, status, type);
        }
        public static void PlayerScene(Packet _packet)
        {
            int _id = _packet.ReadInt();
            string _scene = _packet.ReadString();

            PlayerManager.players[_id].activeScene = _scene;
        }

        public static async void SpawnCars(Packet _packet)
        {
            carData data = _packet.ReadCarData();

            MelonLogger.Msg($"CL: Received new car info : ID[{data.carID}], carPos[{data.carPosition}], status[{data.status}, scene[{data.Scene}]");

            var carLoader = MainMod.carLoaders[data.carLoaderID];

            if (data.Scene == SceneManager.GetActiveScene().name)
            {
                CarSpawn.PauseHandle = true;
                if (data.status)
                {
                    if (!CarSpawn.CarHandle.ContainsKey(data.carLoaderID))
                    {
                        carLoader.color = new ModColor().Convert(data.carColor);
                        carLoader.SetFactoryColor(new ModColor().Convert(data.carColor));
                        carLoader.gameObject.GetComponentInChildren<CarDebug>().LoadCar(data.carID, data.configNumber);

                        while(carLoader.placeNo == -1 || !PreCarPart.isSuspensionPartReady(data.carLoaderID) || !carLoader.modelLoaded ||  carLoader.e_engine_h == null || carLoader.Parts._items.Length < 6 || carLoader.carParts._items.Length < 20 || !carLoader.done)
                        {
                            await Task.Delay(250);
                        }

                        await Task.Delay(1500);
                        if(carLoader.placeNo != data.carPosition)
                            carLoader.ChangePosition(data.carPosition);
                        CarSpawn.CarHandle.Add(data.carLoaderID, data);
                    }
                }
                else
                {
                    if (CarSpawn.CarHandle.ContainsKey(data.carLoaderID))
                    {
                        CarSpawn.CarHandle.Remove(data.carLoaderID);
                        CarSpawn.RemoveCar(data.carLoaderID);
                    }

                    carLoader.DeleteCar();
                }

                CarSpawn.PauseHandle = false;
                MelonLogger.Msg("Finished SpawnHandling");
            }
        }
        
        public static void MoveCar(Packet _packet)
        {
            int _carPos = _packet.ReadInt();
            int _carLoaderID = _packet.ReadInt();
            
           // MelonLogger.Msg("CL: Received a new carPos, moving car...");

            if (!String.IsNullOrEmpty(MainMod.carLoaders[_carLoaderID].carToLoad))
            {
                MainMod.carLoaders[_carLoaderID].placeNo = _carPos;
                MainMod.carLoaders[_carLoaderID].PlaceAtPosition();
                MainMod.carLoaders[_carLoaderID].ChangePosition(_carPos);
            }
            
        }

        public static void carParts(Packet _packet)
        {
            PartScriptInfo _part = _packet.ReadPartScriptInfo();
            CarPartHandle.StartUpdating(_part);
        }
        
        public static void bodyPart(Packet _packet)
        {
            carPartsData _bodyPart = _packet.ReadBodyPart();
            BodyPartHandle.StartUpdating(_bodyPart);
        }

        public static void lifterPos(Packet _packet)
        {
            CarLifterState _state = _packet.ReadCarLifterState();
            int _carLoaderID = _packet.ReadInt();
            
            GarageInteractionHandle.UpdateLifter(_state, _carLoaderID);
        }
        
    }
}