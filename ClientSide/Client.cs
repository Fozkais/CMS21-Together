using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using CMS21MP.ClientSide.Data;
using CMS21MP.ClientSide.DataHandle;
using CMS21MP.ClientSide.Transport;
using CMS21MP.SharedData;
using MelonLoader;


namespace CMS21MP.ClientSide
{
    public class Client : MonoBehaviour
    {
        public static Client Instance;
        public static int dataBufferSize = 4096;

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

        public void Initialize()
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

            threadManager = new ThreadManager();
        }

        public void ClientOnApplicationQuit()
        {
            ClientSend.Disconnect(Instance.Id);
        }


        public void ConnectToServer(string _ipAdress)
        {
            InitializeClientData();
            Instance.ip = _ipAdress;
            Instance.port = MainMod.PORT;


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
                { (int)PacketTypes.keepAlive, ClientHandle.KeepAlive},
                { (int)PacketTypes.keepAliveConfirmed, ClientHandle.KeepAliveConfirmation},
                { (int)PacketTypes.disconnect, ClientHandle.Disconnect },
                { (int)PacketTypes.readyState, ClientHandle.ReadyState },
                { (int)PacketTypes.playerInfo, ClientHandle.PlayersInfo },
                { (int)PacketTypes.startGame, ClientHandle.StartGame },
                { (int)PacketTypes.spawnPlayer, ClientHandle.SpawnPlayer },
                { (int)PacketTypes.playerPosition, ClientHandle.playerPosition },
                { (int)PacketTypes.playerRotation, ClientHandle.playerRotation },
                { (int)PacketTypes.playerSceneChange, ClientHandle.playerSceneChange },
                { (int)PacketTypes.carInfo, ClientHandle.CarInfo},
                { (int)PacketTypes.carPosition, ClientHandle.CarPosition},
                { (int)PacketTypes.carPart, ClientHandle.CarPart},
                { (int)PacketTypes.bodyPart, ClientHandle.BodyPart},
                { (int)PacketTypes.carParts, ClientHandle.PartScripts},
                { (int)PacketTypes.bodyParts, ClientHandle.BodyParts},
                { (int)PacketTypes.inventoryItem, ClientHandle.InventoryItem},
                { (int)PacketTypes.inventoryGroupItem, ClientHandle.InventoryGroupItem},
                { (int)PacketTypes.lifterPos, ClientHandle.LifterPos },
                { (int)PacketTypes.tireChanger, ClientHandle.TireChange },
                { (int)PacketTypes.tireChanger_ResetAction, ClientHandle.TireChanger_ResetAction },
                { (int)PacketTypes.wheelBalancer, ClientHandle.WheelBalancer },
                { (int)PacketTypes.wheelBalancer_ResetAction, ClientHandle.WheelBalancer_ResetAction }
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
                ClientData.needToKeepAlive = false;
                /*if(tcp.socket != null)
                    tcp.socket.Close();
                if (udp.socket != null)
                    udp.socket.Close();*/
                
                if(Client.PacketHandlers != null)
                    Client.PacketHandlers.Clear();
                GameData.DataInitialzed = false;
                
                GameData.carLoaders = null;
            
                if(ClientData.carOnScene != null)
                    ClientData.carOnScene.Clear();
                if(ClientData.serverPlayers != null)
                    ClientData.serverPlayers.Clear();
                if(ClientData.serverPlayerInstances != null)
                    ClientData.serverPlayerInstances.Clear();
                if(GameData.localInventory != null)
                    GameData.localInventory.DeleteAll();
                if(ClientData.playerGroupInventory != null)
                    ClientData.playerGroupInventory.Clear();
                if( ClientData.playerInventory != null)
                    ClientData.playerInventory.Clear();

                ClientData.asGameStarted = false;

                ModUI.Instance.ShowMainInterface();
                
                MelonLogger.Msg("CL : Disconnected from server.");
            }
        }
    }
}
