using System;
using System.Linq;
using System.Text;
using CMS21MP.ClientSide.DataHandle;
using CMS21MP.SharedData;
using Il2Cpp;
using MelonLoader;
using Steamworks;
using UnityEngine;

namespace CMS21MP
{
    public static class PacketHandling
    {
        public static void SendPacket(Packet packet, SteamId sendToUser)
        {
            byte[] data = packet.ToArray();
            //bool ret = SteamMatchmaking.SendLobbyChatMsg(lobbyID, data, data.Length);
            
           // byte[] data = "Hello World!".Select(Convert.ToByte).ToArray();
            
            var sent = SteamNetworking.SendP2PPacket(sendToUser, data);
            if (!sent)
            {
                var sent2 = SteamNetworking.SendP2PPacket(sendToUser, data);
                if (!sent2)
                {
                    MelonLogger.Msg("Failed to send packet!");
                }
                MelonLogger.Msg("Sended packet2 :" + sent2);
            }
            MelonLogger.Msg("Sended packet1 :" + sent);
        }

        public static void HandlePacket(byte[] data)
        {
            MelonLogger.Msg("Received packet");
            Packet packet = new Packet(data);
            int packetID = packet.ReadInt();

            switch (packetID)
            {
                case (int)PacketTypes.welcome:
                    ClientHandle.Welcome(packet);
                    break;
                case (int)PacketTypes.disconnect:
                    ClientHandle.Disconnect(packet);
                    break;
                case (int)PacketTypes.playerInfo:
                    ClientHandle.PlayersInfo(packet);
                    break;
                case (int)PacketTypes.playerPosition:
                    ClientHandle.playerPosition(packet);
                    break;
                case (int)PacketTypes.readyState:
                    ClientHandle.ReadyState(packet);
                    break;
                case (int)PacketTypes.playerRotation:
                    ClientHandle.playerRotation(packet);
                    break;
                case (int)PacketTypes.spawnPlayer:
                    ClientHandle.SpawnPlayer(packet);
                    break;
                case (int)PacketTypes.startGame:
                    ClientHandle.StartGame(packet);
                    break;
            }
        }

        public static Friend[] GetLobbyMemberID(SteamId lobbyID)
        {
            int numMembers = SteamData.lobby.MemberCount;
            
            Friend[] members = new Friend[numMembers];
            
            for (int i = 0; i < numMembers; i++)
            {
                members[i] = SteamData.lobby.Members.ElementAt(i);
            }

            return members;
        }
    }
}