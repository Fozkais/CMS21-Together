using System.Net;
using CMS21Together.ClientSide.Data.CustomUI;
using CMS21Together.Shared.Data;
using MelonLoader;

namespace CMS21Together.ClientSide.Data.Handle;

public static class ClientHandle
{
    public static void ConnectPacket(Packet packet)
    {
        string message = packet.Read<string>();
        int newID = packet.ReadInt();
        
        MelonLogger.Msg($"[ClientHandle->ConnectPacket] {message}");
        ClientData.UserData.playerID = newID;

        if(Client.Instance.networkType == NetworkType.tcp)
            Client.Instance.udp.Connect();
        
        ClientSend.ConnectValidationPacket();
    }

    public static void DisconnectPacket(Packet packet)
    {
        string message = packet.Read<string>();
        
        MelonLogger.Msg($"[ClientHandle->DisconnectPacket] You've been disconnected from server: {message}");
        Client.Instance.Disconnect();
    }

    public static void UserDataPacket(Packet packet)
    {
        UserData data = packet.Read<UserData>();

        ClientData.Instance.ConnectedClients[data.playerID] = data;
        UI_Lobby.AddPlayerToLobby(data.username, data.playerID);
        MelonLogger.Msg("[ClientHandle->UserDataPacket] Receive userData from server.");
    }
}