using System.Collections.Generic;
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
    public Dictionary<int, ModCarPartInfo> CarPartInfos = new Dictionary<int, ModCarPartInfo>();
    
    public void UpdatePartScripts(ModPartScript partScript, int carLoaderID)
    {
        if(!ServerData.Instance.CarPartInfos.ContainsKey(carLoaderID))
            ServerData.Instance.CarPartInfos.Add(carLoaderID, new ModCarPartInfo());

        ModCarPartInfo carPartInfos = ServerData.Instance.CarPartInfos[carLoaderID];
        int key = partScript.partID;
        int index = partScript.partIdNumber;
        
        switch (partScript.type)
        {
            case ModPartType.engine:
                carPartInfos.EnginePartsReferences[key] = partScript;
                break;
            case ModPartType.suspension:
                if(!carPartInfos.SuspensionPartsReferences.ContainsKey(key))
                    carPartInfos.SuspensionPartsReferences.Add(key, new Dictionary<int, ModPartScript>());

                if (!carPartInfos.SuspensionPartsReferences[key].ContainsKey(index))
                    carPartInfos.SuspensionPartsReferences[key].Add(index, partScript);
                else
                    carPartInfos.SuspensionPartsReferences[key][index] = partScript;
               
                break;
            case ModPartType.other:
                if(!carPartInfos.OtherPartsReferences.ContainsKey(key))
                    carPartInfos.OtherPartsReferences.Add(key, new Dictionary<int, ModPartScript>());

                if (!carPartInfos.OtherPartsReferences[key].ContainsKey(index))
                    carPartInfos.OtherPartsReferences[key].Add(index, partScript);
                else
                    carPartInfos.OtherPartsReferences[key][index] = partScript;
                break;
            case ModPartType.driveshaft:
                carPartInfos.DriveshaftPartsReferences[key] = partScript;
                break;
        }
    }

    public void UpdateBodyParts(ModCarPart carPart, int carLoaderID)
    {
        if(!ServerData.Instance.CarPartInfos.ContainsKey(carLoaderID))
            ServerData.Instance.CarPartInfos.Add(carLoaderID, new ModCarPartInfo());

        ModCarPartInfo carPartInfos = ServerData.Instance.CarPartInfos[carLoaderID];
        carPartInfos.BodyPartsReferences[carPart.carPartID] = carPart;
    }
}

public class ModCarPartInfo
{
    public Dictionary<int, Dictionary<int,ModPartScript>> OtherPartsReferences = new Dictionary<int,  Dictionary<int,ModPartScript>>();
    public Dictionary<int, Dictionary<int,ModPartScript>> SuspensionPartsReferences  = new Dictionary<int,  Dictionary<int,ModPartScript>>();
    public Dictionary<int, ModPartScript> EnginePartsReferences  = new Dictionary<int, ModPartScript>();
    public Dictionary<int, ModPartScript> DriveshaftPartsReferences  = new Dictionary<int, ModPartScript>();
    public Dictionary<int, ModCarPart> BodyPartsReferences  = new Dictionary<int, ModCarPart>();
}