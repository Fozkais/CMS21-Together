using System;
using System.Collections.Generic;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Handle;
using CMS21Together.ClientSide.Transport;
using CMS21Together.Shared;
using MelonLoader;
using UnityEngine;

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

        public bool isConnected;
        public static bool forceDisconnected;

        public delegate void PacketHandler(Packet _packet);

        public static Dictionary<int, PacketHandler> PacketHandlers;

        public ThreadManager threadManager;

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


        public void ConnectToServer(string _ipAdress)
        {
            InitializeClientData();
            Instance.ip = _ipAdress;
            Instance.port = MainMod.PORT;

            ClientData data = new ClientData();
            ClientData.Instance = data;

            try
            {
                isConnected = true;
                tcp.Connect();
                //udp.Connect(((IPEndPoint)tcp.socket.Client.LocalEndPoint).Port);
            }
            catch (Exception ex) // Capturer toutes les exceptions possibles
            {
                MelonLogger.Msg($"Error detected! Failed to connect to server. Error: {ex}");
                Client.Instance.Disconnect();
            }

            if (!isConnected)
            {
                // Traiter les erreurs de connexion ici
                PacketHandlers.Clear();
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
                
                { (int)PacketTypes.carInfo, ClientHandle.CarInfo},
                //{ (int)PacketTypes.carLoadInfo, ClientHandle.CarLoadInfo},
                { (int)PacketTypes.carPosition, ClientHandle.CarPosition},
                { (int)PacketTypes.carPart, ClientHandle.CarPart},
                { (int)PacketTypes.bodyPart, ClientHandle.BodyPart},
                { (int)PacketTypes.carParts, ClientHandle.CarParts},
                { (int)PacketTypes.bodyParts, ClientHandle.BodyParts},
            };
            MelonLogger.Msg("Initialized Packets!");
        }
        
        public void Disconnect()
        {
            if (isConnected)
            {
                Application.runInBackground = false;
                isConnected = false;
                tcp.Disconnect();
                udp.Disconnect();
                if(tcp.socket != null)
                    tcp.socket.Close();
                if (udp.socket != null)
                    udp.socket.Close();


                ClientData.Instance = null;
                GameData.Instance = null;

                ModUI.Instance.window = guiWindow.main;
                
                MelonLogger.Msg("CL : Disconnected from server.");
            }
            ApiCalls.CallAPIMethod2(ContentManager.Instance.OwnedContents);
        }
    }
}