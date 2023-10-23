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

        public void HandleData(Packet _packetData)
        {
            int _packetLength = _packetData.ReadInt();
            byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    Server.packetHandlers[_packetId](id, _packet);
                }
            });
        }

        public void Disconnect()
        {
            endPoint = null;
        }
    }
}