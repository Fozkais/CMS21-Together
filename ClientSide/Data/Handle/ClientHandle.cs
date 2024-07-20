﻿using System.Net;
using CMS21Together.ClientSide.Data.CustomUI;
using CMS21Together.ClientSide.Data.Garage.Campaign;
using CMS21Together.ClientSide.Data.Garage.Car;
using CMS21Together.ClientSide.Data.Garage.Tools;
using CMS21Together.ClientSide.Data.Player;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using CMS21Together.Shared.Data.Vanilla.Cars;
using CMS21Together.Shared.Data.Vanilla.Jobs;
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

        if(Client.Instance.networkType == NetworkType.TCP)
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
        UI_Lobby.ChangeReadyState(id, ready);
    }
    
    public static void StartPacket(Packet packet)
    {
        Gamemode gamemode = packet.Read<Gamemode>();

        ModSaveData data = new ModSaveData();
        data.selectedGamemode = gamemode;
        
        SavesManager.LoadSave(data, true);
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
        bool initial = packet.Read<bool>();
        int value = packet.ReadInt();
        ModStats type = packet.Read<ModStats>();
        
        MelonLogger.Msg($"Received stat:{value} , {type.ToString()}");
        MelonCoroutines.Start(Stats.UpdateStats(type, value, initial));

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

        // ClientData.Instance.loadedCars[carLoaderID - 1].CarLifterState = (int)state; TODO: fix this?
    }
    
    public static void LoadCarPacket(Packet packet)
    {
        ModNewCarData carData = packet.Read<ModNewCarData>();
        int carLoaderID = packet.ReadInt();
        
        MelonLogger.Msg("[ClientHandle->LoadCarPacket] Received new car info.");

        MelonCoroutines.Start(CarSpawnManager.LoadCarFromServer(carData, carLoaderID));
    }
    
    public static void BodyPartPacket(Packet packet)
    {
        ModCarPart carPart = packet.Read<ModCarPart>();
        int carLoaderID = packet.ReadInt();
        
        MelonLogger.Msg("[ClientHandle->BodyPartPacket] Receive BodyPart.");
        MelonCoroutines.Start(PartsUpdater.UpdateBodyParts(carPart, carLoaderID));
    }
    
    public static void PartScriptPacket(Packet packet)
    {
        ModPartScript partScript = packet.Read<ModPartScript>();
        int carLoaderID = packet.ReadInt();
        
        MelonLogger.Msg("[ClientHandle->PartScriptPacket] Receive PartScript.");
        MelonCoroutines.Start(PartsUpdater.UpdatePartScripts(partScript, carLoaderID));
    }
    
    public static void DeleteCarPacket(Packet packet)
    {
        int carLoaderID = packet.ReadInt();
        
        MelonLogger.Msg($"[ClientHandle->DeleteCarPacket] Delete Car {carLoaderID}.");
        MelonCoroutines.Start(CarSyncManager.DeleteCar(carLoaderID));
    }
    
    public static void CarPositionPacket(Packet packet)
    {
        int placeNo = packet.ReadInt();
        int carLoaderID = packet.ReadInt();
        
        MelonLogger.Msg($"[ClientHandle->CarPositionPacket] Move {carLoaderID} to {placeNo}.");
        MelonCoroutines.Start(CarSyncManager.ChangePosition(carLoaderID, placeNo));
    }
    
    public static void GarageUpgradePacket(Packet packet)
    {
        GarageUpgrade upgrade = packet.Read<GarageUpgrade>();
        
        MelonLogger.Msg($"[ClientHandle->GarageUpgradePacket] Received upgrade for {upgrade.upgradeID}.");
        MelonCoroutines.Start(GarageUpgradeManager.SetUpgrade(upgrade));
    }
    
    public static void JobPacket(Packet packet)
    {
        ModJob job = packet.Read<ModJob>();
        
        MelonLogger.Msg($"[ClientHandle->JobPacket] Received a job.");
        MelonCoroutines.Start(JobManager.AddJob(job));
    }
    
    public static void JobActionPacket(Packet packet)
    {
        int jobID = packet.ReadInt();
        bool takeJob = packet.Read<bool>();

        MelonCoroutines.Start(JobManager.JobAction(jobID, takeJob));
    }
    
    public static void SelectedJobPacket(Packet packet)
    {
        ModJob job = packet.Read<ModJob>();
        bool action = packet.Read<bool>();

        MelonCoroutines.Start(JobManager.SelectedJob(job, action));
    }
}