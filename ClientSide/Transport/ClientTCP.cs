using System;
using System.Net.Sockets;
using System.Threading;
using CMS21Together.ClientSide.Data.CustomUI;
using CMS21Together.Shared;
using MelonLoader;

namespace CMS21Together.ClientSide.Transport
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
                
               // ModUI.Instance.ShowLobbyInterface();  TODO: Fix
                stream = socket.GetStream();
                receivedData = new Packet();
                
                stream = socket.GetStream();
                receivedData = new Packet();

                stream.BeginRead(receiveBuffer, 0, Client.dataBufferSize, ReceiveCallback, null);
            }
            catch // Capturer toutes les exceptions possibles
            {
                CustomLobbyMenu.DisableLobby(); 
               // CustomUIMain.EnableMultiplayerMenu();
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            if(stream == null) return;
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
                Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
                stream.BeginRead(receiveBuffer, 0, Client.dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
               // MelonLogger.Msg($"Error caused Disconnection! : {e}");
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
            
            
           // MelonLogger.Msg("Received a valid packet !");

            while (_packetLenght > 0 && _packetLenght <= receivedData.UnreadLength())
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLenght);
                //MelonLogger.Msg("Reading packet lenght:" + _packetBytes);
                ThreadManager.ExecuteOnMainThread<Exception>(ex =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        // MelonLogger.Msg("Received a packet with id:" + _packetId + " !");
                        if (Client.PacketHandlers.ContainsKey(_packetId))
                        {
                            Client.PacketHandlers[_packetId](_packet);
                        }
                        else
                            MelonLogger.Msg("packet is Invalid !!!");
                    }
                },  null);
                
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
            if(socket != null)
                socket.Close();
            
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }
}