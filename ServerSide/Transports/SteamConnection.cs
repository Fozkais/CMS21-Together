using System;
using System.Collections.Generic;
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
	 
	 private const int MAX_STEAM_PACKET_SIZE = 384 * 1024; 
	 private Dictionary<string, List<byte>> assemblyBuffers = new Dictionary<string, List<byte>>();
	 private Dictionary<string, int> expectedSizes = new Dictionary<string, int>();

	 public SteamConnection(int _id)
	 {
	     isConnected = false;
	     id = _id;
	 }
	
	 
	 public void Send(Packet packet, bool reliable=true)
	 {
	     byte[] data = packet.ToArray();

	     
	     if (data.Length > MAX_STEAM_PACKET_SIZE)
	     {
		     SendFragmented(data);
		     return;
	     }

	     // Envoi normal
	     InternalRawSend(data, reliable);
	 }
	 
	 private void SendFragmented(byte[] fullData)
	 {
		 int totalBytes = fullData.Length;
		 int sentBytes = 0;
		 string transferId = Guid.NewGuid().ToString();

		 while (sentBytes < totalBytes)
		 {
			 int chunkSize = Math.Min(MAX_STEAM_PACKET_SIZE, totalBytes - sentBytes);
			 byte[] chunk = new byte[chunkSize];
			 Array.Copy(fullData, sentBytes, chunk, 0, chunkSize);

			 using (Packet fragment = new Packet((int)PacketTypes.fragmented))
			 {
				 fragment.Write(transferId);
				 fragment.Write(totalBytes);
				 fragment.Write(chunk.Length);
				 fragment.Write(chunk);
				 
				 InternalRawSend(fragment.ToArray(), true);
			 }
			 sentBytes += chunkSize;
		 }
	 }
	 
	 private void InternalRawSend(byte[] data, bool reliable)
	 {
		 SendType type = reliable ? SendType.Reliable : SendType.Unreliable;
		 IntPtr _data = SteamworksUtils.ConvertByteArrayToIntPtr(data);
		 if (_data == IntPtr.Zero) return;

		 try 
		 {
			 Result res = connection.SendMessage(_data, data.Length, type);
			 if (res != Result.OK)
				 MelonLogger.Error($"[SteamConnection->Send] Failed to send {data.Length} bytes. Result: {res}.");
		 }
		 finally 
		 {
			 if (_data != IntPtr.Zero) Marshal.FreeHGlobal(_data);
		 }
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
	    using (Packet receivedData = new Packet(data))
	    {
	        int _packetLength = 0;
	        if (receivedData.UnreadLength() >= 4)
	            _packetLength = receivedData.ReadInt();

	        while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
	        {
	            byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
	            
	            using (Packet _p = new Packet(_packetBytes))
	            {
	                int _packetId = _p.ReadInt();

	                if (_packetId == (int)PacketTypes.fragmented)
	                    HandleFragment(_p);
	                else
	                    ExecuteHandler(_packetId, _packetBytes);
	            }

	            _packetLength = 0;
	            if (receivedData.UnreadLength() >= 4)
	                _packetLength = receivedData.ReadInt();
	        }
	    }
	}

	private void HandleFragment(Packet packet)
	{
		string transferId = packet.Read<string>();
	    int totalSize = packet.ReadInt();
	    int chunkSize = packet.ReadInt();
	    byte[] chunk = packet.ReadBytes(chunkSize);

	    if (!assemblyBuffers.ContainsKey(transferId))
	    {
		    assemblyBuffers[transferId] = new List<byte>();
		    expectedSizes[transferId] = totalSize;
	    }
	    
	    assemblyBuffers[transferId].AddRange(chunk);

	    if (assemblyBuffers[transferId].Count >= expectedSizes[transferId])
	    {
	        byte[] fullPacketRaw = assemblyBuffers[transferId].ToArray();
	        
	        // On réinitialise pour le prochain gros transfert
	        assemblyBuffers.Remove(transferId);
	        expectedSizes.Remove(transferId);

	        // On traite le paquet réassemblé
	        using (Packet fullPacket = new Packet(fullPacketRaw))
	        {
	            int finalId = fullPacket.ReadInt();
	            ExecuteHandler(finalId, fullPacketRaw);
	        }
	    }
	}

	private void ExecuteHandler(int packetId, byte[] data)
	{
	    ThreadManager.ExecuteOnMainThread<Exception>(ex =>
	    {
	        using (Packet p = new Packet(data))
	        {
	            p.ReadInt(); // Skip ID
	            if (Server.packetHandlers.ContainsKey(packetId))
	                Server.packetHandlers[packetId](id, p);
	        }
	    }, null);
	}
}