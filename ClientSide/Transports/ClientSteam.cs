using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CMS21Together.Shared;
using MelonLoader;
using Steamworks;
using Steamworks.Data;
using VehiclePhysics;

namespace CMS21Together.ClientSide.Transports;

public class ClientSteam : ConnectionManager
{
    private const int MAX_STEAM_PACKET_SIZE = 384 * 1024;
    private Dictionary<string, List<byte>> assemblyBuffers = new Dictionary<string, List<byte>>();
    private Dictionary<string, int> expectedSizes = new Dictionary<string, int>();
    
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
    }

    public override void OnDisconnected(ConnectionInfo info)
    {
        base.OnDisconnected(info);
        MelonLogger.Msg("Successfully disconnected from server.");
    }

    public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        base.OnMessage(data, size, messageNum, recvTime, channel);
        
        byte[] byteData =  SteamworksUtils.ConvertIntPtrToByteArray(data, size);
        
        int packetLength = 0;
        Packet receivedData = new Packet();
    
        receivedData.SetBytes(byteData);
        if (receivedData.UnreadLength() >= 4)
        {
            packetLength = receivedData.ReadInt();
            if (packetLength <= 0)
            {
                return;
            }
        }

        while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
        {
           byte[] _packetBytes = receivedData.ReadBytes(packetLength);
           ThreadManager.ExecuteOnMainThread<Exception>(ex =>
           {
               using (Packet _packet = new Packet(_packetBytes))
               {
                   int packetId = _packet.ReadInt();
                   if (packetId == (int)PacketTypes.fragmented)
                   {
                       HandleFragment(_packet);
                   }
                   else
                   {
                       // Exécution normale du handler
                       ExecuteHandler(packetId, _packetBytes);
                   }
               }
           }, null);
           
           packetLength = 0;
           if (receivedData.UnreadLength() >= 4)
           {
               packetLength = receivedData.ReadInt();
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

        // Si on a reçu tous les morceaux pour ce transfert
        if (assemblyBuffers[transferId].Count >= expectedSizes[transferId])
        {
            byte[] fullData = assemblyBuffers[transferId].ToArray();
            
            // Nettoyage des dictionnaires
            assemblyBuffers.Remove(transferId);
            expectedSizes.Remove(transferId);

            using (Packet assembledPacket = new Packet(fullData))
            {
                int finalPacketId = assembledPacket.ReadInt();
                ExecuteHandler(finalPacketId, fullData);
            }
        }
    }

    private void ExecuteHandler(int packetId, byte[] data)
    {
        ThreadManager.ExecuteOnMainThread<Exception>(ex =>
        {
            using (Packet p = new Packet(data))
            {
                p.ReadInt(); // On consomme l'ID pour que le handler lise les données
                if (Client.PacketHandlers.ContainsKey(packetId))
                    Client.PacketHandlers[packetId](p);
                else
                    MelonLogger.Error($"[ClientSteam->ExecuteHandler] packet with id:{packetId} is not valid.");
            }
        }, null);
    }

    public void Send(Packet _packet, bool reliable)
    {
        if (!Connected || Connection.Id == 0) return;

        byte[] data = _packet.ToArray();

        // Si le paquet est trop gros pour Steam, on le fragmente
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
        string transferId = Guid.NewGuid().ToString();
        int totalBytes = fullData.Length;
        int sentBytes = 0;

        while (sentBytes < totalBytes)
        {
            int chunkSize = Math.Min(MAX_STEAM_PACKET_SIZE, totalBytes - sentBytes);
            byte[] chunk = new byte[chunkSize];
            Array.Copy(fullData, sentBytes, chunk, 0, chunkSize);

            using (Packet fragment = new Packet((int)PacketTypes.fragmented))
            {
                fragment.Write(transferId);   // ID unique de ce transfert
                fragment.Write(totalBytes);   // Taille totale attendue à la fin
                fragment.Write(chunk.Length); // Taille de ce morceau
                fragment.Write(chunk);
                
                // Un paquet fragmenté DOIT être Reliable
                InternalRawSend(fragment.ToArray(), true);
            }
            sentBytes += chunkSize;
        }
    }

    private void InternalRawSend(byte[] data, bool reliable)
    {
        SendType sendType = reliable ? SendType.Reliable : SendType.Unreliable;
        IntPtr ptr = SteamworksUtils.ConvertByteArrayToIntPtr(data);
        if (ptr == IntPtr.Zero) return;

        try
        {
            Result res = Connection.SendMessage(ptr, data.Length, sendType);
            if (res != Result.OK)
                MelonLogger.Error($"[ClientSteam->InternalRawSend] Issue while sending data: {res}");
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}