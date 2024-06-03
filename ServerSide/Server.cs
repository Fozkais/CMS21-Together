using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using CMS21Together.ServerSide.Data;
using CMS21Together.ClientSide;
using CMS21Together.ServerSide.Handle;
using CMS21Together.Shared;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ServerSide
{
     public class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, ServerClient> clients = new Dictionary<int, ServerClient>();

        public delegate void packetHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, DateTime> lastClientActivity = new Dictionary<int, DateTime>();
        public static Dictionary<int, packetHandler> packetHandlers;

        public static TcpListener tcpListener;
        private static UdpClient udpListener;
        private static bool isStopping;

        public static void Start()
        {
            MaxPlayers = MainMod.MAX_PLAYER;
            Port = MainMod.PORT;

            MelonLogger.Msg("Starting server...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            MelonLogger.Msg($"Server started successfully !");
            ServerData.isRunning = true;
            isStopping = false;
            
            Application.runInBackground = true;
            
            Client.Instance.ConnectToServer("127.0.0.1");

            MelonCoroutines.Start(CheckForInactiveClientsRoutine());
        }

        public static void CheckForInactiveClients()
        {
            // Délai maximum d'inactivité (en secondes)
            int maxInactivityDelay = 60;
            foreach (KeyValuePair<int, DateTime> entry in lastClientActivity)
            {
                int clientId = entry.Key;
                DateTime lastActivity = entry.Value;

                if ((DateTime.Now - lastActivity).TotalSeconds > maxInactivityDelay)
                {
                    // Le client est inactif depuis trop longtemps, le déconnecter
                    ServerSend.DisconnectClient(clientId, "Client Inactive for too long...");
                    clients.Remove(clientId);
                    lastClientActivity.Remove(clientId);
                }
            }
        }
        

        public static IEnumerator CheckForInactiveClientsRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(10); // Vérifier toutes les 10 secondes
                CheckForInactiveClients();
            }
        }

        public static void Stop()
        {
            if(!ServerData.isRunning) return;
            
            Application.runInBackground = false;
            foreach (int id in Server.clients.Keys)
            {
                ServerSend.DisconnectClient(id, "Server is shutting down.");
            }
            
            ServerData.ResetData();
            isStopping = true;

            if(udpListener != null)
                udpListener.Close();
            if(tcpListener != null)
                tcpListener.Stop();
            
            if(clients != null)
                clients.Clear();
            if(packetHandlers != null)
                packetHandlers.Clear();

            ModUI.Instance.window = guiWindow.main;
            MelonLogger.Msg("Server Closed.");

        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            if(isStopping)
                return;
            
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
           // MelonLogger.Msg($"Incoming connection from {_client.Client.RemoteEndPoint}...");

           foreach (int ClientID in clients.Keys)
           {
               if (clients[ClientID].tcp.socket == null)
               {
                   clients[ClientID].tcp.Connect(_client);
                   return;
               }
           }

            MelonLogger.Msg($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
        }
        
        private static void UDPReceiveCallback(IAsyncResult result)
        {
                if(isStopping)
                    return;
                try
                {
                    IPEndPoint receivedIP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] _data = udpListener.EndReceive(result, ref receivedIP);
                    udpListener.BeginReceive(UDPReceiveCallback, null);
                    
                    if (_data.Length < 4)
                        return;
                    

                    using (Packet _packet = new Packet(_data))
                    {
                        int _clientId = _packet.ReadInt();
                        if (_clientId == 0)
                            return;
                        
                        if (clients[_clientId].udp.endPoint == null)
                        {
                            clients[_clientId].udp.Connect(receivedIP);
                            return;
                        }
                        
                        
                        if (clients[_clientId].udp.endPoint.ToString() == receivedIP.ToString())
                        {
                            clients[_clientId].udp.HandleData(_packet);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Msg($"Error receiving UDP data: {ex}");
                }
            }

        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                if (_clientEndPoint != null)
                {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception _ex)
            {
                MelonLogger.Msg($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
            }
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new ServerClient(i));
            }

            packetHandlers = new Dictionary<int, packetHandler>()
            {
                {(int)PacketTypes.empty, ServerHandle.Empty},
                {(int)PacketTypes.welcome, ServerHandle.WelcomeReceived},
                {(int)PacketTypes.readyState, ServerHandle.ReadyState},
                {(int)PacketTypes.keepAlive, ServerHandle.keepAlive},
                {(int)PacketTypes.disconnect, ServerHandle.Disconnect},
                
                {(int)PacketTypes.playerPosition, ServerHandle.playerPosition},
                {(int)PacketTypes.playerInitialPos, ServerHandle.playerInitialPosition},
                {(int)PacketTypes.playerRotation, ServerHandle.playerRotation},
                {(int)PacketTypes.playerSceneChange, ServerHandle.playerSceneChange},
                {(int)PacketTypes.stats, ServerHandle.playerStats},
                {(int)PacketTypes.inventoryItem, ServerHandle.InventoryItem},
                {(int)PacketTypes.inventoryGroupItem, ServerHandle.InventoryGroupItem},
                
                {(int)PacketTypes.lifter, ServerHandle.Lifter},
                {(int)PacketTypes.tireChanger, ServerHandle.TireChanger},
                {(int)PacketTypes.wheelBalancer, ServerHandle.WheelBalancer},
                {(int)PacketTypes.engineStandAngle, ServerHandle.EngineStandAngle},
                {(int)PacketTypes.setEngineOnStand, ServerHandle.SetEngineOnStand},
                {(int)PacketTypes.setGroupEngineOnStand, ServerHandle.SetGroupEngineOnStand},
                {(int)PacketTypes.EngineStandResync, ServerHandle.ResyncEngineStand},
                {(int)PacketTypes.takeOffEngineFromStand, ServerHandle.TakeOffEngineFromStand},
                {(int)PacketTypes.engineCrane, ServerHandle.EngineCrane},
                {(int)PacketTypes.oilBin, ServerHandle.OilBin},
                {(int)PacketTypes.springClampGroup, ServerHandle.SpringClampGroup},
                {(int)PacketTypes.springClampClear, ServerHandle.SpringClampClear},
                {(int)PacketTypes.toolMove, ServerHandle.ToolsMove},
                
                {(int)PacketTypes.carResync, ServerHandle.CarResync},
                {(int)PacketTypes.carInfo, ServerHandle.CarInfo},
                {(int)PacketTypes.carPosition, ServerHandle.CarPosition},
                {(int)PacketTypes.carPart, ServerHandle.CarPart},
                {(int)PacketTypes.carParts, ServerHandle.CarParts},
                {(int)PacketTypes.bodyPart, ServerHandle.BodyPart},
                {(int)PacketTypes.bodyParts, ServerHandle.BodyParts},
            };
            MelonLogger.Msg("Initialized Packets!");
        }
    }
}