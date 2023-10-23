using System.Net;
using CMS21MP.CustomData;
using CMS21MP.SharedData;
using MelonLoader;

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
            public static void SendCarPart(int _carLoaderID, ModPartScript _partConverted)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carPart))
                {
                    _packet.Write(_carLoaderID);
                    _packet.Write(_partConverted);

                    SendTCPData(_packet);
                }
            }

            public static void SendBodyPart(int _carLoaderID, ModCarPart _partConverted)
            {
                using (Packet _packet = new Packet((int)PacketTypes.bodyPart))
                {
                    _packet.Write(_carLoaderID);
                    _packet.Write(_partConverted);

                    SendTCPData(_packet);
                }
            }

        #endregion


        public static void SendInventoryItem(ModItem _Item, bool status)
        {
            using (Packet _packet = new Packet((int)PacketTypes.inventoryItem))
            {
                _packet.Write(_Item);
                _packet.Write(status);

                SendTCPData(_packet);
            }
        }
        public static void SendInventoryGroupItem(ModGroupItem _Item, bool status)
        {
            using (Packet _packet = new Packet((int)PacketTypes.inventoryGroupItem))
            {
                _packet.Write(_Item);
                _packet.Write(status);

                SendTCPData(_packet);
            }
        }
    }
    
}