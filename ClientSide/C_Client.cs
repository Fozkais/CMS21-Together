using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using CMS21MP.DataHandle;
using CMS21MP.ServerSide;
using MelonLoader;


namespace CMS21MP.ClientSide
{
    public class Client : MonoBehaviour
    {
        public static Client instance;
        public static int dataBufferSize = 4096;

        public string ip = "127.0.0.1";
        public int port = 7777;
        public int myId = 0;
        public TCP tcp;
        public UDP udp;

        public bool isConnected;
        public static bool forceDisconnected;

        public delegate void PacketHandler(Packet _packet);

        public static Dictionary<int, PacketHandler> packetHandlers;

        public ThreadManager threadManager;

        public void Initialize()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                MelonLogger.Msg("Instance already exists, destroying object!");
                Destroy(this);
            }
            tcp = new TCP();
            udp = new UDP();
            
            threadManager = new ThreadManager();
        }

        public void ClientOnApplicationQuit()
        {
            Disconnect();
        }


        public void ConnectToServer()
        {
            InitializeClientData();

            isConnected = true;

            try
            {
                tcp.Connect();
            }
            catch (Exception ex) // Capturer toutes les exceptions possibles
            {
                MelonLogger.Msg($"Error detected! Failed to connect to server. Error: {ex}");
                isConnected = false; // Marquer la connexion comme échouée
            }

            if (!isConnected)
            {
                // Traiter les erreurs de connexion ici
                packetHandlers.Clear();
            }
        }

        public class TCP
        {
            public TcpClient socket = new TcpClient();

            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public void Connect()
            {
                try
                {
                    socket = new TcpClient
                    {
                        ReceiveBufferSize = dataBufferSize,
                        SendBufferSize = dataBufferSize
                    };

                    receiveBuffer = new byte[dataBufferSize];

                    socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
                }
                catch (Exception ex) // Capturer toutes les exceptions possibles
                {
                    MelonLogger.Msg($"Error detected [connect] ! Failed to connect to server. Error: {ex}");
                }
            }

            private void ConnectCallback(IAsyncResult _result)
            {
                try
                {
                    socket.EndConnect(_result);

                    if (!socket.Connected)
                    {
                        MelonLogger.Msg("Cannot Connect to Server!");
                        return;
                    }

                    stream = socket.GetStream();
                    receivedData = new Packet();

                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch // Capturer toutes les exceptions possibles
                {
                    MelonLogger.Msg($"An error occurred while connecting to the server, check your internet or the ip of the server.");
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0)
                    {
                        instance.Disconnect();
                        return;
                    }

                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    receivedData.Reset(HandleData(_data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch
                {
                    //MelonLogger.Msg($"Error caused Disconnection! : {e}");
                    Disconnect();
                }
            }

            private bool HandleData(byte[] _data)
            {
                int _packetLenght = 0;

                receivedData.SetBytes(_data);
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLenght = receivedData.ReadInt();
                    if (_packetLenght <= 0)
                    {
                        return true;
                    }
                }

                while (_packetLenght > 0 && _packetLenght <= receivedData.UnreadLength())
                {
                    byte[] _packetBytes = receivedData.ReadBytes(_packetLenght);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetId = _packet.ReadInt();
                            packetHandlers[_packetId](_packet);
                        }
                    });
                    _packetLenght = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        _packetLenght = receivedData.ReadInt();
                        if (_packetLenght <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (_packetLenght <= 1)
                {
                    return true;
                }

                return false;
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0,
                            _packet.Length(), null, null);

                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Msg($"Error sending data to server via TCP: {e}");
                    throw;
                }
            }

            private void Disconnect()
            {
                instance.Disconnect();

                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        public class UDP
            {
                public UdpClient socket;
                public IPEndPoint endPoint;

                public UDP()
                {
                    endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
                }
                public void Connect(int localPort)
                {
                    socket = new UdpClient(localPort);
                    try
                    {
                        socket.Connect(endPoint);
                    }
                    catch
                    {
                        endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
                        socket.Connect(endPoint);
                    }
                    socket.BeginReceive(ReceiveCallback, null);

                    using(Packet packet = new Packet())
                    {
                        SendData(packet);
                    }
                }

                public void SendData(Packet _packet)
                {
                    try
                    {
                        _packet.InsertInt(instance.myId);
                        if (socket != null)
                        {
                            socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                        }
                    }
                    catch (Exception _ex)
                    {
                        MelonLogger.Msg($"Error sending data to server via UDP: {_ex}");
                    }
                }

                private void ReceiveCallback(IAsyncResult _result)
                {
                    try
                    {
                        byte[] _data = socket.EndReceive(_result, ref endPoint);
                        socket.BeginReceive(ReceiveCallback, null);

                        if (_data.Length < 4)
                        {
                            MelonLogger.Msg("UDP connect error , forced to disconnect");
                            instance.Disconnect();
                            return;
                        }

                        HandleData(_data);
                    }
                    catch 
                    {
                        if (!forceDisconnected)
                        {
                            Disconnect();
                            MelonLogger.Msg($"[UDP ReceiveCallback] Error while connecting, retrying...");
                            Client.instance.ConnectToServer();
                        }
                        else
                        {
                            Disconnect();
                            forceDisconnected = false;
                        }
                    }
                }

                private void HandleData(byte[] _data)
                {
                    using (Packet _packet = new Packet(_data))
                    {
                        int _packetLength = _packet.ReadInt();
                        _data = _packet.ReadBytes(_packetLength);
                    }

                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_data))
                        {
                            int _packetId = _packet.ReadInt();

                            if (packetHandlers.TryGetValue(_packetId, out var handler))
                            {
                                handler(_packet);
                            }
                            else
                            {
                                MelonLogger.Msg($"Packet with id {_packetId} not found in packetHandlers dictionary.");
                            }
                        }
                    });
                }

                private void Disconnect()
                {
                    instance.Disconnect();
                    endPoint = null;
                    socket = null;
                }
            }

        private void InitializeClientData()
        {
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ServerPackets.welcome, ClientHandle.Welcome },
                { (int)ServerPackets.dlc, ClientHandle.DLC },
                { (int)ServerPackets.versionMismatch, ClientHandle.versionMismatch },
                { (int)ServerPackets.askData, ClientHandle.SendData },
                { (int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
                { (int)ServerPackets.playerPosition, ClientHandle.PlayerPosition },
                { (int)ServerPackets.playerRotation, ClientHandle.PlayerRotation },
                { (int)ServerPackets.playerConnected, ClientHandle.PlayerConnected},
                {(int)ServerPackets.playerDisconnect, ClientHandle.PlayerDisconnect},
                {(int)ServerPackets.items, ClientHandle.ItemReceive},
                {(int)ServerPackets.groupItems, ClientHandle.GroupItemReceive},
                {(int)ServerPackets.stats, ClientHandle.Stats},
                {(int)ServerPackets.playerScene, ClientHandle.PlayerScene},
                {(int)ServerPackets.spawnCars, ClientHandle.SpawnCars},
                {(int)ServerPackets.moveCars, ClientHandle.MoveCar},
                {(int)ServerPackets.initialCarPart, ClientHandle.carParts},
                {(int)ServerPackets.car_part, ClientHandle.carParts},
                {(int)ServerPackets.body_part, ClientHandle.bodyPart},
                {(int)ServerPackets.lifterState, ClientHandle.lifterPos}
            };
            MelonLogger.Msg("Initialized Packets!");
        }
        
        public void Disconnect()
        {
            forceDisconnected = true;
            if (!MainMod.SteamMode)
            {
                foreach (KeyValuePair<int, PlayerInfo> element in PlayerManager.players)
                {
                    if (element.Value != MainMod.localPlayer.GetComponent<PlayerInfo>())
                    {
                        Destroy(element.Value.gameObject);
                    }
                    else
                    {
                        Destroy(MainMod.localPlayer.GetComponent<PlayerInfo>());
                        Destroy(MainMod.localPlayer.GetComponent<MPGameManager>());
                    }
                }

                MainMod.isPrefabSet = false;
                PlayerManager.players.Clear();
                if (MainMod.isHosting)
                {
                    Server.Stop();
                    MainMod.isHosting = false;
                }
            }
            
            if (isConnected)
            {
                isConnected = false;
                tcp.socket.Close();
                udp.socket.Close();

                MainMod.isConnected = false;
                MelonLogger.Msg("Disconnected from server.");
            }
        }
    }
}
