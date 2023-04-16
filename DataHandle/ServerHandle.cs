using System.Collections.Generic;
using CMS21MP.ClientSide.Functionnality;
using CMS21MP.ServerSide;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.DataHandle
{
    public static class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();
        
            MelonLogger.Msg($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint}" +
                            $"connected succesfully and is now player{_fromClient}.");

            if (_fromClient != _clientIdCheck)
            {
                MelonLogger.Msg($"Player \"{_username}\" (ID:{_fromClient}) has assumed" +
                                  $" the wrong client ID ({_clientIdCheck})!");
            }
            Server.clients[_fromClient].SendIntoGame(_username);
        }

        public static void PlayerMovement(int _fromclient, Packet _packet)
        {
            _fromclient = _packet.ReadInt();
            Vector3 position = _packet.ReadVector3();

            if (Movement_Handling.MovUpdateQueue.ContainsKey(_fromclient))
            {
                Movement_Handling.MovUpdateQueue[_fromclient].Add(position);
            }
            else
            {
                Movement_Handling.MovUpdateQueue.Add(_fromclient, new List<Vector3>{position});
            }
        }
        public static void PlayerRotation(int _fromclient, Packet _packet)
        {
            _fromclient = _packet.ReadInt();
            Quaternion _rotation = _packet.ReadQuaternion();

            if (Movement_Handling.RotUpdateQueue.ContainsKey(_fromclient))
            {
                Movement_Handling.RotUpdateQueue[_fromclient].Add(_rotation);
            }
            else
            {
                Movement_Handling.RotUpdateQueue.Add(_fromclient, new List<Quaternion>{_rotation});
            }
        }

        public static void PlayerInventory(int _fromClient, Packet _packet)
        {
            _fromClient = _packet.ReadInt();
            string _itemID = _packet.ReadString();
            float _itemCondition = _packet.ReadFloat();
            int _itemQuality = _packet.ReadInt();
            long _itemUID = _packet.ReadLong();
            bool status = _packet.ReadBool();

            Item item = new Item();
            item.ID = _itemID;
            item.Condition = _itemCondition;
            item.Quality = _itemQuality;
            item.UID = _itemUID;
            
            
           // Item item = _packet.ReadItem();
           // MelonLogger.Msg($"SV: Received ItemFromClient[{_fromClient}], ID:{item.ID}, UID:{item.UID}, Type:{status}");

            if (status)
            {
                if (ServerData.AddItemQueue.ContainsKey(_fromClient))
                {
                    ServerData.AddItemQueue[_fromClient].Add(item);
                }
                else
                {
                    ServerData.AddItemQueue.Add(_fromClient, new List<Item>(){item});
                }
            }
            else
            {
                if (ServerData.RemoveItemQueue.ContainsKey(_fromClient))
                {
                    ServerData.RemoveItemQueue[_fromClient].Add(item);
                }
                else
                {
                    ServerData.RemoveItemQueue.Add(_fromClient, new List<Item>(){item});
                }
            }
        }
        public static void PlayerMoney(int _fromClient, Packet _packet)
        {
            _fromClient = _packet.ReadInt();
            int _money =  _packet.ReadInt();
            bool status = _packet.ReadBool();
            
           ServerSend.PlayerMoney(_fromClient,_money, status);
        }
        public static void PlayerScene(int _fromClient, Packet _packet)
        {
            _fromClient = _packet.ReadInt();
            string _username = _packet.ReadString();
            string _scene = _packet.ReadString();
            
            MelonLogger.Msg($"SV : Received scene: {_scene} from client with ID:{_fromClient} !");
            ServerSend.PlayerScene(_fromClient, _username, _scene);
        }

        public static void SpawnCars(int _fromClient, Packet _packet)
        { 
            _fromClient = _packet.ReadInt();
            carData data = _packet.ReadCarData();

            //CarLoader carLoader = _packet.ReadCarLoader();
            
           //MelonLogger.Msg($"SV: Received carLoader : Color[{carLoader.color}], PlaceNo[{carLoader.placeNo}], carID[{carLoader.carToLoad}]");
           
           MelonLogger.Msg($"SV: Received new carInfo : ID[{data.carID}], LoaderID:[{data.carLoaderID}], carPos[{data.carPosition}], Status[{data.status}]");
           //ServerData.carList.Add(data);
           ServerSend.SpawnCars(_fromClient, data);
        }
        
        public static void MoveCar(int _fromClient, Packet _packet)
        {
            _fromClient = _packet.ReadInt();
            int carPosition = _packet.ReadInt();
            int carLoaderID = _packet.ReadInt();
            
            MelonLogger.Msg("SV: Received new carPosition! sending new pos to players.");

            //ServerData.carListHandle[carLoaderID].carPosition = carPosition;
            ServerSend.MoveCar(_fromClient, carPosition, carLoaderID);
        }

        public static void carParts(int _fromClient, Packet _packet)
        {
            //MelonLogger.Msg("SV: Received new carParts! sending to players.");
            
            _fromClient = _packet.ReadInt();
            PartScriptInfo parts = _packet.ReadBPartScriptInfo();

            ServerSend.carPart(_fromClient, parts);
        }

        public static void bodyPart(int _fromClient, Packet _packet)
        {
            _fromClient = _packet.ReadInt();
            C_carPartsData bodyParts = _packet.ReadBodyPart();
            int carLoaderID = _packet.ReadInt();

            ServerSend.bodyPart(_fromClient, carLoaderID, bodyParts);
            MelonLogger.Msg("SV: Received new bodyPart! sending to players.");
        }
    }
}