using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            Dictionary<string, bool> _dlc = _packet.ReadDLCState();
            string modVersion = _packet.ReadString();

            if (modVersion != MainMod.ASSEMBLY_MOD_VERSION)
                ServerSend.VersionMismatch(_clientIdCheck, MainMod.ASSEMBLY_MOD_VERSION);

            Dictionary<string, bool> _DLCDif = new Dictionary<string, bool>();
            Dictionary<string, bool> _DLCDifferences;
            foreach (KeyValuePair<string, bool> dlc in _dlc)
            {
                if (!dlc.Value && MainMod.DLC.hasDLC[dlc.Key])
                {
                    _DLCDif.Add(dlc.Key, false);
                }
                else if (dlc.Value && !MainMod.DLC.hasDLC[dlc.Key])
                {
                    _DLCDif.Add(dlc.Key, true);
                }
            }

            if (_DLCDif.Count > 0)
            {
                _DLCDifferences = new Dictionary<string, bool>(_DLCDif);
                ServerSend.DLC(_DLCDifferences, _clientIdCheck);
            }
            else
            {
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
            int clientID = _packet.ReadInt();


            if (clientID == 0)
            {
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
            else
            {
                ServerSend.SendItem(_fromClient, _item, status, clientID);
            }
        }
        
        public static void ReceivedGroupItem(int _fromClient, Packet _packet)
        {
            _fromClient = _packet.ReadInt();
            ModItemGroup _item = _packet.ReadModItemGroup();
            bool status = _packet.ReadBool();
            int clientID = _packet.ReadInt();


            if (clientID == 0)
            {
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
            else
            {
                ServerSend.SendGroupItem(_fromClient, _item, status, clientID);
            }
            
        }
        public static void Stats(int _fromClient, Packet _packet)
        {
            _fromClient = _packet.ReadInt();
            int _money =  _packet.ReadInt();
            bool status = _packet.ReadBool();
            int _type = _packet.ReadInt();
            
           ServerSend.Stats(_fromClient,_money, status, _type);
        }
        public static void PlayerScene(int _fromClient, Packet _packet)
        {
            _fromClient = _packet.ReadInt();
            string _scene = _packet.ReadString();
            
            MelonLogger.Msg($"SV : Received scene: {_scene} from client with ID:{_fromClient} !");
            ServerSend.PlayerScene(_fromClient, _scene);
        }

        public static void SpawnCars(int _fromClient, Packet _packet)
        { 
            _fromClient = _packet.ReadInt();
            carData data = _packet.ReadCarData();
            int clientID = _packet.ReadInt();

            MelonLogger.Msg($"SV: Received new carInfo : ID[{data.carID}], LoaderID:[{data.carLoaderID}], carPos[{data.carPosition}], Status[{data.status}]");
           //ServerData.carList.Add(data);
           ServerSend.SpawnCars(_fromClient, data, clientID);
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
            PartScriptInfo part = _packet.ReadPartScriptInfo();
            int clientID = _packet.ReadInt();
            
            ServerSend.carPart(_fromClient, part, clientID);

                //MelonLogger.Msg("SV: Received new Part! sending to players.");
        }

        public static void bodyPart(int _fromClient, Packet _packet)
        {
            _fromClient = _packet.ReadInt();
            carPartsData bodyParts = _packet.ReadBodyPart();
            int clientID = _packet.ReadInt();

           // MelonLogger.Msg("SV: Received new bodyPart! sending to players.");
            ServerSend.bodyPart(_fromClient, bodyParts, clientID);
        }


        public static void lifterState(int _fromClient, Packet _packet)
        {
            _fromClient = _packet.ReadInt();
            CarLifterState lifterState = _packet.ReadCarLifterState();
            int carLoaderID = _packet.ReadInt();

            ServerSend.LifterPos(_fromClient, lifterState, carLoaderID);
        }

        public static void AskData(int _fromClient, Packet _packet)
        {
            _fromClient = _packet.ReadInt();
            ServerSend.AskData(_fromClient);
        }
        
    }
}