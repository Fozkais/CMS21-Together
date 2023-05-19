using System.Collections.Generic;
using System.Collections.ObjectModel;
using CMS21MP.ServerSide;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.DataHandle
{
    public class ServerSend
    {
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        private static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].udp.SendData(_packet);
        }

        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }

        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                    Server.clients[i].tcp.SendData(_packet);
            }
        }

        private static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }

        private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].udp.SendData(_packet);
                }
            }
        }

        public static void Welcome(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);
                _packet.Write(ServerData.serverMoney);

                SendTCPData(_toClient, _packet);
            }
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);
                _packet.Write(ServerData.serverMoney);

                SendUDPData(_toClient, _packet);
            }
        }

        public static void DLC(Dictionary<string, bool> dlcDifferences, int clientID)
        {
            using (Packet _packet = new Packet((int)ServerPackets.dlc))
            {
                _packet.Write(dlcDifferences);

                SendTCPData(clientID, _packet);
            }
        }
        public static void SpawnPlayer(int _toClient, Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.username);
                _packet.Write(_player.position);
                _packet.Write(_player.rotation);

                SendTCPDataToAll(_packet);
            }
        }

        public static void PlayerPosition(int clientId, Vector3 position)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
            {
                _packet.Write(clientId);
                _packet.Write(position);

                SendUDPDataToAll(clientId, _packet);
            }
        }

        public static void PlayerRotation(int clientId, Quaternion rotation)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
            {
                _packet.Write(clientId);
                _packet.Write(rotation);

                SendUDPDataToAll(clientId, _packet);
            }
        }

        public static void PlayerConnected(int _connectedPlayers, int _maxPlayers)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerConnected))
            {
                _packet.Write(_connectedPlayers);
                _packet.Write(_maxPlayers);

                SendTCPDataToAll(_packet);
            }
        }

        public static void PlayerDisconnect(int id)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerDisconnect))
            {
                _packet.Write(id);

                SendTCPDataToAll(_packet);
            }

        }

        public static void SendItem(int clientID,ModItem item, bool status, int _clientID)
        {
            using (Packet _packet = new Packet((int)ServerPackets.items))
            {
                _packet.Write(clientID);
                _packet.Write(item);
                _packet.Write(status);
                
               // MelonLogger.Msg($"SV : Sending item info! ID:{item.ID}, UID:{item.UID}, Type:{status}");
               if (_clientID == 0)
               {
                   SendTCPDataToAll(clientID,_packet);
               }
               else
               {
                   SendTCPData(_clientID, _packet);
               }
            }
        }
        public static void SendGroupItem(int clientID,ModItemGroup item, bool status, int _clientID)
        {
            using (Packet _packet = new Packet((int)ServerPackets.groupItems))
            {
                _packet.Write(clientID);
                _packet.Write(item);
                _packet.Write(status);
                
                // MelonLogger.Msg($"SV : Sending item info! ID:{item.ID}, UID:{item.UID}, Type:{status}");
                if (_clientID == 0)
                {
                    SendTCPDataToAll(clientID,_packet);
                }
                else
                {
                    SendTCPData(_clientID, _packet);
                }
            }
        }

        public static void Stats(int clientID, int stat, bool status, int type)
        {
            using (Packet _packet = new Packet((int)ServerPackets.stats))
            {
                _packet.Write(stat);
                _packet.Write(status);
                _packet.Write(type);
                
                SendTCPDataToAll(clientID, _packet);
            }
        }

        public static void PlayerScene(int clientID, string scene)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerScene))
            {
                _packet.Write(clientID);
                _packet.Write(scene);

                SendTCPDataToAll(clientID, _packet);
            }
        }

        public static void SpawnCars(int fromClient, carData data, int _clientID)
        {
            using (Packet _packet = new Packet((int)ServerPackets.spawnCars))
            {
                _packet.Write(data);

                if (_clientID == 0)
                {
                    SendTCPDataToAll(fromClient,_packet);
                }
                else
                {
                    SendTCPData(_clientID, _packet);
                }
            }
        }

        public static void MoveCar(int clientID, int carPosition, int carLoaderID)
        {
            using (Packet _packet = new Packet((int)ServerPackets.moveCars))
            {
                _packet.Write(carPosition);
                _packet.Write(carLoaderID);

                SendTCPDataToAll(clientID, _packet);
            }
            MelonLogger.Msg("Sended new car pos to all");
        }

        public static void carPart(int fromClient, PartScriptInfo part, int _clientID)
        {
            using (Packet _packet = new Packet((int)ServerPackets.car_part))
            {
                _packet.Write(part);

                if (_clientID == 0)
                {
                    SendTCPDataToAll(fromClient,_packet);
                }
                else
                {
                    SendTCPData(_clientID, _packet);
                }
            }
            //MelonLogger.Msg("SV: Sending new Part to players !");
        }

        public static void bodyPart(int fromClient, carPartsData bodyParts, int _clientID)
        {
            using (Packet _packet = new Packet((int)ServerPackets.body_part))
            {
                _packet.Write(bodyParts);

                if (_clientID == 0)
                {
                    SendTCPDataToAll(fromClient,_packet);
                }
                else
                {
                    SendTCPData(_clientID, _packet);
                }
            }
            //MelonLogger.Msg("SV: Sending new BodyPart to player !");
        }

        public static void LifterPos(int fromClient, CarLifterState lifterState, int carLoaderID)
        {
            using (Packet _packet = new Packet((int)ServerPackets.lifterState))
            {
                _packet.Write(lifterState);
                _packet.Write(carLoaderID);

                SendTCPDataToAll(fromClient, _packet);
            }
        }

        public static void AskData(int fromClient)
        {
            using (Packet _packet = new Packet((int)ServerPackets.askData))
            {
                _packet.Write(fromClient);

                SendTCPData(1, _packet);
            }
        }

        public static void VersionMismatch(int clientID, string assemblyModVersion)
        {
            using (Packet _packet = new Packet((int)ServerPackets.versionMismatch))
            {
                _packet.Write(assemblyModVersion);

                SendTCPData(clientID, _packet);
            }
        }
    }
}
