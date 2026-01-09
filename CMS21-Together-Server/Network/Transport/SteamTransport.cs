using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using CMS21_Together_Core;
using CMS21_Together_Core.Data;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using Steamworks;
using Steamworks.Data;

namespace CMS21_Together_Server.Network.Transport
{
	public class SteamTransport : SocketManager
    {
        public static SteamTransport Initialize(int port)
        {
            try 
            {
                SteamServer.Init(1190000, new SteamServerInit("CMS21", "CMS21 Mod")
                {
                    GamePort = (ushort)port,
                    QueryPort = (ushort)(port + 1),
                    Secure = false,
                    VersionString = Program.SERVER_VERSION
                });
                
                SteamServer.LogOnAnonymous();

                int timeout = 0;
               Logger.DebugNoLine("Waiting for Steam Response", "DEBUG");
                while (timeout < 100 && GetServerSteamID() < 90200000000000000)
                {
                    SteamServer.RunCallbacks();
                    Thread.Sleep(25);
                    timeout++;
                    
                    if (timeout % 20 == 0)  Logger.DebugNoLine(".");
                }
                Console.WriteLine("");
                
                var transport = SteamNetworkingSockets.CreateRelaySocket<SteamTransport>(port);
                
                Logger.Info($"Steam server ID: {GetServerSteamID()}");
                return transport;
            }
            catch (Exception e)
            {
                Logger.Error($"Steam Server Init Error: {e.Message}");
            }
            return null;
        }

        public void Update()
        {
            SteamServer.RunCallbacks();
            Receive(); 
        }

        public void Shutdown()
        {
            Close();
            SteamServer.Shutdown();
        }

        public void SendData(Connection conn, byte[] data, bool reliable)
        {
            SendType type = reliable ? SendType.Reliable : SendType.Unreliable;
            
            IntPtr _data = SteamNetworkUtils.ConvertByteArrayToIntPtr(data);

            Result res = conn.SendMessage(_data, data.Length, type);
            if(res != Result.OK)
                Logger.Debug($"[SteamConnection->Send] Could not send packet:{res.ToString()}.");

            if (_data != IntPtr.Zero) Marshal.FreeHGlobal(_data);
        }

        public override void OnConnectionChanged(Connection connection, ConnectionInfo info)
        {
            ulong clientID = info.Identity.SteamId.Value;
            if (info.State == ConnectionState.Connecting)
            {
                if (Server.Clients.Values.All(c => c.isConnected))
                {
                    Logger.Debug($"[SteamTransport->OnConnectionChanged] Incoming connection {clientID} would exceed max connection count. Rejecting.");
                    connection.Close(false, 0, "Max Connection Exceeded");
                    return;
                }

                Result result = connection.Accept();
                if (result == Result.OK)
                    Logger.Debug($"[SteamTransport->OnConnectionChanged] Accepted connection for {clientID}");
                else
                {
                    Logger.Debug($"[SteamTransport->OnConnectionChanged] Client {clientID} couldn't be accepted: {result.ToString()}");
                    connection.Close(false, 0, result.ToString());
                }
            }
            else if (info.State == ConnectionState.Connected)
            {
                OnConnected(connection, info);
                Client client = Server.Clients.Values.First(c => !c.isConnected);
                client.isConnected = true;
                client.ConnectionType = NetworkType.Steam;
                client.steamConnection = connection;
                Server.SendToClient(new ConnectPacket()
                {
                    gameVersion = "",
                    playerGuid = "",
                    username = "",
                    message = "Welcome to server!",
                    modVersion = Program.MOD_VERSION,
                    playerID = client.ID
                }, client.ID);
            }
        }


        public override void OnConnected(Connection connection, ConnectionInfo info)
        {
            base.OnConnected(connection, info);
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo info)
        {
            base.OnDisconnected(connection, info);
        }

        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            byte[] byteData = SteamNetworkUtils.ConvertIntPtrToByteArray(data, size);
            int packetLength = 0;

            int id = Server.Clients.Values.First(c => c.steamConnection == connection).ID;
            if (id == -1) return;
            
            Packet receivedData = new Packet();

            receivedData.SetBytes(byteData);
            if (receivedData.UnreadLength() >= 4)
            {
                packetLength = receivedData.ReadInt();
                if (packetLength <= 0) return ;
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
            }
        }
        
        [DllImport("steam_api64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_SteamGameServer_v013")]
        public static extern IntPtr GetSteamGameServerPointer();

        // 2. La fonction GetSteamID qui prend ce pointeur en argument
        [DllImport("steam_api64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_ISteamGameServer_GetSteamID")]
        public static extern ulong GetSteamID_Native(IntPtr instancePtr);

        public static ulong GetServerSteamID()
        {
            IntPtr serverPtr = GetSteamGameServerPointer();
            if (serverPtr == IntPtr.Zero)
            {
                Logger.Error("SteamGameServer ptr is null. SteamServer.Init as been called?");
                return 0;
            }
            return GetSteamID_Native(serverPtr);
        }
	}
}