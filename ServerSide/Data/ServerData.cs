using System;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Player;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using CMS21Together.Shared.Data.Vanilla.Cars;
using CMS21Together.Shared.Data.Vanilla.GarageTool;
using CMS21Together.Shared.Data.Vanilla.Jobs;
using MelonLoader;

namespace CMS21Together.ServerSide.Data;

public class ServerData
{
	public static ServerData Instance;
	public Dictionary<ModIOSpecialType, ModCarPlace> toolsPosition = new();
	
	public Dictionary<int, ModCarInfo> CarPartInfo = new();
	public Dictionary<int, ModNewCarData> CarSpawnDatas = new();
	public Dictionary<int, ModNewCarData> CarOnPark = new();
	public ModEngineStand engineStand = new(null);
	public ModEngineStand engineStand2 = new(null);
	public float engineStandAngle;
	public float engineStand2Angle;

	public Dictionary<int, UserData> connectedClients = new();

	public Dictionary<string, GarageUpgrade> garageUpgrades = new();
	public List<ModGroupItem> groupItems = new();
	public List<ModItem> items = new();
	public ModWarehouseData warehouseData = new();

	public List<ModJob> jobs = new();
	public int money, scrap;
	public List<ModJob> selectedJobs = new();

	public GarageTool springClamp = new();
	public GarageTool tireChanger = new();
	public GarageTool wheelBalancer = new();

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
		if (carLoaderID == -1 || carLoaderID == -2)
		{
			UpdateEngineStand(partScript, carLoaderID == -2);
			MelonLogger.Msg("received a enginestand part.");
			return;
		}
		
		if (!Instance.CarPartInfo.ContainsKey(carLoaderID))
			Instance.CarPartInfo.Add(carLoaderID, new ModCarInfo());

		var carInfos = Instance.CarPartInfo[carLoaderID];
		var key = partScript.partID;
		var index = partScript.partIdNumber;

		switch (partScript.type)
		{
			case ModPartType.engine:
				carInfos.EnginePartsReferences[key] = partScript;
				break;
			case ModPartType.suspension:
				if (!carInfos.SuspensionPartsReferences.ContainsKey(key))
					carInfos.SuspensionPartsReferences.Add(key, new Dictionary<int, ModPartScript>());

				if (!carInfos.SuspensionPartsReferences[key].ContainsKey(index))
					carInfos.SuspensionPartsReferences[key].Add(index, partScript);
				else
					carInfos.SuspensionPartsReferences[key][index] = partScript;

				break;
			case ModPartType.other:
				if (!carInfos.OtherPartsReferences.ContainsKey(key))
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

	private void UpdateEngineStand(ModPartScript partScript, bool alt)
	{
		if (!alt)
			engineStand.parts[partScript.partID] = partScript;
		else
			engineStand2.parts[partScript.partID] = partScript;
	}

	public void UpdateBodyParts(ModCarPart carPart, int carLoaderID)
	{
		if (!Instance.CarPartInfo.ContainsKey(carLoaderID))
			Instance.CarPartInfo.Add(carLoaderID, new ModCarInfo());

		var carInfos = Instance.CarPartInfo[carLoaderID];
		carInfos.BodyPartsReferences[carPart.carPartID] = carPart;
	}

	public void ChangePosition(int carLoaderID, int placeNo)
	{
		if (Instance.CarPartInfo.TryGetValue(carLoaderID, out var info)) info.placeNo = placeNo;
		if (Instance.CarSpawnDatas.TryGetValue(carLoaderID, out var info2)) info2.carPosition = placeNo;
	}

	public void AddJob(ModJob job)
	{
		jobs.Add(job);
	}

	public void RemoveJob(int jobID)
	{
		var job = jobs.Find(j => j.id == jobID);
		if (job != null)
			jobs.Remove(job);
	}

	public void SetLoadJobCar(ModCar carData)
	{
		if (Instance.CarPartInfo.ContainsKey(carData.carLoaderID)) return;

		Instance.CarPartInfo[carData.carLoaderID] = new ModCarInfo();
		ModCarInfo data = Instance.CarPartInfo[carData.carLoaderID];

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
			if (selectedJobs.All(j => j.id != job.id)) selectedJobs.Add(job);
		}
		else
		{
			if (selectedJobs.Any(j => j.id == job.id)) selectedJobs.Remove(selectedJobs.First(j => j.id == job.id));
		}
	}

	public void ChangeToolPosition(ModIOSpecialType tool, ModCarPlace place)
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
		if (selectedJobs.Any(j => j.id == job.id)) selectedJobs.Remove(selectedJobs.First(j => j.id == job.id));
	}

	public void UpdateFluid(ModFluidData fluid, int carLoaderID)
	{
		//MelonLogger.Msg("Not implemented..."); TODO: Implement this
	}

	public void SetEngineOnStand(ModGroupItem engineGroup, Vector3Serializable position, bool alt)
	{
		if (!alt)
		{
			engineStand = new ModEngineStand(null);
			engineStand.engineGroupItem = engineGroup;
			engineStand.position = position;
		}
		else
		{
			engineStand2 = new ModEngineStand(null);
			engineStand2.engineGroupItem = engineGroup;
			engineStand2.position = position;
		}
	}

	public void ClearEngineFromStand(bool alt)
	{
		MelonLogger.Msg("SV: Clear engine.");
		if (!alt)
			engineStand = new ModEngineStand(null);
		else
			engineStand2 = new ModEngineStand(null);
	}

	public void IncreaseStandAngle(float val, bool alt)
	{
		if (!alt)
			engineStandAngle = val;
		else
			engineStand2Angle = val;
	}

	public void SetPlayerInfo(int id, PlayerInfo info)
	{
		connectedClients[id].playerExp = info.playerExp;
		connectedClients[id].playerLevel = info.playerLevel;
		connectedClients[id].playerSkillPoints = info.skillPoints;
		connectedClients[id].position = info.position;
		connectedClients[id].rotation = info.rotation;
	}

	public void SetCarColor(ModColor color)
	{
		foreach (ModCarInfo car in CarPartInfo.Values)
		{
			if (car.placeNo == 5)
			{
				CarSpawnDatas[car.carLoaderID].color = color;
			}
		}
	}

	public void UpdatePartInfo(ModPartInfo info, bool isBody, bool success)
	{
		if (items.Any(i => i.UID == info.Item.UID))
		{
			ModItem item = items.First(i => i.UID == info.Item.UID);
			item.Condition = (success ? info.SuccessCondition : info.FailCondition);
			item.RepairAmount++;
			if (isBody)
			{
				item.Dent = (success ? info.DentSuccessCondition : info.DentFailCondition);
			}
		}
	}
	
	public void SetCarWash(int loaderID, bool interior)
	{
		if (!CarPartInfo.TryGetValue(loaderID, out var car))
			return;

		if (interior)
		{
			foreach (KeyValuePair<int, ModCarPart> part in car.BodyPartsReferences)
			{
				if (!part.Value.unmounted)
				{
						part.Value.washFactor = 1;
						part.Value.Dust = 0;
					
				}
			}
			ModCarPart detailsPart = GetCarPart(loaderID, "details");
			if (detailsPart != null)
			{
				detailsPart.Dust = 0;
				ModCarPart details2 = GetCarPart(loaderID, "details2");
				if (details2 != null) details2.Dust = 0;
				ModCarPart details3 = GetCarPart(loaderID, "details3");
				if (details3 != null) details3.Dust = 0;
			}
		}
		else
		{
			string[] interiorParts = { "benchFront", "bench", "steeringWheel", "seatLeft", "seatRight", "details", "details2", "details3" };
			foreach (string partName in interiorParts)
			{
				ModCarPart part = GetCarPart(loaderID, partName);
				if (part != null && !part.unmounted)
				{
					part.condition = 1;
					part.Dust = 0;
					if (partName == "details")
					{
						ModCarPart details2 = GetCarPart(loaderID, "details2");
						if (details2 != null) details2.Dust = 0;

						ModCarPart details3 = GetCarPart(loaderID, "details3");
						if (details3 != null) details3.Dust = 0;
					}
				}
			}
		}
	}

	private ModCarPart GetCarPart(int loaderID, string partName)
	{
		return CarPartInfo[loaderID].BodyPartsReferences
			.Values.FirstOrDefault(p => p.name == partName);
	}

	public void SetWelder(int loaderID)
	{
		if (!CarPartInfo.ContainsKey(loaderID))
			return;
		
		ModCarPart part = GetCarPart(loaderID, "body");
		part.condition = 1;
		part.dent = 1;
		ModCarPart part2 = GetCarPart(loaderID, "details");
		part2.dent = 1;
	}

	public void AddCarToPark(ModNewCarData car, int index)
	{
		CarOnPark.Add(index, car);
	}

	public void RemoveCarFromPark(int index)
	{
		if (CarOnPark.ContainsKey(index))
			CarOnPark.Remove(index);
	}
}


public class GarageTool
{
	public bool additionalState;
	public ModGroupItem groupItem;
	public bool isMounted;
}

public class ModCarInfo
{
	public int carLoaderID;
	public string carToLoad;
	public int configVersion;
	public bool customerCar;
	public int placeNo;
	public Dictionary<int, ModCarPart> BodyPartsReferences = new();
	public Dictionary<int, ModPartScript> DriveshaftPartsReferences = new();
	public Dictionary<int, ModPartScript> EnginePartsReferences = new();
	public Dictionary<int, Dictionary<int, ModPartScript>> OtherPartsReferences = new();
	public Dictionary<int, Dictionary<int, ModPartScript>> SuspensionPartsReferences = new();
}
