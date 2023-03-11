using System.Collections;
using System.Collections.Generic;
using System.Net;
using CMS21MP.DataHandle;
using Il2Cpp;
using Il2CppSystem;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CMS21MP.ClientSide
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
            //ClientSend.PlayerScene(SceneManager.GetActiveScene().name);

            Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
            MainMod.isConnected = true;

            if (!MainMod.isHosting)
            {
                MainMod.localInventory.items.Clear();
                GlobalData.PlayerMoney = _serverMoney;
                playerManagement.moneyHandler = _serverMoney;
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
           if (PlayerManager.players.ContainsKey(_id) && GameObject.Find($"{PlayerManager.players[_id].username}") != null)
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
                if (GameObject.Find($"{PlayerManager.players[_id].username}") != null)
                {
                    PlayerManager.players[_id].transform.rotation = _rotation;
                }
            }
        }

        public static void PlayerInventory(Packet _packet)
        {
            int _playerId = _packet.ReadInt();
            string _itemID = _packet.ReadString();
            float _itemCondition = _packet.ReadFloat();
            int _itemQuality = _packet.ReadInt();
            long _itemUID = _packet.ReadLong();
            bool status = _packet.ReadBool();

            Item _item = new Item();
            _item.ID = _itemID;
            _item.Condition = _itemCondition;
            _item.Quality = _itemQuality;
            _item.UID = _itemUID;
            
           // MelonLogger.Msg($"Received ItemFromServer, ID:{_itemID}, UID:{_itemUID}, Type:{status}");

            if (status)
            {
                if (!playerManagement.ItemsUID.Contains(_itemUID) )
                {
                   // MelonLogger.Msg($"Adding Item with UID:{_itemUID}");
                    playerManagement.InventoryHandler.Add(_item);
                    playerManagement.ItemsUID.Add(_itemUID);
                    MainMod.localInventory.Add(_item);
                }
            }
            else
            {
                if (playerManagement.ItemsUID.Contains(_itemUID))
                {
                   // MelonLogger.Msg($"Removing Item with UID:{_itemUID}");
                    playerManagement.InventoryHandler.Remove(_item);
                    playerManagement.ItemsUID.Remove(_itemUID);
                    MainMod.localInventory.Delete(_item);
                }
            }
            
        }

        public static void PlayerDisconnect(Packet _packet)
        {
            int _id = _packet.ReadInt();

            if (PlayerManager.players.ContainsKey(_id) && GameObject.Find(PlayerManager.players[_id].username) != null)
            {
                Object.Destroy(GameObject.Find(PlayerManager.players[_id].username));
                PlayerManager.players.Remove(_id);
            }
            if(DataUpdating.MovUpdateQueue.ContainsKey(_id))
                DataUpdating.MovUpdateQueue.Remove(_id);
            if(DataUpdating.RotUpdateQueue.ContainsKey(_id))
                DataUpdating.RotUpdateQueue.Remove(_id);

        }

        public static void PlayerMoney(Packet _packet)
        {
            int money = _packet.ReadInt();
            bool status = _packet.ReadBool();
            if (status)
            {
                MelonLogger.Msg($"Adding Money!! +{money}");
                GlobalData.AddPlayerMoney(money);
                playerManagement.moneyHandler = GlobalData.PlayerMoney;
            }
            else
            {
                MelonLogger.Msg($"Removing Money!! {money}");
                GlobalData.AddPlayerMoney(money);
                playerManagement.moneyHandler = GlobalData.PlayerMoney;
            }
        }
        public static void PlayerScene(Packet _packet)
        {
            int _id = _packet.ReadInt();
            string _username = _packet.ReadString();
            string _scene = _packet.ReadString();
            
            MelonLogger.Msg($"received new scene from {_username}, scene: {_scene}");

            if (SceneManager.GetActiveScene().name == _scene && GameObject.Find($"{_username}") == null)
            {
                PlayerManager.instance.SpawnPlayer(_id, _username, new Vector3(0,0,0), new Quaternion(0,0,0,0));
            }
            else if(GameObject.Find($"{_username}") != null && _id != Client.instance.myId)
            {
                Object.Destroy(GameObject.Find($"{_username}"));
                PlayerManager.players.Remove(_id);
            }
        }

        public static void SpawnCars(Packet _packet)
        {
            carData data = _packet.ReadCarData();

            MelonLogger.Msg($"CL: Received new car info : ID[{data.carID}], carPos[{data.carPosition}], status[{data.status}");
            
            if (data.status)
            {
                if (!playerManagement.carHandler.Contains(data))
                {
                    playerManagement.carHandler.Add(data);
                }

                Color carColor = new Color(data.carColor.r,data.carColor.g,data.carColor.b,data.carColor.a);

                MainMod.carLoaders[data.carLoaderID].placeNo = data.carPosition;
                MainMod.carLoaders[data.carLoaderID].PlaceAtPosition();
                MainMod.carLoaders[data.carLoaderID].color = carColor;
                MainMod.carLoaders[data.carLoaderID].gameObject.GetComponentInChildren<CarDebug>().LoadCar(data.carID);
            }
            else
            {
                if (playerManagement.carHandler.Contains(data))
                {
                    playerManagement.carHandler.Remove(data);
                }
                
                MainMod.carLoaders[data.carLoaderID].DeleteCar();
            }
            
        }
        
        public static void MoveCar(Packet _packet)
        {
            int _carPos = _packet.ReadInt();
            int _carLoaderID = _packet.ReadInt();
            
            MelonLogger.Msg("CL: Received a new carPos, moving car...");

            if (!String.IsNullOrEmpty(MainMod.carLoaders[_carLoaderID].carToLoad))
            {
                MainMod.carLoaders[_carLoaderID].placeNo = _carPos;
                MainMod.carLoaders[_carLoaderID].PlaceAtPosition();
                MainMod.carLoaders[_carLoaderID].ChangePosition(_carPos);
            }
            
        }
        
        public static void carParts(Packet _packet)
        {
            int _carLoaderID = _packet.ReadInt();
            C_PartsData _carParts = _packet.ReadCarPart();
            
            MelonLogger.Msg("CL: Received a new carParts");

            if (!String.IsNullOrEmpty(MainMod.carLoaders[_carLoaderID].carToLoad))
            {
                MainMod.carLoaders[_carLoaderID].Parts._items[_carParts.partID].p_name = _carParts.name;
                MainMod.carLoaders[_carLoaderID].Parts._items[_carParts.partID].p_position =
                    new Vector3(_carParts.positionX, _carParts.positionY, _carParts.positionZ);
                MainMod.carLoaders[_carLoaderID].Parts._items[_carParts.partID].p_rotation = 
                    new Vector3(_carParts.rotationX, _carParts.rotationY, _carParts.rotationZ);
                MainMod.carLoaders[_carLoaderID].Parts._items[_carParts.partID].p_scale = _carParts.scale;
                MainMod.carLoaders[_carLoaderID].Parts._items[_carParts.partID].p_reflection = _carParts.reflection;
                
                MelonLogger.Msg($"Parts[{_carParts.partID}] updated!, {_carParts.name}, -> {MainMod.carLoaders[_carLoaderID].Parts._items[_carParts.partID].p_name}");
                
                MainMod.carLoaders[_carLoaderID].SetParts();
                MainMod.carLoaders[_carLoaderID].Update();
                
            }
            
        }

        public static void bodyPart(Packet _packet)
        {
            int _carLoaderID = _packet.ReadInt();
            C_carPartsData _bodyPart = _packet.ReadBodyPart();

            MelonLogger.Msg($"CL: Received a new bodyPart! {_bodyPart.name}");
            //MelonCoroutines.Start(delayUpdate(_bodyPart, _carLoaderID));
              Color color = new Color(_bodyPart.colors.r, _bodyPart.colors.g, _bodyPart.colors.b, _bodyPart.colors.a);
            
            if (!String.IsNullOrEmpty(MainMod.carLoaders[_carLoaderID].carToLoad))
            {
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].name = _bodyPart.name;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].Switched = _bodyPart.switched;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].InProgress = _bodyPart.inprogress;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].Condition = _bodyPart.condition;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].Unmounted = _bodyPart.unmounted;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].TunedID = _bodyPart.tunedID;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].Color = color;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].PaintType = (PaintType)_bodyPart.paintType;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].StructureCondition = _bodyPart.conditionStructure;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].ConditionPaint = _bodyPart.conditionPaint;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].Livery = _bodyPart.livery;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].LiveryStrength = _bodyPart.liveryStrength;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].OutsideRustEnabled = _bodyPart.outsaidRustEnabled;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].AdditionalString = _bodyPart.adtionalString;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].ConnectedParts.Clear();
                    foreach (string attachedPart in _bodyPart.mountUnmountWith)
                    {
                        MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].ConnectedParts.Add(attachedPart);
                    }

                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].Quality = _bodyPart.quality;
                    
                    MainMod.carLoaders[_carLoaderID].SetUnmountWithCarParts();
                    MainMod.carLoaders[_carLoaderID].UpdateCarBodyParts();
                    //MainMod.carLoaders[_carLoaderID].

                    MelonLogger.Msg($"Parts[{_bodyPart.carPartID}] updated!, {_bodyPart.name}, -> {MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].name}, {_bodyPart.unmounted}, -> {MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].Unmounted}");
            }
            else
            {
                MelonLogger.Msg($"Loss of data from bodyPart ! {_bodyPart.name}");
            }

            MelonLogger.Msg($"CL: Received new car info : ID[{data.carID}], carPos[{data.carPosition}], status[{data.status}");
            
            if (data.status)
            {
                if (!playerManagement.carHandler.Contains(data))
                {
                    playerManagement.carHandler.Add(data);
                }
                MainMod.carLoaders[data.carLoaderID].placeNo = data.carPosition;
                MainMod.carLoaders[data.carLoaderID].PlaceAtPosition();
                MainMod.carLoaders[data.carLoaderID].gameObject.GetComponentInChildren<CarDebug>().LoadCar(data.carID);
            }
            else
            {
                if (playerManagement.carHandler.Contains(data))
                {
                    playerManagement.carHandler.Remove(data);
                }
                
                MainMod.carLoaders[data.carLoaderID].DeleteCar();
            }
            
        }
        
        public static void MoveCar(Packet _packet)
        {
            int _carPos = _packet.ReadInt();
            int _carLoaderID = _packet.ReadInt();
            
            MelonLogger.Msg("CL: Received a new carPos, moving car...");

            if (!String.IsNullOrEmpty(MainMod.carLoaders[_carLoaderID].carToLoad))
            {
                MainMod.carLoaders[_carLoaderID].placeNo = _carPos;
                MainMod.carLoaders[_carLoaderID].PlaceAtPosition();
                MainMod.carLoaders[_carLoaderID].ChangePosition(_carPos);
            }
            
        }

        static IEnumerator delayUpdate(C_carPartsData _bodyPart, int _carLoaderID)
        {
            yield return new WaitForSeconds(1);
             Color color = new Color(_bodyPart.colors.r, _bodyPart.colors.g, _bodyPart.colors.b, _bodyPart.colors.a);
            
            if (!String.IsNullOrEmpty(MainMod.carLoaders[_carLoaderID].carToLoad))
            {
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].name = _bodyPart.name;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].Switched = _bodyPart.switched;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].InProgress = _bodyPart.inprogress;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].Condition = _bodyPart.condition;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].Unmounted = _bodyPart.unmounted;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].TunedID = _bodyPart.tunedID;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].Color = color;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].PaintType = (PaintType)_bodyPart.paintType;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].StructureCondition = _bodyPart.conditionStructure;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].ConditionPaint = _bodyPart.conditionPaint;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].Livery = _bodyPart.livery;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].LiveryStrength = _bodyPart.liveryStrength;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].OutsideRustEnabled = _bodyPart.outsaidRustEnabled;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].AdditionalString = _bodyPart.adtionalString;
                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].ConnectedParts.Clear();
                    foreach (string attachedPart in _bodyPart.mountUnmountWith)
                    {
                        MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].ConnectedParts.Add(attachedPart);
                    }

                    MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].Quality = _bodyPart.quality;
                    
                    MainMod.carLoaders[_carLoaderID].SetUnmountWithCarParts();
                    MainMod.carLoaders[_carLoaderID].UpdateCarBodyParts();

                    MelonLogger.Msg($"Parts[{_bodyPart.carPartID}] updated!, {_bodyPart.name}, -> {MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].name}, {_bodyPart.unmounted}, -> {MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].Unmounted}");
            }
            else
            {
                MelonLogger.Msg($"Loss of data from bodyPart ! {_bodyPart.name}");
            }
        }
       
    }
}