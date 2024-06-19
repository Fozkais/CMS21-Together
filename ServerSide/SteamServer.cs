using System;
using System.Runtime.InteropServices;
using CMS21Together.Shared;
using MelonLoader;
using Steamworks;
using Steamworks.Data;

namespace CMS21Together.ServerSide
{
    public class SteamServer 
    {
        public async void HostLobby()
        {
            await SteamMatchmaking.CreateLobbyAsync(MainMod.MAX_PLAYER);
        }

        /*public override void OnMessage( Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel )
        {
            MelonLogger.Msg( $"We got a message from {identity}!" );

            ThreadManager.ExecuteOnMainThread<Exception>(ex =>
            {
                using (Packet _packet = new Packet(ConvertIntPtrToByteArray(data, size)))
                {
                    int _packetId = _packet.ReadInt();
                    int id = _packet.ReadInt(); // TODO: ensure clientID is written on send Method
                    Server.packetHandlers[_packetId](id, _packet);
                }
            }, null);
            
            
            // Send it right back
            // connection.SendMessage( data, size, SendType.Reliable); TODO: Adapt ServerSend to use this method.
        }

        public static void SendData(Packet _packet, int toClient)
        {
            IntPtr data = ConvertByteArrayToIntPtr(_packet.ToArray());
            int index = 0;
            foreach (Connection client in Server.steamServer.Connected)
            {
                if(index == toClient)
                    client.SendMessage(data, _packet.Length());
                
                index++;
            }
            Marshal.FreeHGlobal(data);
        }
        
        public static byte[] ConvertIntPtrToByteArray(IntPtr ptr, int size)
        {
            byte[] byteArray = new byte[size];
            Marshal.Copy(ptr, byteArray, 0, size);
            return byteArray;
        }
        
        public static IntPtr ConvertByteArrayToIntPtr(byte[] byteArray)
        {
            // Alloue de la mémoire non managée
            IntPtr ptr = Marshal.AllocHGlobal(byteArray.Length);

            // Copie les données du tableau de bytes dans la mémoire non managée
            Marshal.Copy(byteArray, 0, ptr, byteArray.Length);

            return ptr;
        }*/
    }
}