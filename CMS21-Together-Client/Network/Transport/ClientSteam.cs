using System;
using MelonLoader;
using Steamworks;
using Steamworks.Data;

namespace CMS21Together.Network.Transport;

public class ClientSteam : ConnectionManager
{
	public static ClientSteam ConnectToServer(ulong serverSteamId)
	{
		return SteamNetworkingSockets.ConnectRelay<ClientSteam>(serverSteamId);
	}

	public override void OnConnected(ConnectionInfo info)
	{
		base.OnConnected(info);
		MelonLogger.Msg("[Steam] Connecté au serveur !");
	}

	public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
	{
		byte[] managedData = new byte[size];
		System.Runtime.InteropServices.Marshal.Copy(data, managedData, 0, size);
        
		// Traitement (HandleData comme pour TCP) TODO
	}
}