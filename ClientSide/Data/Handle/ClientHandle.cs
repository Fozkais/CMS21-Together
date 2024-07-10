using System.Net;
using CMS21Together.ClientSide.Data.CustomUI;
using CMS21Together.ClientSide.Data.Garage.Tools;
using CMS21Together.ClientSide.Data.Player;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using Il2Cpp;
using MelonLoader;
using Inventory = CMS21Together.ClientSide.Data.Player.Inventory;

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

        ClientData.Instance.connectedClients[data.playerID] = data;
        UI_Lobby.AddPlayerToLobby(data.username, data.playerID);
        MelonLogger.Msg("[ClientHandle->UserDataPacket] Receive userData from server.");
    }

    public static void ReadyPacket(Packet packet)
    {
        int id = packet.ReadInt();
        bool ready = packet.Read<bool>();
        
        ClientData.Instance.connectedClients[id].isReady = ready;
    }
    
    public static void PositionPacket(Packet packet)
    {
        int id = packet.ReadInt();
        Vector3Serializable position = packet.Read<Vector3Serializable>();
        Movement.UpdatePosition(id, position);
        packet.Dispose();
    }
                
    public static void RotationPacket(Packet packet)
    {
        int id = packet.ReadInt();
        QuaternionSerializable rotation = packet.Read<QuaternionSerializable>();
        Rotation.UpdateRotation(id, rotation);
        packet.Dispose();
    }
    
    public static void ItemPacket(Packet packet)
    {
        InventoryAction action = packet.Read<InventoryAction>();
        ModItem item = packet.Read<ModItem>();

        MelonCoroutines.Start(Inventory.HandleItem(item, action));
        packet.Dispose();
    }
    
    public static void GroupItemPacket(Packet packet)
    {
        InventoryAction action = packet.Read<InventoryAction>();
        ModGroupItem item = packet.Read<ModGroupItem>();

        MelonCoroutines.Start(Inventory.HandleGroupItem(item, action));
        packet.Dispose();
    }

    public static void StatPacket(Packet packet)
    {
        int value = packet.ReadInt();
        ModStats type = packet.Read<ModStats>();
                
        switch (type)
        {
            case ModStats.money:
                ClientData.Instance.money = value;
                GlobalData.PlayerMoney = value;
                break;
            case ModStats.scrap:
                ClientData.Instance.scrap = value;
                GlobalData.PlayerScraps = value;
                break;
        }
    }

    public static void LifterPacket(Packet packet)
    {
        if(ClientData.UserData.scene != GameScene.garage) return;
        
        ModLifterState state = packet.Read<ModLifterState>();
        int carLoaderID = packet.ReadInt();
        
        if(!ClientData.Instance.loadedCars.ContainsKey(carLoaderID-1)) return;

        var lifter = GameData.Instance.carLoaders[carLoaderID - 1].lifter;
        LifterLogic.listen = false;

        if ((int)state > (int)lifter.currentState)
            lifter.Action(0);
        else
            lifter.Action(1);

        ClientData.Instance.loadedCars[carLoaderID - 1].CarLifterState = (int)state;
    }
}