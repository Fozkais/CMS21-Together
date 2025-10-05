using System.Collections.Generic;
using System.Collections.ObjectModel;
using CMS21Together.ClientSide.Data.CustomUI;
using CMS21Together.ClientSide.Data.Garage.Campaign;
using CMS21Together.ClientSide.Data.Garage.Car;
using CMS21Together.ClientSide.Data.Garage.Tools;
using CMS21Together.ClientSide.Data.NewUI;
using CMS21Together.ClientSide.Data.Player;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using CMS21Together.Shared.Data.Vanilla.Cars;
using CMS21Together.Shared.Data.Vanilla.GarageTool;
using CMS21Together.Shared.Data.Vanilla.Jobs;
using MelonLoader;

namespace CMS21Together.ClientSide.Data.Handle;

public static class ClientHandle
{
	public static void ConnectPacket(Packet packet)
	{
		var message = packet.Read<string>();
		var newID = packet.ReadInt();

		MelonLogger.Msg($"[ClientHandle->ConnectPacket] {message}");
		ClientData.UserData.playerID = newID;

		if (Client.Instance.networkType == NetworkType.TCP)
			Client.Instance.udp.Connect();

		Client.Instance.OnConnectedInvoke();
		ClientSend.ConnectValidationPacket();
	}

	public static void DisconnectPacket(Packet packet)
	{
		var message = packet.Read<string>();

		MelonLogger.Msg($"[ClientHandle->DisconnectPacket] You've been disconnected from server: {message}");
		if (ClientData.UserData.scene == GameScene.menu)
			UICustomPanel.CreateInfoPanel($"You've been disconnected from server: {message}");
		Client.Instance.Disconnect(true);
	}

	public static void UserDataPacket(Packet packet)
	{
		var data = packet.Read<UserData>();

		ClientData.Instance.connectedClients[data.playerID] = data;
		UILobby.RefreshPlayers();
		//MelonLogger.Msg("[ClientHandle->UserDataPacket] Receive userData from server.");
	}

	public static void ContentsInfoPacket(Packet _packet)
	{
		var infos = _packet.Read<ReadOnlyDictionary<string, bool>>();
		ApiCalls.API_M2(infos);
	}

	public static void ReadyPacket(Packet packet)
	{
		var id = packet.ReadInt();
		var ready = packet.Read<bool>();

		ClientData.Instance.connectedClients[id].isReady = ready;
		UILobby.RefreshPlayers();
	}

	public static void StartPacket(Packet packet)
	{
		var gamemode = packet.Read<Gamemode>();
		var parkCars = packet.Read<Dictionary<int, ModNewCarData>>();

		var data = new ModSaveData();
		data.selectedGamemode = gamemode;

		SavesManager.LoadSave(data, parkCars,true);
	}
	
	public static void SpawnPacket(Packet packet)
	{
		int playerMoney = packet.ReadInt();
		int playerExp = packet.ReadInt();
		int playerLevel = packet.ReadInt();
		int skillPoints = packet.ReadInt();
		Vector3Serializable position = packet.Read<Vector3Serializable>();
		QuaternionSerializable rotation = packet.Read<QuaternionSerializable>();
		Dictionary<string, List<bool>> skills = packet.Read<Dictionary<string, List<bool>>>();
		long startItemUID = packet.Read<long>();
		int missionFinished = packet.ReadInt();
		bool missionInProgress = packet.Read<bool>();
		
		MelonCoroutines.Start(ClientData.Instance.SpawnPlayer(playerMoney, playerExp, playerLevel, position.toVector3(),
			rotation.toQuaternion(), skillPoints, skills, startItemUID, missionFinished, missionInProgress));
	}

	public static void PositionPacket(Packet packet)
	{
		var id = packet.ReadInt();
		var position = packet.Read<Vector3Serializable>();
		Movement.UpdatePosition(id, position);
		packet.Dispose();
	}

	public static void RotationPacket(Packet packet)
	{
		var id = packet.ReadInt();
		var rotation = packet.Read<QuaternionSerializable>();
		Rotation.UpdateRotation(id, rotation);
		packet.Dispose();
	}

	public static void ItemPacket(Packet packet)
	{
		var action = packet.Read<InventoryAction>();
		var item = packet.Read<ModItem>();

		MelonCoroutines.Start(Player.Inventory.HandleItem(item, action));
		packet.Dispose();
	}

	public static void GroupItemPacket(Packet packet)
	{
		var action = packet.Read<InventoryAction>();
		var item = packet.Read<ModGroupItem>();

		MelonCoroutines.Start(Player.Inventory.HandleGroupItem(item, action));
		packet.Dispose();
	}

	public static void StatPacket(Packet packet)
	{
		var value = packet.ReadInt();
		var type = packet.Read<ModStats>();
		var initial = packet.Read<bool>();

		//MelonLogger.Msg($"Received stat:{value} , {type.ToString()}");
		MelonCoroutines.Start(Stats.UpdateStats(type, value, initial));
	}

	public static void LifterPacket(Packet packet)
	{
		if (SceneManager.CurrentScene() != GameScene.garage) return;

		var state = packet.Read<ModLifterState>();
		var carLoaderID = packet.ReadInt();
		if (!ClientData.Instance.loadedCars.ContainsKey(carLoaderID)) return;
		var lifter = GameData.Instance.carLoaders[carLoaderID].lifter;
		LifterLogic.listen = false;

		if ((int)state > (int)lifter.currentState)
			lifter.Action(0);
		else
			lifter.Action(1);

		// ClientData.Instance.loadedCars[carLoaderID - 1].CarLifterState = (int)state; TODO: fix this?
	}

	public static void SetTireChangerPacket(Packet packet)
	{
		var item = packet.Read<ModGroupItem>();
		var instant = packet.Read<bool>();
		var connect = packet.Read<bool>();

		Garage.Tools.TireChangerLogic.listen = false;
		GameData.Instance.tireChanger.SetGroupOnTireChanger(item.ToGame(), instant, connect);
	}

	public static void ClearTireChangerPacket(Packet packet)
	{
		GameData.Instance.tireChanger.ResetActions();
	}
	public static void WheelBalancePacket(Packet packet)
	{
		if(SceneManager.CurrentScene() != GameScene.garage) return;
                
		ModWheelBalancerActionType aType = packet.Read<ModWheelBalancerActionType>();
		ModGroupItem _item = null;
		if(aType == ModWheelBalancerActionType.start || aType == ModWheelBalancerActionType.setGroup)
			_item = packet.Read<ModGroupItem>();

		if (aType == ModWheelBalancerActionType.remove)
		{
			GameData.Instance.wheelBalancer.ResetActions();
			GameData.Instance.wheelBalancer.Clear();
		}
		else
		{
			WheelBalancer.listen = false;
			//MelonLogger.Msg("CL: Received WheelBalance!");
			GameData.Instance.wheelBalancer.SetGroupOnWheelBalancer(_item!.ToGame(_item), true);
		}
	}
	
	public static void OilBinPacket(Packet _packet)
	{
		int carLoaderID = _packet.ReadInt();

		if(SceneManager.CurrentScene() != GameScene.garage) return;
                
		OilBin.listen = false;
		GameData.Instance.carLoaders[carLoaderID].UseOilbin();
	}
	
	public static void WelderPacket(Packet _packet)
	{
		int carLoaderID = _packet.ReadInt();

		MelonCoroutines.Start(Garage.Tools.WelderLogic.UseWelder(carLoaderID));
	}
	
	public static void SetSpringClampPacket(Packet packet)
	{
		var item = packet.Read<ModGroupItem>();
		var instant = packet.Read<bool>();
		var mount = packet.Read<bool>();

		MelonCoroutines.Start(Garage.Tools.SpringClampLogic.Action(item, instant, mount));
	}

	public static void SpringClampClearPacket(Packet packet)
	{
		if (GameData.Instance.springClampLogic.GroupOnSpringClamp != null)
			if (GameData.Instance.springClampLogic.GroupOnSpringClamp.ItemList != null)
			{
				GameData.Instance.springClampLogic.GroupOnSpringClamp.ItemList.Clear();
				Garage.Tools.SpringClampLogic.listen = false;
				GameData.Instance.springClampLogic.ClearSpringClamp();
			}
	}

	public static void ToolsMovePacket(Packet _packet)
	{
		var tool = _packet.Read<ModIOSpecialType>();
		var place = _packet.Read<ModCarPlace>();
		var playSound = _packet.Read<bool>();

		MelonCoroutines.Start(Garage.Tools.ToolsMoveManager.UpdateToolMove((IOSpecialType)tool, place, playSound));
	}

	public static void CarWashPacket(Packet _packet)
	{
		int carLoaderID = _packet.ReadInt();
		bool interior = _packet.Read<bool>();

		MelonCoroutines.Start(Garage.Tools.CarWashLogic.WashCar(carLoaderID, interior));
	}
	
	public static void CarPaintPacket(Packet _packet)
	{
		ModColor color = _packet.Read<ModColor>();

		MelonCoroutines.Start(CarPaintLogic.ChangeColor(color));
	}
	
	public static void LoadCarPacket(Packet packet)
	{
		var carData = packet.Read<ModNewCarData>();
		var carLoaderID = packet.ReadInt();

		//MelonLogger.Msg("[ClientHandle->LoadCarPacket] Received new car info.");

		MelonCoroutines.Start(CarSpawnManager.LoadCarFromServer(carData, carLoaderID));
	}

	public static void BodyPartPacket(Packet packet)
	{
		var carPart = packet.Read<ModCarPart>();
		var carLoaderID = packet.ReadInt();

		//MelonLogger.Msg("[ClientHandle->BodyPartPacket] Receive BodyPart.");
		MelonCoroutines.Start(PartsUpdater.UpdateBodyParts(carPart, carLoaderID));
	}

	public static void PartScriptPacket(Packet packet)
	{
		var partScript = packet.Read<ModPartScript>();
		var carLoaderID = packet.ReadInt();

		MelonLogger.Msg("[ClientHandle->PartScriptPacket] Receive PartScript.");
		MelonCoroutines.Start(PartsUpdater.UpdatePartScripts(partScript, carLoaderID));
	}

	public static void DeleteCarPacket(Packet packet)
	{
		var carLoaderID = packet.ReadInt();

		MelonLogger.Msg($"[ClientHandle->DeleteCarPacket] Delete Car {carLoaderID}.");
		MelonCoroutines.Start(CarSyncManager.DeleteCar(carLoaderID));
	}

	public static void CarPositionPacket(Packet packet)
	{
		var placeNo = packet.ReadInt();
		var carLoaderID = packet.ReadInt();
		
		MelonLogger.Msg($"[ClientHandle->CarPositionPacket] Move {carLoaderID} to {placeNo}.");
		MelonCoroutines.Start(CarSyncManager.ChangePosition(carLoaderID, placeNo));
	}

	public static void GarageUpgradePacket(Packet packet)
	{
		var upgrade = packet.Read<GarageUpgrade>();

		//MelonLogger.Msg($"[ClientHandle->GarageUpgradePacket] Received upgrade for {upgrade.upgradeID}.");
		MelonCoroutines.Start(GarageUpgradeManager.SetUpgrade(upgrade));
	}

	public static void JobPacket(Packet packet)
	{
		var job = packet.Read<ModJob>();
		
		//MelonLogger.Msg("[ClientHandle->JobPacket] Received a job.");
		MelonCoroutines.Start(JobManager.AddJob(job));
	}

	public static void JobActionPacket(Packet packet)
	{
		ModJob job = packet.Read<ModJob>();
		var takeJob = packet.Read<bool>();
		
		MelonCoroutines.Start(JobManager.JobAction(job, takeJob));
	}

	public static void SelectedJobPacket(Packet packet)
	{
		var job = packet.Read<ModJob>();
		var action = packet.Read<bool>();

		MelonCoroutines.Start(JobManager.SelectedJob(job, action));
	}

	public static void EndJobPacket(Packet packet)
	{
		var job = packet.Read<ModJob>();

		MelonCoroutines.Start(JobManager.OnJobComplete(job));
	}
	
	public static void EngineCraneHandlePacket(Packet packet)
	{
		int action = packet.ReadInt();
		int carLoaderID = packet.ReadInt();

		if (action == 1)
		{
			ModGroupItem item = packet.Read<ModGroupItem>();
			MelonCoroutines.Start(EngineCrane.InsertEngineIntoCar(item));
			return;
		}
		MelonCoroutines.Start(EngineCrane.UseEngineCrane(carLoaderID));
	}
	
	public static void EngineSetGroupPacket(Packet packet)
	{
		ModGroupItem engineGroup = packet.Read<ModGroupItem>();
		Vector3Serializable position = packet.Read<Vector3Serializable>();
		bool alt = packet.Read<bool>();
		
		MelonCoroutines.Start(EngineStand.TakeOnEngineFromStand(engineGroup, position, alt));
	}
	public static void EngineTakeOffPacket(Packet packet)
	{
		bool alt = packet.Read<bool>();
		
		MelonCoroutines.Start(EngineStand.TakeOffEngineFromStand(alt));
	}
	
	public static void EngineStandAnglePacket(Packet packet)
	{
		float angle = packet.ReadInt();
		bool alt = packet.Read<bool>();
		
		MelonCoroutines.Start(EngineStand.IncreaseEngineStandAngle(angle, alt));
	}
	
	public static void RepairPartPacket(Packet packet)
	{
		ModPartInfo info = packet.Read<ModPartInfo>();
		bool isBody = packet.Read<bool>();
		bool success = packet.Read<bool>();
		
		MelonCoroutines.Start(RepairPartLogic.RepairAction(info, isBody, success));
	}
	
	public static void CarFluidPacket(Packet packet)
	{
		var carLoaderID = packet.ReadInt();
		ModFluidData fluid = packet.Read<ModFluidData>();
		
		MelonCoroutines.Start(PartsUpdater.UpdateFluid(fluid, carLoaderID));
	}
	
	public static void AddCarToParkPacket(Packet packet)
	{
		ModNewCarData car = packet.Read<ModNewCarData>();
		int index = packet.ReadInt();
		
		MelonCoroutines.Start(ParkHook.AddCarToPark(car, index));
	}
	
	public static void RemoveCarFromParkPacket(Packet packet)
	{
		int index = packet.ReadInt();
		
		MelonCoroutines.Start(ParkHook.RemoveCarFromPark(index));
	}

	public static void SceneChangePacket(Packet packet)
	{
		var scene = packet.Read<GameScene>();
		var id = packet.ReadInt();

		ClientData.Instance.connectedClients[id].scene = scene;
		if (scene != SceneManager.CurrentScene())
			ClientData.Instance.connectedClients[id].DestroyPlayer();
		else if (ClientData.Instance.connectedClients[id].userObject == null) ClientData.Instance.connectedClients[id].SpawnPlayer();
	}
}