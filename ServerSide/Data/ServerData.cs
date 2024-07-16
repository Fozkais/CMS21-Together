﻿using System.Collections.Generic;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Garage.Car;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;

namespace CMS21Together.ServerSide.Data;

public class ServerData
{
    public static ServerData Instance;
    
    public Dictionary<int, UserData> connectedClients = new Dictionary<int, UserData>();
    public List<ModItem> items = new List<ModItem>();
    public List<ModGroupItem> groupItems = new List<ModGroupItem>();
    public int money, scrap;

    public Dictionary<int, ModNewCarData> CarSpawnDatas = new Dictionary<int, ModNewCarData>();
    public Dictionary<int, ModCarInfo> CarPartInfo = new Dictionary<int, ModCarInfo>();

    public void DeleteCar(int carLoaderID)
    {
        if (CarSpawnDatas.ContainsKey(carLoaderID))
            CarSpawnDatas.Remove(carLoaderID);
        if (CarPartInfo.ContainsKey(carLoaderID))
            CarPartInfo.Remove(carLoaderID);
    }
    
    public void UpdatePartScripts(ModPartScript partScript, int carLoaderID)
    {
        if(!ServerData.Instance.CarPartInfo.ContainsKey(carLoaderID))
            ServerData.Instance.CarPartInfo.Add(carLoaderID, new ModCarInfo());

        ModCarInfo carInfos = ServerData.Instance.CarPartInfo[carLoaderID];
        int key = partScript.partID;
        int index = partScript.partIdNumber;
        
        switch (partScript.type)
        {
            case ModPartType.engine:
                carInfos.EnginePartsReferences[key] = partScript;
                break;
            case ModPartType.suspension:
                if(!carInfos.SuspensionPartsReferences.ContainsKey(key))
                    carInfos.SuspensionPartsReferences.Add(key, new Dictionary<int, ModPartScript>());

                if (!carInfos.SuspensionPartsReferences[key].ContainsKey(index))
                    carInfos.SuspensionPartsReferences[key].Add(index, partScript);
                else
                    carInfos.SuspensionPartsReferences[key][index] = partScript;
               
                break;
            case ModPartType.other:
                if(!carInfos.OtherPartsReferences.ContainsKey(key))
                    carInfos.OtherPartsReferences.Add(key, new Dictionary<int, ModPartScript>());

                if (!carInfos.OtherPartsReferences[key].ContainsKey(index))
                    carInfos.OtherPartsReferences[key].Add(index, partScript);
                else
                    carInfos.OtherPartsReferences[key][index] = partScript;
                break;
            case ModPartType.driveshaft:
                carInfos.DriveshaftPartsReferences[key] = partScript;
                break;
        }
    }

    public void UpdateBodyParts(ModCarPart carPart, int carLoaderID)
    {
        if(!ServerData.Instance.CarPartInfo.ContainsKey(carLoaderID))
            ServerData.Instance.CarPartInfo.Add(carLoaderID, new ModCarInfo());

        ModCarInfo carInfos = ServerData.Instance.CarPartInfo[carLoaderID];
        carInfos.BodyPartsReferences[carPart.carPartID] = carPart;
    }

    public void ChangePosition(int carLoaderID, int placeNo)
    {
        if(ServerData.Instance.CarPartInfo.TryGetValue(carLoaderID, out ModCarInfo info))
        {
            info.placeNo = placeNo;
        }
    }
}

public class ModCarInfo
{
    public int placeNo;
    
    public Dictionary<int, Dictionary<int,ModPartScript>> OtherPartsReferences = new Dictionary<int,  Dictionary<int,ModPartScript>>();
    public Dictionary<int, Dictionary<int,ModPartScript>> SuspensionPartsReferences  = new Dictionary<int,  Dictionary<int,ModPartScript>>();
    public Dictionary<int, ModPartScript> EnginePartsReferences  = new Dictionary<int, ModPartScript>();
    public Dictionary<int, ModPartScript> DriveshaftPartsReferences  = new Dictionary<int, ModPartScript>();
    public Dictionary<int, ModCarPart> BodyPartsReferences  = new Dictionary<int, ModCarPart>();
}