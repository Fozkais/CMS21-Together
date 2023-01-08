using CMS21MP.ClientSide;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ServerSide
{
    public class ServerHandle
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
            Quaternion _rotation = _packet.ReadQuaternion();

            // MelonLogger.Msg($"Player{_fromclient} sended new Pos!");
            MainMod.UpdateQueue.Add(_fromclient, position);
        }
    }
}