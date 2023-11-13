using System;
using System.Net.Sockets;
using CMS21MP.ClientSide.DataHandle;
using CMS21MP.SharedData;
using MelonLoader;

namespace CMS21MP.ClientSide.Transport
{
    public class ClientTCP
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
                    ReceiveBufferSize = Client.dataBufferSize,
                    SendBufferSize = Client.dataBufferSize
                };

                receiveBuffer = new byte[Client.dataBufferSize];

                socket.BeginConnect(Client.Instance.ip, Client.Instance.port, ConnectCallback, socket);
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
                
                ModUI.Instance.ShowLobbyInterface();
                stream = socket.GetStream();
                receivedData = new Packet();

                stream.BeginRead(receiveBuffer, 0, Client.dataBufferSize, ReceiveCallback, null);
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
                    Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, Client.dataBufferSize, ReceiveCallback, null);
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
                        if(Client.PacketHandlers.ContainsKey(_packetId))
                            Client.PacketHandlers[_packetId](_packet);
                        else
                            MelonLogger.Msg($"Invalid packetId: {_packetId} ");
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

        public void Disconnect()
        {
            ClientSend.Disconnect(Client.Instance.Id);
            
            socket.Close();
            
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }
}