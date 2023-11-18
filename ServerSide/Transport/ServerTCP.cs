using System;
using System.Net.Sockets;
using CMS21MP.ServerSide.DataHandle;
using CMS21MP.SharedData;
using MelonLoader;
using Steamworks;

namespace CMS21MP.ServerSide.Transport
{
     public class ServerTCP
        {
            
            public static int dataBufferSize = 4096;
            
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public ServerTCP(int _id)
            {
                id = _id;
            }

            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();
                stream.ReadTimeout = 200;
                
                receivedData = new Packet();

                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                

                ServerSend.Welcome(new SteamId(), id, "Welcome to the server!");
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Msg($"Error sending data to player{id} via TCP:{e}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0)
                    {
                        if (Server.clients.TryGetValue(id, out var client))
                        {
                            client.Disconnect(id);
                        }
                        return;
                    }

                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    receivedData.Reset(HandleData(_data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception _ex)
                {
                    if(_ex.InnerException is SocketException sockEx && sockEx.ErrorCode != 10054) {
                        MelonLogger.Msg($"Error receiving TCP data: {_ex}"); 
                    }
                    if (Server.clients.TryGetValue(id, out var client))
                    {
                        client.Disconnect(id);
                    }
                }
            }
            public bool HandleData(byte[] _data)
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
                            if(Server.packetHandlers.ContainsKey(_packetId))
                                Server.packetHandlers[_packetId](id, _packet);
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

            public void Disconnect()
            {
                socket.Close();
                stream.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }
}