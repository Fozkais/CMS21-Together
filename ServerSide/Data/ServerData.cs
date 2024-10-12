using System.Collections.Generic;
using System.Linq;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Garage.Car;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using CMS21Together.Shared.Data.Vanilla.Cars;
using CMS21Together.Shared.Data.Vanilla.GarageTool;
using CMS21Together.Shared.Data.Vanilla.Jobs;

namespace CMS21Together.ServerSide.Data;

public class ServerData
{
    public static ServerData Instance;
    
    public Dictionary<int, UserData> connectedClients = new Dictionary<int, UserData>();
    public List<ModItem> items = new List<ModItem>();
    public List<ModGroupItem> groupItems = new List<ModGroupItem>();
    public int money, scrap;

    public GarageTool springClamp = new GarageTool();
    public GarageTool tireChanger = new GarageTool();
    public GarageTool wheelBalancer = new GarageTool();

    public Dictionary<int, ModNewCarData> CarSpawnDatas = new Dictionary<int, ModNewCarData>();
    public Dictionary<int, ModCarInfo> CarPartInfo = new Dictionary<int, ModCarInfo>();

    public Dictionary<string, GarageUpgrade> garageUpgrades = new Dictionary<string, GarageUpgrade>();
    public static Dictionary<ModIOSpecialType, ModCarPlace> toolsPosition = new Dictionary<ModIOSpecialType, ModCarPlace>();
    
    public List<ModJob> jobs = new List<ModJob>();
    public List<ModJob> selectedJobs = new List<ModJob>();


    public void sendCar(int id, int carLoader)
    {
        ServerSend.LoadCarPacket(-1, CarSpawnDatas[id], carLoader);
    }
    public void SetGarageUpgrade(GarageUpgrade upgrade)
    {
        garageUpgrades[upgrade.upgradeID] = upgrade;
    }
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
    public void AddJob(ModJob job)
    {
        jobs.Add(job);
    }
    public void RemoveJob(int jobID)
    {
        ModJob job = jobs.Find(j => j.id == jobID);
        if(job != null)
            jobs.Remove(job);
    }
    public void SetLoadJobCar(ModCar carData)
    {
        if( ServerData.Instance.CarPartInfo.ContainsKey(carData.carLoaderID)) return;
        
        ServerData.Instance.CarPartInfo[carData.carLoaderID] = new ModCarInfo();
        var data = ServerData.Instance.CarPartInfo[carData.carLoaderID];

        data.carToLoad = carData.carID;
        data.carLoaderID = carData.carLoaderID;
        data.configVersion = carData.configVersion;
        data.placeNo = carData.carPosition;
        data.customerCar = carData.customerCar;

    }
    public void UpdateSelectedJobs(ModJob job, bool action)
    {
        if (action)
        {
            if (selectedJobs.All(j => j.id != job.id))
            {
                selectedJobs.Add(job);
            }
        }
        else
        {
            if (selectedJobs.Any(j => j.id == job.id))
            {
                selectedJobs.Remove(selectedJobs.First(j => j.id == job.id));
            }
        }
    }
    public static void ChangeToolPosition(ModIOSpecialType tool, ModCarPlace place)
    {
        toolsPosition[tool] = place;
    }
    public void SetSpringClampState(bool remove, ModGroupItem item)
    {
        if (remove)
        {
            springClamp.isMounted = true;
            springClamp.groupItem = null;
            return;
        }
        
        springClamp.isMounted = false;
        springClamp.groupItem = item;
    }
    public void SetTireChangerState(bool remove, ModGroupItem item)
    {
        if (remove)
        {
            tireChanger.isMounted = true;
            tireChanger.groupItem = null;
            return;
        }
        
        tireChanger.isMounted = false;
        tireChanger.groupItem = item;
    }
    public void SetWheelBalancerState(ModGroupItem item)
    {
        if (item == null)
        {
            wheelBalancer.isMounted = false;
            wheelBalancer.additionalState = false;
            wheelBalancer.groupItem = null;
            return;
        }

        if (!wheelBalancer.additionalState)
        {
            wheelBalancer.groupItem = item;
            wheelBalancer.additionalState = true;
            wheelBalancer.isMounted = true;
            return;
        }

        wheelBalancer.groupItem = item;
        wheelBalancer.additionalState = true;
        wheelBalancer.isMounted = true;
    }

    public void EndJob(ModJob job)
    {
        if (selectedJobs.Any(j => j.id == job.id))
        {
            selectedJobs.Remove(selectedJobs.First(j => j.id == job.id));
        }
    }
}

public class GarageTool
{
    public bool isMounted;
    public bool additionalState;
    public ModGroupItem groupItem;
}

public class ModCarInfo
{
    public int carLoaderID;
    public string carToLoad;
    public int configVersion;
    public bool customerCar;
    public int placeNo;
    
    public Dictionary<int, Dictionary<int,ModPartScript>> OtherPartsReferences = new Dictionary<int,  Dictionary<int,ModPartScript>>();
    public Dictionary<int, Dictionary<int,ModPartScript>> SuspensionPartsReferences  = new Dictionary<int,  Dictionary<int,ModPartScript>>();
    public Dictionary<int, ModPartScript> EnginePartsReferences  = new Dictionary<int, ModPartScript>();
    public Dictionary<int, ModPartScript> DriveshaftPartsReferences  = new Dictionary<int, ModPartScript>();
    public Dictionary<int, ModCarPart> BodyPartsReferences  = new Dictionary<int, ModCarPart>();
}