using System;
using System.Runtime.InteropServices;
using CMS21Together.Shared;
using MelonLoader;
using Steamworks;
using Steamworks.Data;

namespace CMS21Together.ServerSide.Transports;

public class SteamConnection
{
	 public readonly int id;
	 public Connection connection;
	 public bool isConnected;

	 public SteamConnection(int _id)
	 {
	     isConnected = false;
	     id = _id;
	 }
	 public void Send(Packet packet, bool reliable=true)
	 {
	     SendType type = reliable ? SendType.Reliable : SendType.Unreliable;

	     byte[] data = packet.ToArray();
	     IntPtr _data = SteamworksUtils.ConvertByteArrayToIntPtr(data);

	     Result res = connection.SendMessage(_data, data.Length, type);
	     if(res != Result.OK)
	         MelonLogger.Error($"[SteamConnection->Send] Could not send packet:{res.ToString()}.");

	     if (_data != IntPtr.Zero) Marshal.FreeHGlobal(_data);
	 }

	 public void Disconnect()
	 {
	     if (isConnected)
	     {
	         isConnected = false;
	         connection.Close();
	     }
	 }

	 public void HandleData(byte[] data)
	 {
	     int _packetLenght = 0;
	     Packet receivedData = new Packet();

	     receivedData.SetBytes(data);
	     if (receivedData.UnreadLength() >= 4)
	     {
	         _packetLenght = receivedData.ReadInt();
	         if (_packetLenght <= 0)
	         {
	             return;
	         }
	     }

	     while (_packetLenght > 0 && _packetLenght <= receivedData.UnreadLength())
	     {
	         byte[] _packetBytes = receivedData.ReadBytes(_packetLenght);
	         ThreadManager.ExecuteOnMainThread<Exception>(ex =>
	         {
	             using (Packet _packet = new Packet(_packetBytes))
	             {
	                 int _packetId = _packet.ReadInt();
	                 if (Server.packetHandlers.ContainsKey(_packetId))
	                     Server.packetHandlers[_packetId](id, _packet);
	             }
	         }, null);
	     }
	 }
}