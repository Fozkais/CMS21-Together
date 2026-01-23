using System;
using System.Runtime.InteropServices;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using MelonLoader;
using Steamworks;
using Steamworks.Data;

namespace CMS21Together.Network.Transport;

public class ClientSteam : ConnectionManager
{
	public static ClientSteam ConnectToServer(ulong serverSteamId)
	{
		return SteamNetworkingSockets.ConnectRelay<ClientSteam>(serverSteamId, 7777);
	}

	public override void OnConnectionChanged(ConnectionInfo info)
	{
		if (info.State == ConnectionState.Connecting)
		{
			Interface?.OnConnecting(info);
			Connecting = true;
			OnConnecting(info);
			MelonLogger.Msg("[ClientSteam->OnConnectionChanged] Connection in progress.");   
		}
		else if (info.State == ConnectionState.Connected)
		{
			Interface?.OnConnected(info);
			Connected = true;
			Connecting = false;
			OnConnected(info);
			MelonLogger.Msg("[ClientSteam->OnConnectionChanged] Connection established.");
		}
		else if (info.State == ConnectionState.ClosedByPeer || info.State == ConnectionState.Dead || info.State == ConnectionState.None)
		{
			Connected = false;
			OnDisconnected(info);
			MelonLogger.Msg("[ClientSteam->OnConnectionChanged] Disconnected.");
			Close();
		}
		else
		{
			MelonLogger.Msg($"[ClientSteam->OnConnectionChanged] Connection state changed: {info.State.ToString()}");
		}
	}
    
	public override void OnConnecting(ConnectionInfo info)
	{
		MelonLogger.Msg("Connecting to server.");
	}

	public override void OnConnected(ConnectionInfo info)
	{
		MelonLogger.Msg("Successfully connected to server.");
		Client.Instance.Send(new ConnectPacket()
		{
			gameVersion = "",
			message = "",
			modVersion = MainMod.ASSEMBLY_MOD_VERSION,
			playerID = Client.Instance.ID,
			username = $"TestUser{Client.Instance.ID}"
		});
	}

	public override void OnDisconnected(ConnectionInfo info)
	{
		base.OnDisconnected(info);
		MelonLogger.Msg("Successfully disconnected from server.");
	}

	public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
	{
		base.OnMessage(data, size, messageNum, recvTime, channel);

		MelonLogger.Msg("Received a packet from server!");
		
		byte[] byteData = SteamNetworkUtils.ConvertIntPtrToByteArray(data, size);
		
		int packetLength = 0;
		Packet receivedData = new Packet();

		receivedData.SetBytes(byteData);
		if (receivedData.UnreadLength() >= 4)
		{
			packetLength = receivedData.ReadInt();
			if (packetLength <= 0)
			{
				MelonLogger.Msg("Packet is empty");
				return;
			}
		}

		while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
		{
			byte[] packetBytes = receivedData.ReadBytes(packetLength);
            
			ThreadManager.ExecuteOnMainThread<object>((_) =>
			{
				using (Packet packet = new Packet(packetBytes))
				{
					int packetId = packet.ReadInt();
					try 
					{
						object dataObject = packet.Read<object>();
						PacketRouter.Dispatch((PacketTypes)packetId, dataObject, 0);
					}
					catch (Exception ex)
					{
						MelonLogger.Error($"Error reading packet {packetId}: {ex.Message}");
					}
				}
			}, null); 

			packetLength = 0;
			if (receivedData.UnreadLength() >= 4)
			{
				packetLength = receivedData.ReadInt();
				if (packetLength <= 0) return;
			}
		}
	}
	
	public void Send(Packet _packet, bool reliable)
	{
		MelonLogger.Msg("Sent a packet to server.");

		IntPtr data = SteamNetworkUtils.ConvertByteArrayToIntPtr(_packet.ToArray());
		
		SendType sendType = reliable ? SendType.Reliable : SendType.Unreliable;
		Result res = Connection.SendMessage(data, _packet.Length(), sendType);
		if(res != Result.OK)
			MelonLogger.Error($"[ClientSteam->SendData] Issue while sending data:{res}");
		if (data != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(data); 
		}
	}
}