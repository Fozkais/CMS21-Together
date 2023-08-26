using System.Net;
using CMS21MP.ClientSide.Data;
using CMS21MP.CustomData;
using CMS21MP.SharedData;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide.DataHandle
{
    public static class ClientSend
    {
        private static void SendTCPData(Packet _packet)
        {
            _packet.WriteLength();
         //   if(MainMod.usingSteamAPI)
             //   PacketHandling.SendPacket(_packet, CallbackHandler.lobbyID);
          //  else
                Client.Instance.tcp.SendData(_packet);
        }
        
        private static void SendUDPData(Packet _packet)
        {
            _packet.WriteLength();
            //if(MainMod.usingSteamAPI)
           //     PacketHandling.SendPacket(_packet, CallbackHandler.lobbyID);
          //  else
                Client.Instance.udp.SendData(_packet);
        }
        
        #region Lobby and connection
        
            public static void WelcomeReceived()
            {
                using (Packet _packet = new Packet((int)PacketTypes.welcome))
                {
                    _packet.Write(Client.Instance.Id);
                    _packet.Write(ModUI.Instance.username);
                    
                    SendTCPData(_packet);
                }
                Client.Instance.udp.Connect(((IPEndPoint)Client.Instance.tcp.socket.Client.LocalEndPoint).Port);
            }

            public static void SendReadyState(bool b, int number)
            {
                using (Packet _packet = new Packet((int)PacketTypes.readyState))
                {
                    _packet.Write(b);
                    _packet.Write(number);
                    
                    SendTCPData(_packet);
                }
            }
            
            public static void Disconnect(int id)
            {
                using (Packet _packet = new Packet((int)PacketTypes.disconnect))
                {
                    _packet.Write(id);
                    
                    SendTCPData(_packet);
                }
            }
        #endregion

        #region Movement and Rotation

        public static void SendPosition(Vector3Serializable position)
        {
            using (Packet _packet = new Packet((int)PacketTypes.playerPosition))
            {
                _packet.Write(position);
                
                SendTCPData(_packet);
            }
          //  MelonLogger.Msg("Send position to server");
        }
        
        public static void SendRotation(QuaternionSerializable rotation)
        {
            using (Packet _packet = new Packet((int)PacketTypes.playerRotation))
            {
                _packet.Write(rotation);
                
                SendTCPData(_packet);
            }
        }


        #endregion

        #region Car

        public static void SendCarInfo(ModCar _car)
        {
            using (Packet _packet = new Packet((int)PacketTypes.carInfo))
            {
                _packet.Write(_car);

                SendTCPData(_packet);
            }
            MelonLogger.Msg("Send car info to server");
        }
        public static void SendCarPosition(int _carLoaderID, int _placeNo)
        {
            using (Packet _packet = new Packet((int)PacketTypes.carPosition))
            {
                _packet.Write(_carLoaderID);
                _packet.Write(_placeNo);

                SendTCPData(_packet);
            }
            MelonLogger.Msg("Send car Position to server");
        }

        #endregion

        public static void SendCarPart(int _carLoaderID, ModPartScript _partConverted)
        {
            using (Packet _packet = new Packet((int)PacketTypes.carPart))
            {
                _packet.Write(_carLoaderID);
                _packet.Write(_partConverted);

                SendTCPData(_packet);
            }
        }

        public static void SendPartSize(int engine, int suspension, int other, int carLoaderID)
        {
            using (Packet _packet = new Packet((int)PacketTypes.carPartSize))
            {
                _packet.Write(carLoaderID);
                _packet.Write(engine);
                _packet.Write(suspension);
                _packet.Write(other);

                SendTCPData(_packet);
            }
        }
    }
    
}