using CMS21Together.Shared.Data;
using MelonLoader;

namespace CMS21Together.ServerSide.Data;

public static class ServerHandle
{
    public static void ConnectValidationPacket(int fromClient, Packet packet)
    {
        int clientIdCheck = packet.ReadInt();
        string username = packet.Read<string>();
       // ReadOnlyDictionary<string, bool> content = _packet.Read<ReadOnlyDictionary<string, bool>>();
       // string gameVersion = packet.Read<string>();TODO: reimplement those
       string modVersion = packet.Read<string>(); 
        
        MelonLogger.Msg($"[ServerHandle->ConnectValidationPacket] Received info : {clientIdCheck},{username},{modVersion}");

        if (modVersion != MainMod.ASSEMBLY_MOD_VERSION)
        {
            ServerSend.DisconnectPacket(fromClient, $"Server is on {MainMod.ASSEMBLY_MOD_VERSION} .");
            return;
        }

        if (fromClient != clientIdCheck)
        {
            ServerSend.DisconnectPacket(fromClient, $"Error on ClientIdCheck.");
            return;
        }
        
        MelonLogger.Msg($"[ServerHandle->ConnectValidationPacket] {username} connected successfully.");
        Server.Instance.clients[fromClient].SendToLobby(username);
    }

    public static void DisconnectPacket(int fromclient, Packet packet)
    {
        MelonLogger.Msg("[ServerHandle->DisconnectPacket] as disconnected from server.");
        Server.Instance.clients[fromclient].Disconnect();
    }
}