using System;
using CMS21Together.ServerSide;
using CMS21Together.Shared;
using CMS21Together.Shared.Steam;
using MelonLoader;
using Steamworks;
using Steamworks.Data;

namespace CMS21Together.ClientSide
{
    public class SteamClient : ConnectionManager
    {
        
        
        public static void OnMessage( Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel )
        {
            MelonLogger.Msg( $"We got a message from {identity}!" );

            ThreadManager.ExecuteOnMainThread<Exception>(ex =>
            {
                using (Packet _packet = new Packet(NetworkingUtils.ConvertIntPtrToByteArray(data, size)))
                {
                    int _packetId = _packet.ReadInt();
                    Client.PacketHandlers[_packetId](_packet);
                }
            }, null);
            
            
            // Send it right back
            // connection.SendMessage( data, size, SendType.Reliable); TODO: Adapt ServerSend to use this method.
        }
    }
}