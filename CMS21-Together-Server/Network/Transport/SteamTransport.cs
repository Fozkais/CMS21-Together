using System;
using Steamworks;
using Steamworks.Data;

namespace CMS21_Together_Server.Network.Transport
{
	public class SteamTransport : SocketManager
    {
        public bool isInitialized { get; private set; }
        
		public event Action<long> OnClientConnected;
        public event Action<long> OnClientDisconnected;
        public event Action<long, byte[]> OnDataReceived;

        public void Initialize(int port)
        {
            // 1. Initialisation du Serveur Steam
            // AppID de CMS21 = 1190000 (Vérifie si c'est le bon)
            // Le port ici est le port "Query" de Steam, pas forcément le port TCP du jeu
            try 
            {

                
                // Init(AppId, Port, GamePort, QueryPort, ServerMode, VersionString)
                SteamServer.Init(1190000, new SteamServerInit("CMS21", "CMS21 Mod")
                {
                    GamePort = (ushort)port,
                    QueryPort = (ushort)(port + 1),
                    Secure = false,
                    VersionString = Program.SERVER_VERSION
                });
                
                SteamServer.LogOnAnonymous();
                // SteamServer.LogOn( "TON_GSLT_TOKEN_ICI" ); TODO:Replace Anonymous with this when exiting test
                
                SteamNetworkingSockets.CreateRelaySocket<SteamTransport>(port);
                Console.WriteLine("[Steam] Serveur Steam initialisé !");
                Console.WriteLine($"Steam server ID: '{SteamServer.SteamId.Value}'");
                isInitialized = true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Steam] Erreur Init: {e.Message}");
            }
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

        public void SendToClient(long connectionId, byte[] data, bool reliable)
        {
            foreach (var conn in Connected)
            {
                if ((long)conn.Id == connectionId)
                {
                    conn.SendMessage(data, reliable ? SendType.Reliable : SendType.Unreliable);
                    return;
                }
            }
        }
        

        public override void OnConnected(Connection connection, ConnectionInfo info)
        {
            base.OnConnected(connection, info);
            Console.WriteLine($"[Steam] Nouveau client : {info.Identity.SteamId}");
            
            OnClientConnected?.Invoke((long)info.Identity.SteamId.Value);
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo info)
        {
            base.OnDisconnected(connection, info);
            Console.WriteLine($"[Steam] Client déconnecté : {info.Identity.SteamId}");
            
            OnClientDisconnected?.Invoke((long)info.Identity.SteamId.Value);
        }

        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            byte[] managedData = new byte[size];
            System.Runtime.InteropServices.Marshal.Copy(data, managedData, 0, size);
            
            OnDataReceived?.Invoke((long)identity.SteamId.Value, managedData);
        }
	}
}