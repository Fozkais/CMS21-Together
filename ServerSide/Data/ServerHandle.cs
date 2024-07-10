using System.Linq;
using CMS21Together.ClientSide.Data.Player;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
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
    
    public static void PositionPacket(int fromClient, Packet packet)
    {
        Vector3Serializable _position = packet.Read<Vector3Serializable>();
        ServerData.Instance.ConnectedClients[fromClient].position = _position;
                
        ServerSend.PositionPacket(fromClient, _position);
    }
                
    public static void RotationPacket(int fromClient, Packet packet)
    {
        QuaternionSerializable _rotation = packet.Read<QuaternionSerializable>();
        ServerData.Instance.ConnectedClients[fromClient].rotation = _rotation;
                    
        ServerSend.RotationPacket(fromClient, _rotation);
    }
    
    public static void ItemPacket(int fromClient, Packet _packet)
    {
        InventoryAction action = _packet.Read<InventoryAction>();
                

        if (action != InventoryAction.resync)
        {
            ModItem item = _packet.Read<ModItem>();
                    
            if (action == InventoryAction.add)
            {
                if (!ServerData.Instance.items.Any(s => s.UID == item.UID))
                {
                    ServerData.Instance.items.Add(item);
                }
            }
            else
            {
                if (ServerData.Instance.items.Any(s => s.UID == item.UID))
                {
                    int index = ServerData.Instance.items.FindIndex(s => s.UID == item.UID);
                    ServerData.Instance.items.Remove(ServerData.Instance.items[index]);
                }
            }
            ServerSend.ItemPacket(fromClient, item, action);
            return;
        }

        foreach (ModItem modItem in ServerData.Instance.items)
        {
            ServerSend.ItemPacket(fromClient, modItem, action);
        }
    }
    
    public static void GroupItemPacket(int fromClient, Packet _packet)
    {
        InventoryAction action = _packet.Read<InventoryAction>();
                

        if (action != InventoryAction.resync)
        {
            ModGroupItem item = _packet.Read<ModGroupItem>();
                    
            if (action == InventoryAction.add)
            {
                if (!ServerData.Instance.groupItems.Any(s => s.UID == item.UID))
                {
                    ServerData.Instance.groupItems.Add(item);
                }
            }
            else
            {
                if (ServerData.Instance.groupItems.Any(s => s.UID == item.UID))
                {
                    int index = ServerData.Instance.groupItems.FindIndex(s => s.UID == item.UID);
                    ServerData.Instance.groupItems.Remove(ServerData.Instance.groupItems[index]);
                }
            }
            ServerSend.GroupItemPacket(fromClient, item, action);
            return;
        }

        foreach (ModGroupItem modItem in ServerData.Instance.groupItems)
        {
            ServerSend.GroupItemPacket(fromClient, modItem, action);
        }
    }
    
    public static void StatPacket(int fromClient, Packet packet)
    {
        int value = packet.ReadInt();
        ModStats type = packet.Read<ModStats>();
                
        switch (type)
        {
            case ModStats.money:
                ServerData.Instance.money = value;
                break;
            case ModStats.scrap:
                ServerData.Instance.scrap = value;
                break;
        }
        ServerSend.StatPacket(fromClient, value, type);
    }
    
    public static void LifterPacket(int fromClient, Packet packet)
    {
        ModLifterState state = packet.Read<ModLifterState>();
        int carLoaderID = packet.ReadInt();

        ServerSend.LifterPacket(fromClient, state, carLoaderID);
    }
}