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
            string msg = _packet.ReadString();
            string _username = _packet.ReadString();
            
            MelonLogger.Msg($"ID:[{_clientIdCheck}] " + msg);
        
            MelonLogger.Msg($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint}" +
                            $"connected succesfully and is now player{_fromClient}.");

            if (_fromClient != _clientIdCheck)
            {
                MelonLogger.Msg($"Player \"{_username}\" (ID:{_fromClient}) has assumed" +
                                  $" the wrong client ID ({_clientIdCheck})!");
            }
            Server.clients[_fromClient].SendIntoGame(_username);
        }

        public static void KeepAlive(int _fromClient, Packet _packet)
        {
            int clientId = _packet.ReadInt();
        }

        public static void PlayerMovement(int _fromclient, Packet _packet)
        {
            _fromclient = _packet.ReadInt();
            Vector3 position = _packet.ReadVector3();

            if (Movement.MovUpdateQueue.ContainsKey(_fromclient))
            {
                Movement.MovUpdateQueue[_fromclient].Add(position);
            }
            else
            {
                Movement.MovUpdateQueue.Add(_fromclient, new List<Vector3>{position});
            }
        }
        public static void PlayerRotation(int _fromclient, Packet _packet)
        {
            _fromclient = _packet.ReadInt();
            Quaternion _rotation = _packet.ReadQuaternion();

            if (Movement.RotUpdateQueue.ContainsKey(_fromclient))
            {
                Movement.RotUpdateQueue[_fromclient].Add(_rotation);
            }
            else
            {
                Movement.RotUpdateQueue.Add(_fromclient, new List<Quaternion>{_rotation});
            }
        }

        public static void ReceivedModItem(int _fromClient, Packet _packet)
        {
            _fromClient = _packet.ReadInt();
            ModItem _item = _packet.ReadModItem();
            bool status = _packet.ReadBool();
            

            if (status)
            {
                if (ServerData.ItemAddQueue.ContainsKey(_fromClient))
                {
                    ServerData.ItemAddQueue[_fromClient].Add(_item);
                }
                else
                {
                    ServerData.ItemAddQueue.Add(_fromClient, new List<ModItem>(){_item});
                }
            }
            else
            {
                if (ServerData.ItemRemoveQueue.ContainsKey(_fromClient))
                {
                    ServerData.ItemRemoveQueue[_fromClient].Add(_item);
                }
                else
                {
                    ServerData.ItemRemoveQueue.Add(_fromClient, new List<ModItem>(){_item});
                }
            }
        }
        
        public static void ReceivedGroupItem(int _fromClient, Packet _packet)
        {
            _fromClient = _packet.ReadInt();
            ModItemGroup _item = _packet.ReadModItemGroup();
            bool status = _packet.ReadBool();
            

            if (status)
            {
                if (ServerData.GroupItemAddQueue.ContainsKey(_fromClient))
                {
                    ServerData.GroupItemAddQueue[_fromClient].Add(_item);
                }
                else
                {
                    ServerData.GroupItemAddQueue.Add(_fromClient, new List<ModItemGroup>(){_item});
                }
            }
            else
            {
                if (ServerData.GroupItemRemoveQueue.ContainsKey(_fromClient))
                {
                    ServerData.GroupItemRemoveQueue[_fromClient].Add(_item);
                }
                else
                {
                    ServerData.GroupItemRemoveQueue.Add(_fromClient, new List<ModItemGroup>(){_item});
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
            bool partType = _packet.ReadBool();
            if (!partType)
            {
                PartScriptInfo part = _packet.ReadPartScriptInfo();
                ServerSend.carPart(_fromClient, partType, part);
            }
            else
            {
                List<PartScriptInfo> parts = _packet.ReadPartScriptInfoList();
                ServerSend.initialcarPart(_fromClient, partType, parts);
            }

            //MelonLogger.Msg("SV: Received new Part! sending to players.");
        }

        public static void bodyPart(int _fromClient, Packet _packet)
        {
            _fromClient = _packet.ReadInt();
            carPartsData bodyParts = _packet.ReadBodyPart();

           // MelonLogger.Msg("SV: Received new bodyPart! sending to players.");
            ServerSend.bodyPart(_fromClient, bodyParts);
        }


        public static void lifterState(int _fromClient, Packet _packet)
        {
            _fromClient = _packet.ReadInt();
            CarLifterState lifterState = _packet.ReadCarLifterState();
            int carLoaderID = _packet.ReadInt();

            ServerSend.LifterPos(_fromClient, lifterState, carLoaderID);
        }
        
    }
}