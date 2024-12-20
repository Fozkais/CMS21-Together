using System;
using CMS21Together.Shared;
using MelonLoader;
using Steamworks;
using Steamworks.Data;
using VehiclePhysics;

namespace CMS21Together.ClientSide.Transports;

public class ClientSteam : ConnectionManager
{
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
        
        int packetLenght = 0;
        Packet receivedData = new Packet();
    
        receivedData.SetBytes(byteData);
        if (receivedData.UnreadLength() >= 4)
        {
            packetLenght = receivedData.ReadInt();
            if (packetLenght <= 0)
            {
                return;
            }
        }

        while (packetLenght > 0 && packetLenght <= receivedData.UnreadLength())
        {
           byte[] _packetBytes = receivedData.ReadBytes(packetLenght);
           ThreadManager.ExecuteOnMainThread<Exception>(ex =>
           {
               using (Packet _packet = new Packet(_packetBytes))
               {
                   int _packetId = _packet.ReadInt();
                   if (Client.PacketHandlers.ContainsKey(_packetId))
                       Client.PacketHandlers[_packetId](_packet);
                   else
                       MelonLogger.Error($"[ClientSteam->OnMessage] packet with id:{_packetId} is not valid.");
               }
           }, null);
        }
    }

    public void Send(Packet _packet, bool reliable)
    {
        SendType sendType = reliable ? SendType.Reliable : SendType.Unreliable; // Reliable=TCP , Unrealiable=UDP
        Result res = Connection.SendMessage(SteamworksUtils.ConvertByteArrayToIntPtr(_packet.ToArray()), _packet.Length(), sendType);
        if(res != Result.OK)
            MelonLogger.Error($"[ClientSteam->SendData] Issue while sending data:{res}");
    }
}