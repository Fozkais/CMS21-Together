using System.Collections.Generic;
using System.Net;
using CMS21MP.DataHandle;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide
{
    public class ClientHandle
    {
        public static void Welcome(Packet _packet)
        {
            string _msg = _packet.ReadString();
            int _myId = _packet.ReadInt();
            
            MelonLogger.Msg($"Message from Server:{_msg}");
            Client.instance.myId = _myId;
            
            ClientSend.WelcomeReceived();
            
            Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
            MainMod.isConnected = true;

            if (!MainMod.isHosting)
            {
                MainMod.localInventory.items.Clear();
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
           if (PlayerManager.players[_id] != null)
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
                PlayerManager.players[_id].transform.rotation = _rotation;
            }
            else
            {
                
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
            
            MelonLogger.Msg($"Received ItemFromServer, ID:{_itemID}, UID:{_itemUID}, Type:{status}");

            if (status)
            {
                if (!playerManagement.ItemsUID.Contains(_itemUID) )
                {
                    MelonLogger.Msg($"Adding Item with UID:{_itemUID}");
                    playerManagement.InventoryHandler.Add(_item);
                    playerManagement.ItemsUID.Add(_itemUID);
                    MainMod.localInventory.Add(_item);
                }
            }
            else
            {
                if (playerManagement.ItemsUID.Contains(_itemUID))
                {
                    MelonLogger.Msg($"Removing Item with UID:{_itemUID}");
                    playerManagement.InventoryHandler.Remove(_item);
                    playerManagement.ItemsUID.Remove(_itemUID);
                    MainMod.localInventory.Delete(_item);
                }
            }
            
        }

        public static void PlayerDisconnect(Packet _packet)
        {
            int _id = _packet.ReadInt();

            Object.Destroy(PlayerManager.players[_id].gameObject);
            PlayerManager.players.Remove(_id);
            DataUpdating.MovUpdateQueue.Remove(_id);
            DataUpdating.RotUpdateQueue.Remove(_id);

        }
    }
}