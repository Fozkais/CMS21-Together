using System;
using System.Net;
using CMS21MP.ClientSide;
using CMS21MP.SharedData;
using MelonLoader;

namespace CMS21MP.ServerSide.Transport
{
    public class ServerUDP
    {
        public IPEndPoint endPoint;
        private int id;
        private Packet receivedData;

        public ServerUDP(int _id)
        {
            id = _id;
        }

        public bool IsConnected()
        {
            return endPoint != null;
        }

        public void Connect(IPEndPoint _endPoint)
        {
            endPoint = _endPoint;
        }

        public void SendData(Packet _packet)
        {
            Server.SendUDPData(endPoint, _packet);
        }

        public bool HandleData(byte[] _data)
        {
            receivedData = new Packet();
            
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
            endPoint = null;
        }
    }
}