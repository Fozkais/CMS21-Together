using System.Net;
using CMS21MP.ClientSide;
using CMS21MP.CustomData;
using CMS21MP.SharedData;
using MelonLoader;

namespace CMS21MP.ServerSide.DataHandle
{
    public static class ServerHandle
    {

        #region Lobby and Connection
        
            public static void WelcomeReceived(int _fromClient, Packet _packet)
            {
                int _clientIdCheck = _packet.ReadInt();
                string _username = _packet.ReadString();

                MelonLogger.Msg($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected succesfully and is now {_username}.");

                if (_fromClient != _clientIdCheck)
                {
                    MelonLogger.Msg($"Player \"{_username}\" (ID:{_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
                }
                Server.clients[_fromClient].SendToLobby(_username);
            }
            
            public static void Disconnect(int _fromClient, Packet _packet)
            {
                int id = _packet.ReadInt();
                
                Server.clients[_fromClient].Disconnect(id);
                MelonLogger.Msg($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} " + $"has disconnected.");
            }
            
            public static void ReadyState(int _fromClient, Packet _packet)
            {
                bool _ready = _packet.ReadBool();
                int _id = _packet.ReadInt();

                Server.clients[_id].player.isReady = _ready;

                ServerSend.SendReadyState(_fromClient,_ready, _id);
            }
            
        #endregion
        
        #region Movement and Rotation
        
            public static void playerPosition(int _fromClient, Packet _packet)
            {
                Vector3Serializable _position = _packet.Read<Vector3Serializable>();
                Server.clients[_fromClient].player.position = _position;
                
                ServerSend.SendPosition(_fromClient, _position);
               // MelonLogger.Msg("Received position from:" + Server.clients[_fromClient].player.username);
            }
            
            public static void playerRotation(int _fromClient, Packet _packet)
            {
                QuaternionSerializable _rotation = _packet.Read<QuaternionSerializable>();
                Server.clients[_fromClient].player.rotation = _rotation;
                
                ServerSend.SendRotation(_fromClient, _rotation);
            }
        
        #endregion

        #region Car

        public static void CarInfo(int _fromClient, Packet _packet)
        {
            ModCar car = _packet.Read<ModCar>();

            ServerSend.CarInfo(_fromClient, car);
        }
        
        public static void CarPosition(int _fromClient, Packet _packet)
        {
            int carLoaderID = _packet.ReadInt();
            int carPosition = _packet.ReadInt();
            
            ServerSend.CarPosition(_fromClient, carLoaderID, carPosition);
        }

        public static void CarPart(int _fromClient, Packet _packet)
        {
            int carLoaderID = _packet.ReadInt();
            ModPartScript carPart = _packet.Read<ModPartScript>();
            
            ServerSend.CarPart(_fromClient, carLoaderID, carPart);
        }

        #endregion
    }
}