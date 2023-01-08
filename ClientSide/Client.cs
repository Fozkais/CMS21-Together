using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
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

        public bool isConnected = false;

        private delegate void PacketHandler(Packet _packet);

        private static Dictionary<int, PacketHandler> packetHandlers;

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
            tcp.Connect();
        }

        public class TCP
        {
            public TcpClient socket;

            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public void Connect()
            {
                socket = new TcpClient
                {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize
                };

                receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
            }

            private void ConnectCallback(IAsyncResult _result)
            {
                socket.EndConnect(_result);

                if (!socket.Connected)
                {
                    return;
                }

                stream = socket.GetStream();

                receivedData = new Packet();

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
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
                            instance.Disconnect();
                            return;
                        }

                        HandleData(_data);
                    }
                    catch
                    {
                        Disconnect();
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
                            packetHandlers[_packetId](_packet);
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
                { (int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
                { (int)ServerPackets.playerPosition, ClientHandle.PlayerPosition },
                { (int)ServerPackets.playerRotation, ClientHandle.PlayerRotation },
                { (int)ServerPackets.playerConnected, ClientHandle.PlayerConnected}
            };
            MelonLogger.Msg("Initialized Packets!");
        }
        
        public void Disconnect()
        {
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
