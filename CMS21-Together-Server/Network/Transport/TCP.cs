using System;
using System.Net.Sockets;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;

namespace CMS21_Together_Server.Network.Transport
{
	public class TCP
	{
		public TcpClient socket;
        private readonly int id;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public TCP(int _id)
        {
            id = _id;
        }

        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.ReceiveBufferSize = 4096;
            socket.SendBufferSize = 4096;

            stream = socket.GetStream();
            receivedData = new Packet();
            receiveBuffer = new byte[4096];

            stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    Server.clients[id].Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                // Gestion de la fragmentation des paquets
                receivedData.Reset(HandleData(_data)); 
                
                stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);
            }
            catch (Exception)
            {
                Server.clients[id].Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0) return true;
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);

                // --- NOUVEAU SYSTÈME AVEC PACKET ROUTER ---
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();

                    try 
                    {
                        // On lit l'objet complet (si tes packets sont des classes sérialisées)
                        // Note: Assure-toi que Packet.Read<object>() existe et utilise BinaryFormatter comme avant
                        object packetData = _packet.Read<object>(); 
                        
                        // On dispatch via le Router
                        // On passe 'id' (l'ID du client) pour savoir QUI a envoyé le message
                        PacketRouter.Dispatch((PacketTypes)_packetId, packetData, id);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error packet {_packetId}: {ex.Message}");
                    }
                }
                // ------------------------------------------

                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0) return true;
                }
            }

            if (_packetLength <= 1) return true;

            return false;
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
                Console.WriteLine($"Erreur sending TCP data to {id} : {e.Message}");
            }
        }

        public void Disconnect()
        {
            socket?.Close();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
	}
}