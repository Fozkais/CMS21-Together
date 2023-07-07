using System.Collections.Generic;
using CMS21MP.ClientSide.Data;
using CMS21MP.ServerSide;
using CMS21MP.SharedData;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide.DataHandle
{
    public class ClientHandle : MonoBehaviour
    {
        public static void Welcome(Packet _packet)
        {
            string _msg = _packet.ReadString();
            int _myId = _packet.ReadInt();

            MelonLogger.Msg($"Message from Server:{_msg}");
            Client.Instance.Id = _myId;

            ClientSend.WelcomeReceived();
        }

        public static void Disconnect(Packet _packet)
        {
            string _msg = _packet.ReadString();
            
            MelonLogger.Msg($"Message from Server:{_msg}");
            Client.Instance.Disconnect();
        }

        public static void ReadyState(Packet _packet)
        {
            bool _ready = _packet.ReadBool();
            int _id = _packet.ReadInt();

            ClientData.serverPlayers[_id].isReady = _ready;
        }
        
        public static void PlayersInfo(Packet _packet)
        {
            Player info = _packet.Read<Player>();

            if(ClientData.serverPlayers.ContainsKey(info.id))
                ClientData.serverPlayers[info.id] = info;
            else
                ClientData.serverPlayers.Add(info.id, info);
            
            MelonLogger.Msg($"Received {info.username} info from server.");
        }
    }
}