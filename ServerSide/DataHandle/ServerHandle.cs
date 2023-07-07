using CMS21MP.SharedData;
using MelonLoader;

namespace CMS21MP.ServerSide.DataHandle
{
    public static class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();

            MelonLogger.Msg($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected succesfully and is now {_username}.");

            if (_fromClient != _clientIdCheck)
            {
                MelonLogger.Msg($"Player \"{_username}\" (ID:{_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }
            Server.clients[_fromClient].SendIntoGame(_username);
        }
        
        public static void Disconnect(int _fromClient, Packet _packet)
        {
            int id = _packet.ReadInt();
            
            Server.clients[_fromClient].Disconnect(id);
            MelonLogger.Msg($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} " + $"has disconnected.");
        }
        
        public static void ReadyState(int _fromClient, Packet _packet)
        {
            bool _ready = _packet.ReadBool();
            int _id = _packet.ReadInt();

            Server.clients[_id].player.isReady = _ready;

            ServerSend.SendReadyState(_fromClient,_ready, _id);
        }
    }
}