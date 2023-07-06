using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using CMS21MP.DataHandle;
using CMS21MP.ServerSide;
using MelonLoader;
using UnityEngine.Serialization;


namespace CMS21MP.ClientSide
{
    public class Client : MonoBehaviour
    {
        public static Client Instance;
        public static int dataBufferSize = 4096;

        public string ip = "127.0.0.1";
        public int port;
        public int Id;
        public TCP tcp;
        public UDP udp;

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
            tcp = new TCP();
            udp = new UDP();
            
            threadManager = new ThreadManager();
        }

        public void ClientOnApplicationQuit()
        {
            Disconnect();
        }


        public void ConnectToServer(string _ipAdress)
        {
            InitializeClientData();
            Instance.ip = _ipAdress;
            Instance.port = MainMod.PORT;

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
                PacketHandlers.Clear();
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

                    socket.BeginConnect(Instance.ip, Instance.port, ConnectCallback, socket);
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
                        Instance.Disconnect();
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
                            PacketHandlers[_packetId](_packet);
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
                Instance.Disconnect();

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
                    endPoint = new IPEndPoint(IPAddress.Parse(Instance.ip), Instance.port);
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
                        endPoint = new IPEndPoint(IPAddress.Parse(Instance.ip), Instance.port);
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
                        _packet.InsertInt(Instance.Id);
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
                            Instance.Disconnect();
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
                            Client.Instance.ConnectToServer(Instance.ip);
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

                            if (PacketHandlers.TryGetValue(_packetId, out var handler))
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
                    Instance.Disconnect();
                    endPoint = null;
                    socket = null;
                }
            }

        private void InitializeClientData()
        {
            PacketHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)PacketTypes.welcome, ClientHandle.Welcome },
            };
            MelonLogger.Msg("Initialized Packets!");
        }
        
        public void Disconnect()
        {
            forceDisconnected = true;
            
            if (isConnected)
            {
                isConnected = false;
                tcp.socket.Close();
                udp.socket.Close();
                
                MelonLogger.Msg("Disconnected from server.");
            }
        }
    }
}
