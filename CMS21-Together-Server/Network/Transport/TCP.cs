using System;
using System.Net.Sockets;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Server.Log;

namespace CMS21_Together_Server.Network.Transport
{
	public class Tcp
	{
		public TcpClient Socket;
        private readonly int id;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;
        

        public Tcp(int id)
        {
            this.id = id;
        }

        public void Connect(TcpClient socket)
        {
            Socket = socket;
            Socket.ReceiveBufferSize = 4096;
            Socket.SendBufferSize = 4096;

            stream = Socket.GetStream();
            receivedData = new Packet();
            receiveBuffer = new byte[4096];

            stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0)
                {
                    Server.Clients[id].Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                // Gestion de la fragmentation des paquets
                receivedData.Reset(HandleData(data)); 
                
                stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);
            }
            catch (Exception)
            {
                Server.Clients[id].Disconnect();
            }
        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;

            receivedData.SetBytes(data);

            if (receivedData.UnreadLength() >= 4)
            {
                packetLength = receivedData.ReadInt();
                if (packetLength <= 0) return true;
            }

            while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
            {
                byte[] packetBytes = receivedData.ReadBytes(packetLength);
                
                using (Packet packet = new Packet(packetBytes))
                {
                    int packetId = packet.ReadInt();

                    try 
                    {
                        
                        object packetData = packet.Read<object>(); 
                        PacketRouter.Dispatch((PacketTypes)packetId, packetData, id);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error packet {packetId}: {ex.Message}");
                    }
                }

                packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0) return true;
                }
            }

            if (packetLength <= 1) return true;

            return false;
        }

        public void SendData(Packet packet)
        {
            try
            {
                if (Socket != null)
                {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Erreur sending TCP data to {id} : {e.Message}");
            }
        }

        public void Disconnect()
        {
            Socket?.Close();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            Socket = null;
        }
	}
}