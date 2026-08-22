using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CMS21_Together_Core;
using CMS21_Together_Core.Data.Enum;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21_Together_Server.Log;
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

                bool isConnectedToSteam = false;
                Action onConnected = () => 
                {
                    isConnectedToSteam = true;
                };
                SteamServer.OnSteamServersConnected += onConnected;
                int timeout = 0;
                
                if (Program.Config.GsltToken != string.Empty)
                    LogOn(Program.Config.GsltToken);
                else
                {
                    Logger.Warn("GSLT Token not set. Login as Anonymous");
                    Logger.Warn("Without GSLT Token server ID will not be persistent.");
                   
                    SteamServer.LogOnAnonymous();
                }
                
                Logger.DebugNoNL("Waiting for Steam Response", "DEBUG");
                while (timeout < 50 && !isConnectedToSteam)
                {
                    SteamServer.RunCallbacks();
                    Thread.Sleep(100);
                    timeout++;
                    
                    if (timeout % 10 == 0)  Logger.DebugNoNL(".");
                }
                Console.WriteLine("");
                SteamServer.OnSteamServersConnected -= onConnected;
                
                if (isConnectedToSteam)
                {
                    ulong steamID = GetServerSteamID();
                    Logger.Success($"Steam connection established! SteamID: {steamID}");
                }
                else
                {
                    Logger.Warn("Timeout reached. Steam connection could not be established in time.");
                    Logger.Warn("Server will continue over DirectIp.");
                    SteamServer.Shutdown();
                    return null;
                }
                
                var transport = SteamNetworkingSockets.CreateRelaySocket<SteamTransport>(port);
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
                if (Server.Clients.Values.All(c => c.IsConnected))
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
                Client client = Server.Clients.Values.First(c => !c.IsConnected);
                client.IsConnected = true;
                client.ConnectionType = NetworkType.Steam;
                client.SteamConnection = connection;
                Server.SendToClient(new ConnectPacket()
                {
                    gameVersion = "",
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

            int id = Server.Clients.Values.First(c => c.SteamConnection == connection).ID;
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
        public static ulong GetServerSteamID()
        {
            IntPtr serverPtr = SteamNative.GetSteamGameServerPointer();
            if (serverPtr == IntPtr.Zero)
            {
                Logger.Error("SteamGameServer ptr is null. SteamServer.Init as been called?");
                return 0;
            }
            return SteamNative.GetSteamID_Native(serverPtr);
        }
        
        public static void LogOn(string token)
        {
            IntPtr serverPtr = SteamNative.GetSteamGameServerPointer();

            if (serverPtr == IntPtr.Zero)
            {
                Logger.Error("SteamGameServer ptr is null. SteamServer.Init has likely not been called.");
                return;
            }

            if (string.IsNullOrEmpty(token))
            {
                Logger.Error("LogOn called with an empty or null token.");
                return;
            }
            
            byte[] tokenBytes = Encoding.UTF8.GetBytes(token + "\0");
            IntPtr tokenPtr = Marshal.AllocHGlobal(tokenBytes.Length);

            try
            {
                Marshal.Copy(tokenBytes, 0, tokenPtr, tokenBytes.Length);
                Logger.Debug("Logging on to Steam with GSLT...");
                SteamNative.LogOn_Native(serverPtr, tokenPtr);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while trying to LogOn: {ex.Message}");
            }
            finally
            {
                Marshal.FreeHGlobal(tokenPtr);
            }
        }
    }
}