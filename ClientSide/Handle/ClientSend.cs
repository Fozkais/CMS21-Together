using System.Collections.Generic;
using System.IO;
using System.Net;
using CMS21Together.ClientSide.Data;
using CMS21Together.ServerSide;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Steam;
using Il2Cpp;
using MelonLoader;
using Steamworks.Data;

namespace CMS21Together.ClientSide.Handle
{
    public static class ClientSend
    {
        private static void SendData(Packet _packet,bool reliable = true)
        {
            _packet.WriteLength();
            Client.Instance.SendData(_packet, reliable);
        }
        
        #region Lobby and connection
        
        public static void WelcomeReceived()
        {
            using (Packet _packet = new Packet((int)PacketTypes.welcome))
            {
                _packet.Write(Client.Instance.Id);
                _packet.Write(Client.Instance.username);
                _packet.Write(ContentManager.Instance.OwnedContents);
                _packet.Write(MainMod.ASSEMBLY_MOD_VERSION);
                _packet.Write(ContentManager.Instance.gameVersion);
                    
                SendData(_packet);
            }
            if(Client.Instance.currentType == NetworkType.TcpUdp)
                Client.Instance.udp.Connect(((IPEndPoint)Client.Instance.tcp.socket.Client.LocalEndPoint).Port);
            
            ClientSend.KeepAlive();
        }
            
        public static void KeepAlive()
        {
            using (Packet _packet = new Packet((int)PacketTypes.keepAlive))
            {
                SendData(_packet);
            }
        }

        public static void SendReadyState(bool b, int number)
        {
            using (Packet _packet = new Packet((int)PacketTypes.readyState))
            {
                _packet.Write(b);
                _packet.Write(number);
                    
                SendData(_packet);
            }
        }
            
        public static void Disconnect(int id)
        {
            using (Packet _packet = new Packet((int)PacketTypes.disconnect))
            {
                _packet.Write(id);
                    
                SendData(_packet);
            }
        }
        #endregion


        #region PlayerData
        
            public static void SendInitialPosition(Vector3Serializable position)
            {
                using (Packet _packet = new Packet((int)PacketTypes.playerInitialPos))
                {
                    _packet.Write(position);
                        
                    SendData(_packet);
                }
            }
            public static void SendPosition(Vector3Serializable position)
            {
                using (Packet _packet = new Packet((int)PacketTypes.playerPosition))
                {
                    _packet.Write(position);
                    
                    SendData(_packet);
                }
            }
            public static void SendRotation(QuaternionSerializable rotation)
            {
                using (Packet _packet = new Packet((int)PacketTypes.playerRotation))
                {
                    _packet.Write(rotation);
                    
                    SendData(_packet);
                }
            }
            
            public static void SendSceneChange(GameScene scene)
            {
                using (Packet _packet = new Packet((int)PacketTypes.playerSceneChange))
                {
                    _packet.Write(scene);

                    SendData(_packet);
                }
            }
            public static void Stats(int diff, ModStats stats)
            {
                using (Packet _packet = new Packet((int)PacketTypes.stats))
                {
                    _packet.Write(diff);
                    _packet.Write(stats);

                    SendData(_packet);
                }
            }
            
            public static void SendInventoryItem(ModItem _Item, bool status, bool resync=false)
            {
                using (Packet _packet = new Packet((int)PacketTypes.inventoryItem))
                {
                    _packet.Write(resync);
                    if (!resync)
                    {
                        _packet.Write(_Item);
                        _packet.Write(status);
                    }
                    
                    SendData(_packet);
                }
            }
            public static void SendInventoryGroupItem(ModGroupItem _Item, bool status, bool resync=false)
            {
                using (Packet _packet = new Packet((int)PacketTypes.inventoryGroupItem))
                {
                    _packet.Write(resync);
                    if (!resync)
                    {
                        _packet.Write(_Item);
                        _packet.Write(status);
                    }

                    SendData(_packet);
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

                    SendData(_packet);
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

                    SendData(_packet);
                }
            }
            public static void WheelBalancer(int type, GroupItem groupItem)
            {
                using (Packet _packet = new Packet((int)PacketTypes.wheelBalancer))
                {
                    var aType = (ModWheelBalancerActionType)type;
                    _packet.Write(aType);
                    if(aType == ModWheelBalancerActionType.setGroup || aType == ModWheelBalancerActionType.start)
                        _packet.Write(new ModGroupItem(groupItem));
                    SendData(_packet);
                }
            }
            
            public static void SendEngineAngle(float newAngle)
            {
                using (Packet _packet = new Packet((int)PacketTypes.engineStandAngle))
                {
                    _packet.Write(newAngle);
                        
                    SendData(_packet);
                }
                MelonLogger.Msg("Sent EngineAngle");
            }
            
            public static void SendSetEngineOnStand(ModItem modItem)
            {
                using (Packet _packet = new Packet((int)PacketTypes.setEngineOnStand))
                {
                    _packet.Write(modItem);
                        
                    SendData(_packet);
                }
                MelonLogger.Msg("Sent Engine");
            }
            
            public static void SendSetGroupEngineOnStand(ModGroupItem modItem, Vector3Serializable position, QuaternionSerializable rotation)
            {
                using (Packet _packet = new Packet((int)PacketTypes.setGroupEngineOnStand))
                {
                    _packet.Write(modItem);
                    _packet.Write(position);
                    _packet.Write(rotation);
                        
                    SendData(_packet);
                }
                MelonLogger.Msg("Sent EngineGroup");
            }
            
            public static void SendEngineStandResync()
            {
                using (Packet _packet = new Packet((int)PacketTypes.EngineStandResync))
                {
                    SendData(_packet);
                }
                MelonLogger.Msg("Sent Engine Resync");
            }
            
            public static void SendEngineTakeOffFromStand()
            {
                using (Packet _packet = new Packet((int)PacketTypes.takeOffEngineFromStand))
                {
                    SendData(_packet);
                }
                MelonLogger.Msg("Sent Engine take off");
            }
            
            public static void EngineCraneHandle(int carLoaderId,ModGroupItem modGroupItem=null)
            {
                using (Packet _packet = new Packet((int)PacketTypes.engineCrane))
                {
                    if (carLoaderId == -1)
                    {
                        _packet.Write(false);
                        _packet.Write(modGroupItem);
                    }
                    else
                    {
                        _packet.Write(true);
                        _packet.Write(carLoaderId);
                    }
                        
                    SendData(_packet);
                    MelonLogger.Msg("Sent EngineCrane");
                }
            }
            
            public static void SendGroupOnSpringClamp(ModGroupItem item, bool instant, bool mount)
            {
                using (Packet _packet = new Packet((int)PacketTypes.springClampGroup))
                {
                    _packet.Write(item);
                    _packet.Write(instant);
                    _packet.Write(mount);
                        
                    SendData(_packet);
                    MelonLogger.Msg("Sent SpringClamp");
                }
            }
            
            public static void SendClearSpring()
            {
                using (Packet _packet = new Packet((int)PacketTypes.springClampClear))
                {
                    SendData(_packet);
                    MelonLogger.Msg("Sent ClearSpringClamp");
                }
            }
            
            public static void SendOilBin(int carLoaderID)
            {
                using (Packet _packet = new Packet((int)PacketTypes.oilBin))
                {
                    _packet.Write(carLoaderID);
                    
                    SendData(_packet);
                    MelonLogger.Msg("Sent OilBin");
                }
            }
            
            public static void SendToolPosition(IOSpecialType tool, ModCarPlace place, bool playSound=false)
            {
                using (Packet _packet = new Packet((int)PacketTypes.toolMove))
                {
                    _packet.Write((ModIOSpecialType)tool);
                    _packet.Write(place);
                    _packet.Write(playSound);
                        
                    SendData(_packet);
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

                    SendData(_packet);
                }
                //MelonLogger.Msg("Send car info to server");
            }
            public static void SendCarPart(int carCarLoaderID, ModPartScript modPartScript)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carPart))
                {
                    _packet.Write(modPartScript);
                    _packet.Write(carCarLoaderID);
                    

                    SendData(_packet);
                }
            }
            public static void SendBodyPart(int carCarLoaderID, ModCarPart modCarPart)
            {
                using (Packet _packet = new Packet((int)PacketTypes.bodyPart))
                {
                    _packet.Write(modCarPart);
                    _packet.Write(carCarLoaderID);
                    

                    SendData(_packet);
                }
            }
            public static void SendPartsScript(List<ModPartScript> otherPartsBuffer, int carCarLoaderID, ModPartType modPartType)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carParts))
                {
                    _packet.Write(otherPartsBuffer);
                    _packet.Write(carCarLoaderID);
                    _packet.Write(modPartType);

                    SendData(_packet);
                }
                MelonLogger.Msg($"Sent part : {modPartType} , {otherPartsBuffer.Count}");
            }

            public static void SendBodyParts(List<ModCarPart> bodyPartsBuffer, int carCarLoaderID)
            {
                using (Packet _packet = new Packet((int)PacketTypes.bodyParts))
                {
                    _packet.Write(bodyPartsBuffer);
                    _packet.Write(carCarLoaderID);

                    SendData(_packet);
                }
                MelonLogger.Msg($"Sent bodyParts : {bodyPartsBuffer.Count}");
            }
            public static void SendCarPosition(int carLoaderID, int placeNo)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carPosition))
                {
                    _packet.Write(carLoaderID);
                    _packet.Write(placeNo);

                    SendData(_packet);
                }
            }
            
            public static void SendResyncCars(List<(int,string)> carToResync = null)
            {
                using (Packet _packet = new Packet((int)PacketTypes.carResync))
                {
                    if(carToResync == null)
                        _packet.Write(true);
                    else
                    {
                        _packet.Write(false);
                        _packet.Write(carToResync);
                    }
                    
                    SendData(_packet);
                }
            }
        

        #endregion



    }
}