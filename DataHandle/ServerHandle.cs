using System.Collections.Generic;
using CMS21MP.ClientSide;
using CMS21MP.ServerSide;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.DataHandle
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

            if (DataUpdating.MovUpdateQueue.ContainsKey(_fromclient))
            {
                DataUpdating.MovUpdateQueue[_fromclient].Add(position);
            }
            else
            {
                DataUpdating.MovUpdateQueue.Add(_fromclient, new List<Vector3>{position});
            }
        }
        public static void PlayerRotation(int _fromclient, Packet _packet)
        {
            _fromclient = _packet.ReadInt();
            Quaternion _rotation = _packet.ReadQuaternion();

            if (DataUpdating.RotUpdateQueue.ContainsKey(_fromclient))
            {
                DataUpdating.RotUpdateQueue[_fromclient].Add(_rotation);
            }
            else
            {
                DataUpdating.RotUpdateQueue.Add(_fromclient, new List<Quaternion>{_rotation});
            }
        }

        public static void PlayerInventory(int _fromClient, Packet _packet)
        {
            _fromClient = _packet.ReadInt();
            string _itemID = _packet.ReadString();
            float _itemCondition = _packet.ReadFloat();
            int _itemQuality = _packet.ReadInt();

            Item item = new Item();
            item.ID = _itemID;
            item.Condition = _itemCondition;
            item.Quality = _itemQuality;

            if (DataUpdating.InventoryUpdateQueue.ContainsKey(_fromClient))
            {
                DataUpdating.InventoryUpdateQueue[_fromClient].Add(item);
            }
            else
            {
                DataUpdating.InventoryUpdateQueue.Add(_fromClient, new List<Item>{item});
            }
        }
    }
}