using System.Collections.Generic;
using System.Net;
using CMS21MP.ClientSide.Data;
using CMS21MP.CustomData;
using CMS21MP.ServerSide;
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
                ClientSend.KeepAlive();
            }
            
            public static void KeepAlive()
            {
                using (Packet _packet = new Packet((int)PacketTypes.keepAlive))
                {
                    SendTCPData(_packet);
                }
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

        #region Player info

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
            
            public static void SendSceneChange(string scene)
            {
                using (Packet _packet = new Packet((int)PacketTypes.playerSceneChange))
                {
                    _packet.Write(scene);

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
            
            public static void SendPartScripts(List<ModPartScript> tempCarScripts, int carCarLoaderID)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carParts))
                {
                    MelonLogger.Msg($"Sending: {tempCarScripts.Count} parts");
                    _packet.Write(tempCarScripts);
                    _packet.Write(carCarLoaderID);

                    SendTCPData(_packet);
                }
            }

            public static void SendBodyParts(List<ModCarPart> tempCarParts, int carCarLoaderID)
            {
                using (Packet _packet = new Packet((int)PacketTypes.bodyParts))
                {
                    MelonLogger.Msg($"Sending: {tempCarParts.Count} bodyParts");
                    _packet.Write(tempCarParts);
                    _packet.Write(carCarLoaderID);

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


        #region Inventory
            public static void SendInventoryItem(ModItem _Item, bool status, bool resync=false)
            {
                using (Packet _packet = new Packet((int)PacketTypes.inventoryItem))
                {
                    _packet.Write(_Item);
                    _packet.Write(status);
                    _packet.Write(resync);

                    SendTCPData(_packet);
                }
            }
            public static void SendInventoryGroupItem(ModGroupItem _Item, bool status, bool resync=false)
            {
                using (Packet _packet = new Packet((int)PacketTypes.inventoryGroupItem))
                {
                    _packet.Write(_Item);
                    _packet.Write(status);
                    _packet.Write(resync);

                    SendTCPData(_packet);
                }
            }
        

        #endregion

        #region Garage Interaction

            public static void SendLifterNewPos(int action, int pos,int _carLoaderID)
            {
                using (Packet _packet = new Packet((int)PacketTypes.lifterPos))
                {
                    _packet.Write(action);
                    _packet.Write(pos);
                    _packet.Write(_carLoaderID);

                    SendTCPData(_packet);
                }
            }
        

            public static void SendTireChange(ModGroupItem modGroupItem, bool instant, bool connect)
            {
                using (Packet _packet = new Packet((int)PacketTypes.tireChanger))
                {
                    _packet.Write(modGroupItem);
                    _packet.Write(instant);
                    _packet.Write(connect);

                    SendTCPData(_packet);
                }
            }

            public static void SendTireChange_ResetAction()
            {
                using (Packet _packet = new Packet((int)PacketTypes.tireChanger_ResetAction))
                {
                    SendTCPData(_packet);
                }
            }

            public static void SendWheelBalance(ModGroupItem modGroupItem)
            {
                using (Packet _packet = new Packet((int)PacketTypes.wheelBalancer))
                {
                    _packet.Write(modGroupItem);
                    
                    SendTCPData(_packet);
                }
            }
            public static void SendWheelBalance_ResetAction()
            {
                using (Packet _packet = new Packet((int)PacketTypes.wheelBalancer_ResetAction))
                {
                    SendTCPData(_packet);
                }
            }
            public static void SendUpdatedWheelFromBalancer(ModGroupItem modGroupItem)
            {
                using (Packet _packet = new Packet((int)PacketTypes.wheelBalancer_UpdateWheel))
                {
                    _packet.Write(modGroupItem);
                    
                    SendTCPData(_packet);
                }
            }
            
        #endregion
        
    }
    
}