using System;
using System.Collections.Generic;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Car;
using CMS21Together.ClientSide.Data.PlayerData;
using CMS21Together.ClientSide.Handle;
using CMS21Together.ClientSide.Transport;
using CMS21Together.Shared;
using CMS21Together.Shared.Steam;
using MelonLoader;
using Steamworks;
using UnityEngine;
using UnityEngine.Serialization;

namespace CMS21Together.ClientSide
{
    [RegisterTypeInIl2Cpp]
    public class Client: MonoBehaviour
    {
        public static Client Instance;
        public static int dataBufferSize = 4096;

        public string username = "player";
        public string ip = "127.0.0.1";
        public int port;
        public int Id;
        public ClientTCP tcp;
        public ClientUDP udp;
        public ClientSteam steam;

        public NetworkType currentType = NetworkType.TcpUdp;
        public bool isConnected;

        public delegate void PacketHandler(Packet _packet);
        public static Dictionary<int, PacketHandler> PacketHandlers;
        

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                MelonLogger.Msg("Instance already exists, destroying object!");
                Destroy(this);
            }
            tcp = new ClientTCP();
            udp = new ClientUDP();
        }

        public void ClientOnApplicationQuit()
        {
           // ClientSend.Disconnect(Instance.Id); TODO:FIX
        }


        public void ConnectToServer(string _ipAdress, NetworkType type=NetworkType.none)
        {
            if (type == NetworkType.none) type = currentType;
            
            InitializeClientData();
            Instance.ip = _ipAdress;
            Instance.port = MainMod.PORT;

            ClientData.Instance = new ClientData();
            isConnected = true;

            currentType = type;
            
            if (type == NetworkType.TcpUdp)
            {
                MelonLogger.Msg("Connecting via TCP...");
                try
                {
                    tcp.Connect();
                }
                catch (Exception ex) // Capturer toutes les exceptions possibles
                {
                    MelonLogger.Msg($"Error detected! Failed to connect to server. Error: {ex}");
                    Client.Instance.Disconnect();
                }
            }
            else
            {
                MelonLogger.Msg("Connecting via Steam...");
                SteamId hostID = NetworkingUtils.ConvertCode(_ipAdress);
                steam = SteamNetworkingSockets.ConnectRelay<ClientSteam>(hostID);
            }

            if (!isConnected)
            {
                // Traiter les erreurs de connexion ici
                PacketHandlers.Clear();
            }
        }

        public void SendData(Packet _packet, bool reliable)
        {
            switch (currentType)
            {
                case NetworkType.TcpUdp:
                    if(reliable) tcp.SendData(_packet);
                    else udp.SendData(_packet);
                    break;
                case NetworkType.steamNetworking:
                    steam.SendData(_packet, reliable);
                    break;
            }
        }

        private void InitializeClientData()
        {
            PacketHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)PacketTypes.welcome, ClientHandle.Welcome },
                { (int)PacketTypes.contentInfo, ClientHandle.ContentsInfo },
                { (int)PacketTypes.keepAlive, ClientHandle.KeepAlive},
                { (int)PacketTypes.keepAliveConfirmed, ClientHandle.KeepAliveConfirmation},
                { (int)PacketTypes.disconnect, ClientHandle.Disconnect },
                { (int)PacketTypes.readyState, ClientHandle.ReadyState },
                { (int)PacketTypes.playerInfo, ClientHandle.PlayerInfo },
                { (int)PacketTypes.playersInfo, ClientHandle.PlayersInfo },
                { (int)PacketTypes.startGame, ClientHandle.StartGame },
                { (int)PacketTypes.spawnPlayer, ClientHandle.SpawnPlayer },
                
                { (int)PacketTypes.playerPosition, ClientHandle.playerPosition },
                { (int)PacketTypes.playerInitialPos, ClientHandle.playerInitialPos },
                { (int)PacketTypes.playerRotation, ClientHandle.playerRotation},
                { (int)PacketTypes.playerSceneChange, ClientHandle.playerSceneChange},
                { (int)PacketTypes.stats, ClientHandle.playerStats},
                { (int)PacketTypes.inventoryItem, ClientHandle.InventoryItem},
                { (int)PacketTypes.inventoryGroupItem, ClientHandle.InventoryGroupItem},
                
                { (int)PacketTypes.lifter, ClientHandle.Lifter},
                { (int)PacketTypes.tireChanger, ClientHandle.TireChange},
                { (int)PacketTypes.wheelBalancer, ClientHandle.WheelBalancer},
                { (int)PacketTypes.engineStandAngle, ClientHandle.EngineStandAngle},
                { (int)PacketTypes.takeOffEngineFromStand, ClientHandle.TakeOffEngineFromStand},
                { (int)PacketTypes.engineCrane, ClientHandle.EngineCrane},
                { (int)PacketTypes.setEngineOnStand, ClientHandle.setEngineOnStand},
                { (int)PacketTypes.setGroupEngineOnStand, ClientHandle.setGroupEngineOnStand},
                { (int)PacketTypes.oilBin, ClientHandle.OilBin},
                { (int)PacketTypes.springClampGroup, ClientHandle.SpringClampGroup},
                { (int)PacketTypes.springClampClear, ClientHandle.SpringClampClear},
                { (int)PacketTypes.toolMove, ClientHandle.ToolsMove},
                
                { (int)PacketTypes.carInfo, ClientHandle.CarInfo},
                //{ (int)PacketTypes.carLoadInfo, ClientHandle.CarLoadInfo},
                { (int)PacketTypes.carPosition, ClientHandle.CarPosition},
                { (int)PacketTypes.carPart, ClientHandle.CarPart},
                { (int)PacketTypes.bodyPart, ClientHandle.BodyPart},
                { (int)PacketTypes.carParts, ClientHandle.CarParts},
                { (int)PacketTypes.bodyParts, ClientHandle.BodyParts},
            };
            MelonLogger.Msg("Initialized Client Packets!");
        }
        
        public void Disconnect()
        {
            if (isConnected)
            {
                CarHarmonyPatches.ListenToDeleteCar = false;
                
                Application.runInBackground = false;
                isConnected = false;
                tcp.Disconnect();
                udp.Disconnect();
                if(tcp.socket != null)
                    tcp.socket.Close();
                if (udp.socket != null)
                    udp.socket.Close();

                ClientData.Instance.GameReady = false;
                ClientData.Instance = null;
                GameData.Instance = null;
                
                ModInventory.handledGroupItem.Clear();
                ModInventory.handledItem.Clear();

                ModUI.Instance.window = guiWindow.main;
                
                MelonLogger.Msg("CL : Disconnected from server.");
            }
            CarHarmonyPatches.ListenToDeleteCar = true;
            ApiCalls.API_M2(ContentManager.Instance.OwnedContents);
        }
    }
}