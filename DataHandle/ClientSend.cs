using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CMS21MP.ClientSide;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.DataHandle
{
    public static class ClientSend
    {
        private static void SendTCPData(Packet _packet)
        {
            _packet.WriteLength();
            Client.instance.tcp.SendData(_packet);
        }
        
        private static void SendUDPData(Packet _packet)
        {
            _packet.WriteLength();
            Client.instance.udp.SendData(_packet);
        }
        
        public static void WelcomeReceived()
        {
            using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
            {
                _packet.Write(Client.instance.myId);
                _packet.Write("Client received welcome.");
                _packet.Write(ModGUI.instance.usernameField);
                _packet.Write(MainMod.DLC.hasDLC);
                
                
                SendTCPData(_packet);
            }
            Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
        }

        public static void PlayerMovement(Vector3 _position)
        {
            using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
            {
                _packet.Write(Client.instance.myId);
                _packet.Write(_position);

                // MelonLogger.Msg($"Sending playerPos to server!");
                SendUDPData(_packet);
            }
        }

        public static void PlayerRotation(Quaternion _rotation)
        {
            using (Packet _packet = new Packet((int)ClientPackets.playerRotation))
            {
                _packet.Write(Client.instance.myId);
                _packet.Write(_rotation);

                // MelonLogger.Msg($"Sending playerPos to server!");
                SendUDPData(_packet);
            }
        }

        public static void SendItem(ModItem _item, bool status)
        {
            using (Packet _packet = new Packet((int)ClientPackets.items))
            {
                _packet.Write(Client.instance.myId);
                _packet.Write(_item);
                _packet.Write(status);

                SendTCPData(_packet);
            }
        }

        public static void SendGroupItem(ModItemGroup item, bool status)
        {
            using (Packet _packet = new Packet((int)ClientPackets.groupItems))
            {
                _packet.Write(Client.instance.myId);
                _packet.Write(item);
                _packet.Write(status);

                SendTCPData(_packet);
            }
        }

        public static void PlayerMoney(int _money, bool status)
        {
            using (Packet _packet = new Packet((int)ClientPackets.playerMoney))
            {
                _packet.Write(Client.instance.myId);
                _packet.Write(_money);
                _packet.Write(status);
                
                SendTCPData(_packet);
            }
        }

        public static void PlayerScene(string scene)
        {
            using (Packet _packet = new Packet((int)ClientPackets.playerScene))
            {
                _packet.Write(Client.instance.myId);
                _packet.Write(PlayerManager.players[Client.instance.myId].username);
                _packet.Write(scene);
                
                SendTCPData(_packet);
            }
        }

        public static void SpawnCars(carData data)
        {
            using(Packet _packet = new Packet((int)ClientPackets.spawnCars))
            {
                _packet.Write(Client.instance.myId);
                _packet.Write(data);

                SendTCPData(_packet);
            }
           // MelonLogger.Msg($"CL: Sending car Info! ID:[{data.carID}], LoaderID:[{data.carLoaderID}], Pos:[{data.carPosition}], status:[{data.status}]");
        }

        public static void MoveCar(int carPosition, int carLoaderID)
        {
            using(Packet _packet = new Packet((int)ClientPackets.moveCars))
            {
                _packet.Write(Client.instance.myId);
                _packet.Write(carPosition);
                _packet.Write(carLoaderID);

                SendTCPData(_packet);
            }
            MelonLogger.Msg("CL: Detected a moved car! sending new pos to server.");
        }
        public static void carParts(PartScriptInfo part)
        {
            using(Packet _packet = new Packet((int)ClientPackets.car_part))
                {
                    _packet.Write(Client.instance.myId);
                    _packet.Write(false);
                    _packet.Write(part);

                    SendTCPData(_packet);
                }
            //MelonLogger.Msg("CL: sending Part to Server!");
        }
        public static void bodyParts(carPartsData parts)
        {
            using(Packet _packet = new Packet((int)ClientPackets.body_part))
            {
                _packet.Write(Client.instance.myId);
                _packet.Write(parts);

                SendTCPData(_packet);
            }
            //MelonLogger.Msg($"CL: sending BodyPart:{parts.carPartID},  to Server!");
        }

        public static void LifterPos(CarLifterState lifterState, int carLoaderID)
        {
            using(Packet _packet = new Packet((int)ClientPackets.lifterState))
            {
                _packet.Write(Client.instance.myId);
                _packet.Write(lifterState);
                _packet.Write(carLoaderID);

                SendTCPData(_packet);
                MelonLogger.Msg($"Sending new Pos to Server : {lifterState.ToString()}");
            }
        }
    }
    
}