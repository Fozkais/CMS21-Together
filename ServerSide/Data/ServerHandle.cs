using System;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.ClientSide.Data.Player;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using CMS21Together.Shared.Data.Vanilla.Cars;
using CMS21Together.Shared.Data.Vanilla.Jobs;
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
    
    public static void ReadyPacket(int fromClient, Packet packet)
    {
        int id = packet.ReadInt();
        bool ready = packet.Read<bool>();

        ServerData.Instance.connectedClients[id].isReady = ready;

        ServerSend.ReadyPacket(fromClient,ready, id);
    }
    
    public static void PositionPacket(int fromClient, Packet packet)
    {
        Vector3Serializable _position = packet.Read<Vector3Serializable>();
        ServerData.Instance.connectedClients[fromClient].position = _position;
                
        ServerSend.PositionPacket(fromClient, _position);
    }
                
    public static void RotationPacket(int fromClient, Packet packet)
    {
        QuaternionSerializable _rotation = packet.Read<QuaternionSerializable>();
        ServerData.Instance.connectedClients[fromClient].rotation = _rotation;
                    
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
    
    public static void LoadJobCarPacket(int fromClient, Packet packet)
    {
        ModCar carData = packet.Read<ModCar>();
        
        ServerData.Instance.SetLoadJobCar(carData);
    }
    
    public static void LoadCarPacket(int fromClient, Packet packet)
    {
        ModNewCarData carData = packet.Read<ModNewCarData>();
        int carLoaderID = packet.ReadInt();

        ServerData.Instance.CarSpawnDatas[carLoaderID] = carData;

        ServerSend.LoadCarPacket(fromClient, carData, carLoaderID);
    }

    public static void BodyPartPacket(int fromClient, Packet packet)
    {
        ModCarPart carPart = packet.Read<ModCarPart>();
        int carLoaderID = packet.ReadInt();
        
        ServerData.Instance.UpdateBodyParts(carPart, carLoaderID);
        ServerSend.BodyPartPacket(fromClient, carPart, carLoaderID);
    }
    
    public static void PartScriptPacket(int fromClient, Packet packet)
    {
        ModPartScript partScript = packet.Read<ModPartScript>();
        int carLoaderID = packet.ReadInt();
        
        ServerData.Instance.UpdatePartScripts(partScript, carLoaderID);
        ServerSend.PartScriptPacket(fromClient, partScript, carLoaderID);
    }
    
    public static void DeleteCarPacket(int fromClient, Packet packet)
    {
        int carLoaderID = packet.ReadInt();
        
        ServerData.Instance.DeleteCar(carLoaderID);
        ServerSend.DeleteCarPacket(fromClient, carLoaderID);
    }
    
    public static void CarPositionPacket(int fromClient, Packet packet)
    {
        int placeNo = packet.ReadInt();
        int carLoaderID = packet.ReadInt();
        
        ServerData.Instance.ChangePosition(carLoaderID, placeNo);
        ServerSend.CarPositionPacket(fromClient, carLoaderID, placeNo);
    }
    
    public static void GarageUpgradePacket(int fromClient, Packet packet)
    {
        GarageUpgrade upgrade = packet.Read<GarageUpgrade>();
        
        ServerData.Instance.SetGarageUpgrade(upgrade);
        ServerSend.GarageUpgradePacket(fromClient, upgrade);
    }
    
    public static void JobPacket(int fromClient, Packet packet)
    {
        ModJob job = packet.Read<ModJob>();
        
        ServerData.Instance.AddJob(job);
        ServerSend.JobPacket(fromClient, job);
    }
    
    public static void JobActionPacket(int fromClient, Packet packet)
    {
        int jobID = packet.ReadInt();
        bool takeJob = packet.Read<bool>();
        
        ServerData.Instance.RemoveJob(jobID);
        ServerSend.JobActionPacket(fromClient, jobID, takeJob);
    }
    
    public static void SelectedJobPacket(int fromClient, Packet packet)
    {
        ModJob job = packet.Read<ModJob>();
        bool action = packet.Read<bool>();

        ServerData.Instance.UpdateSelectedJobs(job, action);
        ServerSend.SelectedJobPacket(fromClient, job, action);
    }
}