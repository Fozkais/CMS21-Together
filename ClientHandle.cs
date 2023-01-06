using System.Net;
using MelonLoader;
using UnityEngine;

namespace CMS21MP
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
        }
        
        public static void UDPTest(Packet _packet)
        {
            string _msg = _packet.ReadString();

            MelonLogger.Msg($"Received packet via UDP. Contains message: {_msg}");
            ClientSend.UDPTestReceived();
        }
        
        public static void SpawnPlayer(Packet _packet)
        {
            int _id = _packet.ReadInt();
            string _username = _packet.ReadString();
            Vector3 _position = _packet.ReadVector3();
            Quaternion _rotation = _packet.ReadQuaternion();

            GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
        }


        public static void PlayerPosition(Packet _packet)
        {
            int _id = _packet.ReadInt();
            Vector3 _position = _packet.ReadVector3();

            GameManager.players[_id].transform.position = _position;
        }
        public static void PlayerRotation(Packet _packet)
        {
            int _id = _packet.ReadInt();
            Quaternion _rotation = _packet.ReadQuaternion();

            GameManager.players[_id].transform.rotation = _rotation;
        }
    }
}