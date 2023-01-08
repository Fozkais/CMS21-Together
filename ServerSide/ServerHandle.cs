using System.Collections.Generic;
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

            if (MainMod.MovUpdateQueue.ContainsKey(_fromclient))
            {
                MainMod.MovUpdateQueue[_fromclient].Add(position);
            }
            else
            {
                MainMod.MovUpdateQueue.Add(_fromclient, new List<Vector3>{position});
            }
        }
        public static void PlayerRotation(int _fromclient, Packet _packet)
        {
            _fromclient = _packet.ReadInt();
            Quaternion _rotation = _packet.ReadQuaternion();

            if (MainMod.RotUpdateQueue.ContainsKey(_fromclient))
            {
                MainMod.RotUpdateQueue[_fromclient].Add(_rotation);
            }
            else
            {
                MainMod.RotUpdateQueue.Add(_fromclient, new List<Quaternion>{_rotation});
            }
        }
    }
}