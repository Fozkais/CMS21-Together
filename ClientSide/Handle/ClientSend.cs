using System.Collections.Generic;
using System.Net;
using CMS21Together.ClientSide.Data;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;

namespace CMS21Together.ClientSide.Handle
{
    public class ClientSend
    {
        private static void SendTCPData(Packet _packet)
        {
            _packet.WriteLength();
            Client.Instance.tcp.SendData(_packet);
        }
        
        private static void SendUDPData(Packet _packet)
        {
            _packet.WriteLength();
            Client.Instance.udp.SendData(_packet);
        }
        
        #region Lobby and connection
        
        public static void WelcomeReceived()
        {
            using (Packet _packet = new Packet((int)PacketTypes.welcome))
            {
                _packet.Write(Client.Instance.Id);
                _packet.Write(Client.Instance.username);
                _packet.Write(ContentManager.Instance.Contents);
                _packet.Write(MainMod.ASSEMBLY_MOD_VERSION);
                    
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


        #region PlayerData
        
            public static void SendInitialPosition(Vector3Serializable position)
            {
                using (Packet _packet = new Packet((int)PacketTypes.playerInitialPos))
                {
                    _packet.Write(position);
                        
                    SendUDPData(_packet);
                }
            }
            public static void SendPosition(Vector3Serializable position)
            {
                using (Packet _packet = new Packet((int)PacketTypes.playerPosition))
                {
                    _packet.Write(position);
                    
                    SendUDPData(_packet);
                }
            }
            public static void SendRotation(QuaternionSerializable rotation)
            {
                using (Packet _packet = new Packet((int)PacketTypes.playerRotation))
                {
                    _packet.Write(rotation);
                    
                    SendUDPData(_packet);
                }
            }
            
            public static void SendSceneChange(GameScene scene)
            {
                using (Packet _packet = new Packet((int)PacketTypes.playerSceneChange))
                {
                    _packet.Write(scene);

                    SendTCPData(_packet);
                }
            }
            public static void Stats(int diff, ModStats stats)
            {
                using (Packet _packet = new Packet((int)PacketTypes.stats))
                {
                    _packet.Write(diff);
                    _packet.Write(stats);

                    SendTCPData(_packet);
                }
            }
            
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

            public static void SendLifter(ModLifterState state, int carLoaderID)
            {
                using (Packet _packet = new Packet((int)PacketTypes.lifter))
                {
                    _packet.Write(state);
                    _packet.Write(carLoaderID);

                    SendTCPData(_packet);
                }
            }
            public static void TireChanger(ModGroupItem modGroupItem=null, bool instant=false, bool connect=false, bool resetAction=false)
            {
                using (Packet _packet = new Packet((int)PacketTypes.tireChanger))
                {
                    _packet.Write(modGroupItem);
                    _packet.Write(instant);
                    _packet.Write(connect);
                    _packet.Write(resetAction);

                    SendTCPData(_packet);
                }
            }
            public static void WheelBalancer(ModWheelBalancerActionType aType, ModGroupItem modGroupItem)
            {
                using (Packet _packet = new Packet((int)PacketTypes.wheelBalancer))
                {
                    _packet.Write(aType);
                    if(aType == ModWheelBalancerActionType.setGroup || aType == ModWheelBalancerActionType.start)
                        _packet.Write(modGroupItem);

                    SendTCPData(_packet);
                }
            }
            
            public static void SendEngineAngle(float newAngle)
            {
                using (Packet _packet = new Packet((int)PacketTypes.engineStandAngle))
                {
                    _packet.Write(newAngle);
                        
                    SendTCPData(_packet);
                }
            }
                
        #endregion

        #region CarData
            public static void SendModCar(ModCar modCar, bool removed=false)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carInfo))
                {
                    _packet.Write(removed);
                    _packet.Write(modCar);

                    SendTCPData(_packet);
                }
                //MelonLogger.Msg("Send car info to server");
            }
            public static void SendCarPart(int carCarLoaderID, ModPartScript modPartScript)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carPart))
                {
                    _packet.Write(modPartScript);
                    _packet.Write(carCarLoaderID);
                    

                    SendTCPData(_packet);
                }
            }
            public static void SendBodyPart(int carCarLoaderID, ModCarPart modCarPart)
            {
                using (Packet _packet = new Packet((int)PacketTypes.bodyPart))
                {
                    _packet.Write(modCarPart);
                    _packet.Write(carCarLoaderID);
                    

                    SendTCPData(_packet);
                }
            }
            public static void SendPartsScript(List<ModPartScript> otherPartsBuffer, int carCarLoaderID)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carParts))
                {
                    _packet.Write(otherPartsBuffer);
                    _packet.Write(carCarLoaderID);

                    SendTCPData(_packet);
                }
            }

            public static void SendBodyParts(List<ModCarPart> bodyPartsBuffer, int carCarLoaderID)
            {
                using (Packet _packet = new Packet((int)PacketTypes.bodyParts))
                {
                    _packet.Write(bodyPartsBuffer);
                    _packet.Write(carCarLoaderID);

                    SendTCPData(_packet);
                }
            }
            public static void SendCarPosition(int carLoaderID, int placeNo)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carPosition))
                {
                    _packet.Write(carLoaderID);
                    _packet.Write(placeNo);

                    SendTCPData(_packet);
                }
            }

        #endregion
        
    }
}