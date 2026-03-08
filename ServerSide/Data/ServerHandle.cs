using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Garage.Tools;
using CMS21Together.ClientSide.Data.Player;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using CMS21Together.Shared.Data.Vanilla.Cars;
using CMS21Together.Shared.Data.Vanilla.GarageTool;
using CMS21Together.Shared.Data.Vanilla.Jobs;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ServerSide.Data;

public static class ServerHandle
{
	public static void ConnectValidationPacket(int fromClient, Packet packet)
	{
		var clientIdCheck = packet.ReadInt();
		var username = packet.Read<string>();
		var content = packet.Read<ReadOnlyDictionary<string, bool>>();
		var gameVersion = packet.Read<string>();
		var modVersion = packet.Read<string>();

		//MelonLogger.Msg($"[ServerHandle->ConnectValidationPacket] Received info : {clientIdCheck},{username},{modVersion}");

		if (modVersion != MainMod.ASSEMBLY_MOD_VERSION)
		{
			ServerSend.DisconnectPacket(fromClient, $"Server mod version is on {MainMod.ASSEMBLY_MOD_VERSION}.");
			return;
		}

		if (gameVersion != ContentManager.Instance.gameVersion)
		{
			ServerSend.DisconnectPacket(fromClient, $"Server is on game version : {ContentManager.Instance.gameVersion}.");
			return;
		}

		var a = ApiCalls.API_M1(content, ContentManager.Instance.ownedContents);
		ServerSend.ContentInfoPacket(new ReadOnlyDictionary<string, bool>(a));

		if (fromClient != clientIdCheck)
		{
			ServerSend.DisconnectPacket(fromClient, "Error on ClientIdCheck.");
			return;
		}

		if (ServerData.Instance.connectedClients.Any(c => c.Value.username == username))
		{
			ServerSend.DisconnectPacket(fromClient, "A user with same name is already connected.");
			return;
		}
		
		MelonLogger.Msg($"[ServerHandle->ConnectValidationPacket] {username} connected successfully.");
		Server.Instance.clients[fromClient].SendToLobby(username);
		
		if (SaveSystem.Extensions[SaveSystem.selectedSaveIndex].PlayerInfos.Any(s => s.id == username))
			ServerSend.PlayerSpawnPacket(clientIdCheck, SaveSystem.Extensions[SaveSystem.selectedSaveIndex].PlayerInfos.First(s => s.id == username));
		else
			SaveSystem.Extensions[SaveSystem.selectedSaveIndex].PlayerInfos.Add(new PlayerInfo(username,Vector3.zero, Quaternion.identity, 0,1, 0));
	}

	public static void DisconnectPacket(int fromclient, Packet packet)
	{
		if (ServerData.Instance.connectedClients.ContainsKey(fromclient))
		{
			MelonLogger.Msg($"[ServerHandle->DisconnectPacket] {ServerData.Instance.connectedClients[fromclient].username} as disconnected from server.");
			Server.Instance.clients[fromclient].Disconnect();
		}
		else
			MelonLogger.Msg($"[ServerHandle->DisconnectPacket] a unknown client with id:{fromclient} as disconnected from server.");
	}

	public static void ReadyPacket(int fromClient, Packet packet)
	{
		var id = packet.ReadInt();
		var ready = packet.Read<bool>();

		ServerData.Instance.connectedClients[id].isReady = ready;

		ServerSend.ReadyPacket(fromClient, ready, id);
	}
	
	public static void SkillChangePacket(int fromClient, Packet packet)
	{
		string playerID = packet.Read<string>();
		string id = packet.Read<string>();
		List<bool> skill = packet.Read<List<bool>>();

		SaveSystem.Extensions[SaveSystem.selectedSaveIndex].PlayerInfos.First(p => playerID == p.id).UpdateSkill(id, skill);
	}

	public static void PositionPacket(int fromClient, Packet packet)
	{
		var _position = packet.Read<Vector3Serializable>();
		ServerData.Instance.connectedClients[fromClient].position = _position;

		ServerSend.PositionPacket(fromClient, _position);
	}

	public static void RotationPacket(int fromClient, Packet packet)
	{
		var _rotation = packet.Read<QuaternionSerializable>();
		ServerData.Instance.connectedClients[fromClient].rotation = _rotation;

		ServerSend.RotationPacket(fromClient, _rotation);
	}

	public static void ItemPacket(int fromClient, Packet _packet)
	{
		var action = _packet.Read<InventoryAction>();

		if (action != InventoryAction.resync)
		{
			var item = _packet.Read<ModItem>();

			// Security Rule: Validate item is not null
			if (item == null)
			{
				MelonLogger.Warning($"[ServerHandle->ItemPacket] Received null item from client {fromClient}, skipping.");
				return;
			}

			// Security Rule: Validate ServerData is initialized
			if (ServerData.Instance == null || ServerData.Instance.items == null)
			{
				MelonLogger.Error("[ServerHandle->ItemPacket] ServerData.Instance or items is null!");
				return;
			}

			if (action == InventoryAction.add)
			{
				// Business Rule: Check if item already exists to prevent duplication
				if (ServerData.Instance.items.All(i => i.UID != item.UID))
				{
					// Business Logic: Increment UID counter and add item to server inventory
					ServerData.Instance.items.Add(item);
					
					// Business Logic: Broadcast item to all clients (including sender for confirmation)
					ServerSend.ItemPacket(fromClient, item, action);
					MelonLogger.Msg($"[ServerHandle->ItemPacket] Added item UID: {item.UID} from client {fromClient}");
				}
				else
				{
					// Security Rule: Log duplicate attempt but don't add
					MelonLogger.Msg($"[ServerHandle->ItemPacket] Item with UID {item.UID} already exists, skipping add to prevent duplication.");
				}
			}
			else if (action == InventoryAction.remove)
			{
				// Business Rule: Remove item if it exists
				if (ServerData.Instance.items.Any(s => s.UID == item.UID))
				{
					var index = ServerData.Instance.items.FindIndex(s => s.UID == item.UID);
					ServerData.Instance.items.Remove(ServerData.Instance.items[index]);
					
					// Business Logic: Broadcast removal to all clients
					ServerSend.ItemPacket(fromClient, item, action);
					MelonLogger.Msg($"[ServerHandle->ItemPacket] Removed item UID: {item.UID} from client {fromClient}");
				}
				else
				{
					MelonLogger.Msg($"[ServerHandle->ItemPacket] Item with UID {item.UID} not found for removal from client {fromClient}");
				}
			}
			return;
		}

		// Business Logic: Resync - send all items to requesting client
		MelonLogger.Msg($"[ServerHandle->ItemPacket] Resyncing {ServerData.Instance.items.Count} items to client {fromClient}");
		foreach (var modItem in ServerData.Instance.items) ServerSend.ItemPacket(fromClient, modItem, action);
	}

	public static void GroupItemPacket(int fromClient, Packet _packet)
	{
		var action = _packet.Read<InventoryAction>();

		if (action != InventoryAction.resync)
		{
			var item = _packet.Read<ModGroupItem>();

			// Security Rule: Validate item is not null
			if (item == null)
			{
				MelonLogger.Warning($"[ServerHandle->GroupItemPacket] Received null group item from client {fromClient}, skipping.");
				return;
			}

			// Security Rule: Validate ServerData is initialized
			if (ServerData.Instance == null || ServerData.Instance.groupItems == null)
			{
				MelonLogger.Error("[ServerHandle->GroupItemPacket] ServerData.Instance or groupItems is null!");
				return;
			}

			if (action == InventoryAction.add)
			{
				// Business Rule: Check if group item already exists to prevent duplication
				if (ServerData.Instance.groupItems.All(i => i.UID != item.UID))
				{
					// Business Logic: Add group item to server inventory
					ServerData.Instance.groupItems.Add(item);
					
					// Business Logic: Broadcast group item to all clients (including sender for confirmation)
					ServerSend.GroupItemPacket(fromClient, item, action);
					MelonLogger.Msg($"[ServerHandle->GroupItemPacket] Added group item UID: {item.UID} from client {fromClient}");
				}
				else
				{
					// Security Rule: Log duplicate attempt but don't add
					MelonLogger.Msg($"[ServerHandle->GroupItemPacket] Group item with UID {item.UID} already exists, skipping add to prevent duplication.");
				}
			}
			else if (action == InventoryAction.remove)
			{
				// Business Rule: Remove group item if it exists
				if (ServerData.Instance.groupItems.Any(s => s.UID == item.UID))
				{
					var index = ServerData.Instance.groupItems.FindIndex(s => s.UID == item.UID);
					ServerData.Instance.groupItems.Remove(ServerData.Instance.groupItems[index]);
					
					// Business Logic: Broadcast removal to all clients
					ServerSend.GroupItemPacket(fromClient, item, action);
					MelonLogger.Msg($"[ServerHandle->GroupItemPacket] Removed group item UID: {item.UID} from client {fromClient}");
				}
				else
				{
					MelonLogger.Msg($"[ServerHandle->GroupItemPacket] Group item with UID {item.UID} not found for removal from client {fromClient}");
				}
			}
			return;
		}

		// Business Logic: Resync - send all group items to requesting client
		MelonLogger.Msg($"[ServerHandle->GroupItemPacket] Resyncing {ServerData.Instance.groupItems.Count} group items to client {fromClient}");
		foreach (var modItem in ServerData.Instance.groupItems) ServerSend.GroupItemPacket(fromClient, modItem, action);
	}

	public static void StatPacket(int fromClient, Packet packet)
	{
		var value = packet.ReadInt();
		var type = packet.Read<ModStats>();
		var initial = packet.Read<bool>();

		if (initial)
			switch (type)
			{
				case ModStats.money:
					ServerData.Instance.money = value;
					break;
				case ModStats.scrap:
					ServerData.Instance.scrap = value;
					break;
			}
		else
			switch (type)
			{
				case ModStats.money:
					ServerData.Instance.money += value;
					break;
				case ModStats.scrap:
					ServerData.Instance.scrap += value;
					break;
			}

		ServerSend.StatPacket(fromClient, value, type, initial);
	}
	
	public static void ExpPacket(int fromClient, Packet packet)
	{
		int exp = packet.ReadInt();
		int lvl = packet.ReadInt();
		
		//MelonLogger.Msg($"Received XP Packet : {GlobalData.PlayerExp} , {GlobalData.PlayerLevel}");
		ServerData.Instance.connectedClients[fromClient].playerExp = exp;
		ServerData.Instance.connectedClients[fromClient].playerLevel = lvl;
	}
	
	public static void PointPacket(int fromClient, Packet packet)
	{
		int points = packet.ReadInt();
		
		//MelonLogger.Msg($"Received PointPacket Packet : {points}");
		ServerData.Instance.connectedClients[fromClient].playerSkillPoints = points;
	}

	public static void LifterPacket(int fromClient, Packet packet)
	{
		var state = packet.Read<ModLifterState>();
		var carLoaderID = packet.ReadInt();

		ServerSend.LifterPacket(fromClient, state, carLoaderID);
	}

	public static void LoadJobCarPacket(int fromClient, Packet packet)
	{
		var carData = packet.Read<ModCar>();
		ServerData.Instance.SetLoadJobCar(carData);
	}

	public static void LoadCarPacket(int fromClient, Packet packet)
	{
		var carData = packet.Read<ModNewCarData>();
		var carLoaderID = packet.ReadInt();

		ServerData.Instance.CarSpawnDatas[carLoaderID] = carData;
		ServerData.Instance.SetLoadJobCar(new ModCar(carLoaderID, carData.carToLoad, carData.configVersion, carData.carPosition, carData.customerCar));

		ServerSend.LoadCarPacket(fromClient, carData, carLoaderID);
	}

	public static void BodyPartPacket(int fromClient, Packet packet)
	{
		var carPart = packet.Read<ModCarPart>();
		var carLoaderID = packet.ReadInt();

		ServerData.Instance.UpdateBodyParts(carPart, carLoaderID);
		ServerSend.BodyPartPacket(fromClient, carPart, carLoaderID);
	}

	public static void PartScriptPacket(int fromClient, Packet packet)
	{
		var partScript = packet.Read<ModPartScript>();
		var carLoaderID = packet.ReadInt();

		ServerData.Instance.UpdatePartScripts(partScript, carLoaderID);
		ServerSend.PartScriptPacket(fromClient, partScript, carLoaderID);
	}
	
	public static void CarFluidPacket(int fromClient, Packet packet)
	{
		var carLoaderID = packet.ReadInt();
		ModFluidData fluid = packet.Read<ModFluidData>();
		
		ServerData.Instance.UpdateFluid(fluid, carLoaderID);
		ServerSend.CarFluidPacket(fromClient, carLoaderID, fluid);
	}

	public static void DeleteCarPacket(int fromClient, Packet packet)
	{
		var carLoaderID = packet.ReadInt();

		ServerData.Instance.DeleteCar(carLoaderID);
		ServerSend.DeleteCarPacket(fromClient, carLoaderID);
	}

	public static void CarPositionPacket(int fromClient, Packet packet)
	{
		var placeNo = packet.ReadInt();
		var carLoaderID = packet.ReadInt();

		ServerData.Instance.ChangePosition(carLoaderID, placeNo);
		ServerSend.CarPositionPacket(fromClient, carLoaderID, placeNo);
	}

	public static void GarageUpgradePacket(int fromClient, Packet packet)
	{
		var upgrade = packet.Read<GarageUpgrade>();

		ServerData.Instance.SetGarageUpgrade(upgrade);
		ServerSend.GarageUpgradePacket(fromClient, upgrade);
	}

	public static void JobPacket(int fromClient, Packet packet)
	{
		var job = packet.Read<ModJob>();

		//MelonLogger.Msg("SV: Received JobPacket!");
		ServerData.Instance.AddJob(job);
		ServerSend.JobPacket(fromClient, job);
	}

	public static void JobActionPacket(int fromClient, Packet packet)
	{
		ModJob job = packet.Read<ModJob>();
		var takeJob = packet.Read<bool>();

		//MelonLogger.Msg("SV: Received JobAction!");
		ServerData.Instance.RemoveJob(job.id);
		ServerSend.JobActionPacket(fromClient, job, takeJob);
	}

	public static void SelectedJobPacket(int fromClient, Packet packet)
	{
		var job = packet.Read<ModJob>();
		var action = packet.Read<bool>();
		
		//MelonLogger.Msg("cl: Received SelectedJobPacket!");
		ServerData.Instance.UpdateSelectedJobs(job, action);
		ServerSend.SelectedJobPacket(fromClient, job, action);
	}

	public static void EndJobPacket(int fromClient, Packet packet)
	{
		var job = packet.Read<ModJob>();

		//MelonLogger.Msg("SV : endjob !");
		ServerData.Instance.EndJob(job);
		ServerSend.EndJobPacket(fromClient, job);
	}

	public static void SceneChangePacket(int fromClient, Packet packet)
	{
		var scene = packet.Read<GameScene>();

		ServerData.Instance.connectedClients[fromClient].scene = scene;
		ServerSend.SceneChangePacket(fromClient, scene);
	}

	public static void ToolsMovePacket(int _fromClient, Packet _packet)
	{
		var tool = _packet.Read<ModIOSpecialType>();
		var place = _packet.Read<ModCarPlace>();
		var playSound = _packet.Read<bool>();

		ServerData.Instance.ChangeToolPosition(tool, place);

		ServerSend.ToolsMovePacket(_fromClient, tool, place, playSound);
	}
	

	public static void SetSpringClampPacket(int fromClient, Packet packet)
	{
		var item = packet.Read<ModGroupItem>();
		var instant = packet.Read<bool>();
		var mount = packet.Read<bool>();

		ServerData.Instance.SetSpringClampState(false, item);
		ServerSend.SetSpringClampPacket(fromClient, item, instant, mount);
	}

	public static void SpringClampClearPacket(int fromClient, Packet packet)
	{
		ServerData.Instance.SetSpringClampState(true, null);
		ServerSend.SpringClampClearPacket(fromClient);
	}

	public static void SetTireChangerPacket(int fromClient, Packet packet)
	{
		var item = packet.Read<ModGroupItem>();
		var instant = packet.Read<bool>();
		var connect = packet.Read<bool>();

		ServerData.Instance.SetTireChangerState(false, item);
		ServerSend.SetTireChangerPacket(fromClient, item, instant, connect);
	}

	public static void ClearTireChangerPacket(int fromClient, Packet packet)
	{
		ServerData.Instance.SetTireChangerState(true, null);
		ServerSend.ClearTireChangerPacket(fromClient);
	}
	
	public static void OilBin(int _fromclient, Packet _packet)
	{
		int loaderID = _packet.ReadInt();

		ServerSend.SendOilBin(_fromclient, loaderID);
	}

	public static void WheelBalancePacket(int fromClient, Packet packet)
	{
		int aType = packet.ReadInt();
		ModGroupItem item;
                
		if ((ModWheelBalancerActionType)aType == ModWheelBalancerActionType.start ||(ModWheelBalancerActionType)aType == ModWheelBalancerActionType.setGroup)
		{
			item = packet.Read<ModGroupItem>();
			ServerSend.WheelBalancerPacket(fromClient, (ModWheelBalancerActionType)aType, item);
			return;
		}
		ServerSend.WheelBalancerPacket(fromClient, (ModWheelBalancerActionType)aType);
	}
	

	public static void EngineCraneHandlePacket(int fromClient, Packet packet)
	{
		int action = packet.ReadInt();
		int carLoaderID = packet.ReadInt();
		ModGroupItem item;
		if (action == 1)
		{
			item = packet.Read<ModGroupItem>();
			
			ServerSend.EngineCraneHandlePacket(fromClient, action, carLoaderID, item);			
			return;
		}
		ServerSend.EngineCraneHandlePacket(fromClient, action, carLoaderID);
	}
	public static void EngineStandSetGroupPacket(int fromClient, Packet packet)
	{
		ModGroupItem engineGroup = packet.Read<ModGroupItem>();
		Vector3Serializable position = packet.Read<Vector3Serializable>();
		bool alt = packet.Read<bool>();

		MelonLogger.Msg("SV: received new engine");
		ServerData.Instance.SetEngineOnStand(engineGroup, position, alt);
		ServerSend.EngineStandSetGroupPacket(fromClient, engineGroup, position, alt);
	}
	
	
	public static void EngineStandTakeOffPacket(int fromClient, Packet packet)
	{
		bool alt = packet.Read<bool>();
		
		ServerData.Instance.ClearEngineFromStand(alt);
		ServerSend.EngineStandTakeOffPacket(fromClient, alt);
	}	
	public static void EngineStandAnglePacket(int fromClient, Packet packet)
	{
		float val = packet.Read<float>();
		bool alt = packet.Read<bool>();
		
		ServerData.Instance.IncreaseStandAngle(val, alt);
		ServerSend.IncreaseStandAnglePacket(fromClient, val, alt);
	}
	
	public static void CarWashPacket(int fromClient, Packet packet)
	{
		int loaderID = packet.ReadInt();
		bool interior = packet.Read<bool>();
		
		ServerData.Instance.SetCarWash(loaderID, interior); 
		ServerSend.CarWashPacket(fromClient, loaderID, interior);
	}
	
	public static void WelderPacket(int fromClient, Packet packet)
	{
		int loaderID = packet.ReadInt();
		
		ServerData.Instance.SetWelder(loaderID);
		ServerSend.WelderPacket(fromClient, loaderID);
	}
	
	public static void CarPaintPacket(int fromClient, Packet packet)
	{
		ModColor color = packet.Read<ModColor>();
		
		ServerData.Instance.SetCarColor(color);
		ServerSend.CarPaintPacket(fromClient, color);
	}
	
	public static void AddCarToParkPacket(int fromClient, Packet packet)
	{
		ModNewCarData car = packet.Read<ModNewCarData>();
		int index = packet.ReadInt();
		
		ServerData.Instance.AddCarToPark(car, index);
		ServerSend.AddCarToParkPacket(fromClient, car, index);
	}
	
	public static void RemoveCarFromParkPacket(int fromClient, Packet packet)
	{
		int index = packet.ReadInt();
		
		ServerData.Instance.RemoveCarFromPark(index);
		ServerSend.RemoveCarFromParkPacket(fromClient, index);
	}
	
	public static void RepairPartPacket(int fromClient, Packet packet)
	{
		ModPartInfo info = packet.Read<ModPartInfo>();
		bool isBody = packet.Read<bool>();
		bool success = packet.Read<bool>();
		
		ServerData.Instance.UpdatePartInfo(info, isBody, success);
		ServerSend.RepairPartPacket(fromClient, info, isBody, success);
	}
	
	public static void ResyncPacket(int fromClient, Packet packet)
	{
		PacketTypes resyncType = packet.Read<PacketTypes>();

		switch (resyncType)
		{
			case PacketTypes.loadCar:
				int carLoaderID = packet.ReadInt();
				ServerResyncs.ResyncCar(fromClient, carLoaderID);
				break;
			case PacketTypes.parkAdd:
				ServerResyncs.ResyncPark(fromClient);
				break;
			case PacketTypes.toolMove:
				ServerResyncs.ResyncTools(fromClient);
				break;
			case PacketTypes.garageUpgrade:
				ServerResyncs.ResyncUpgrade(fromClient);
				break;
			case PacketTypes.engineStandSetGroup:
				bool alt = packet.Read<bool>();
				ServerResyncs.ResyncEngineStand(fromClient, alt);
				break;
			case PacketTypes.playerInCar:
				PlayerInCarPacket(fromClient, packet);
				break;
			case PacketTypes.carEngineSound:
				CarEngineSoundPacket(fromClient, packet);
				break;
		}
	}

	public static void PlayerInCarPacket(int fromClient, Packet _packet)
	{
		var isInCar = _packet.Read<bool>();
		var carLoaderID = _packet.ReadInt();
		
		// Business Logic: Update player's car state on server
		if (ServerData.Instance.connectedClients.ContainsKey(fromClient))
		{
			ServerData.Instance.connectedClients[fromClient].isInCar = isInCar;
			ServerData.Instance.connectedClients[fromClient].carLoaderID = carLoaderID;
			MelonLogger.Msg($"[ServerHandle->PlayerInCarPacket] Player {fromClient} car state: InCar={isInCar}, CarLoaderID={carLoaderID}");
		}
		
		// Business Logic: Broadcast to all clients
		ServerSend.PlayerInCarPacket(fromClient, isInCar, carLoaderID);
	}

	public static void CarEngineSoundPacket(int fromClient, Packet _packet)
	{
		var carLoaderID = _packet.ReadInt();
		var isPlaying = _packet.Read<bool>();
		var rpm = _packet.Read<float>();
		
		// Business Logic: Broadcast engine sound state to all clients
		ServerSend.CarEngineSoundPacket(fromClient, carLoaderID, isPlaying, rpm);
	}
}