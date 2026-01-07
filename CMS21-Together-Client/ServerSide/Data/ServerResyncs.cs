using MelonLoader;

namespace CMS21Together.ServerSide.Data;

public static class ServerResyncs
{
	public static void ResyncCar(int playerID, int carLoaderID)
	{
		var carToResync = ServerData.Instance.CarSpawnDatas[carLoaderID];
		var carInfo = ServerData.Instance.CarPartInfo[carLoaderID];

		MelonLogger.Msg($"Sent a resync car from: {carLoaderID}  {carToResync.CarInfoData.CarFrom}");

		ServerSend.LoadCarPacket(playerID, carToResync, carLoaderID, true);

		foreach (var partsReference in carInfo.BodyPartsReferences) ServerSend.BodyPartPacket(playerID, partsReference.Value, carLoaderID, true);

		foreach (var partsReference in carInfo.OtherPartsReferences)
		foreach (var modPartScript in partsReference.Value)
		{
			ServerSend.PartScriptPacket(playerID, modPartScript.Value, carLoaderID, true);
			MelonLogger.Msg("Sent part.");
		}

		foreach (var partsReference in carInfo.DriveshaftPartsReferences) ServerSend.PartScriptPacket(playerID, partsReference.Value, carLoaderID, true);

		foreach (var partsReference in carInfo.EnginePartsReferences) ServerSend.PartScriptPacket(playerID, partsReference.Value, carLoaderID, true);

		foreach (var partsReference in carInfo.SuspensionPartsReferences)
		foreach (var modPartScript in partsReference.Value)
		{
			ServerSend.PartScriptPacket(playerID, modPartScript.Value, carLoaderID, true);
			MelonLogger.Msg("Sent part.");
		}

		MelonLogger.Msg("[ServerResyncs->ResyncCar] Resent car info to client!");
	}

	public static void ResyncEngineStand(int fromClient, bool alt)
	{
		MelonLogger.Msg("Client asked for es resync!");
		if (alt)
		{
			if (ServerData.Instance.engineStand2 != null && ServerData.Instance.engineStand2.engineGroupItem != null)
			{
				ServerSend.EngineStandSetGroupPacket(fromClient, ServerData.Instance.engineStand2.engineGroupItem, ServerData.Instance.engineStand2.position, true, true);
				foreach (var part in ServerData.Instance.engineStand2.parts)
				{
					ServerSend.PartScriptPacket(fromClient, part.Value, -2, true);
					MelonLogger.Msg($"Sent Engine Stand part {part.Value.id}!");
				}

				MelonLogger.Msg("Sent Engine Stand Resync!");
			}
		}
		else
		{
			if (ServerData.Instance.engineStand != null && ServerData.Instance.engineStand.engineGroupItem != null)
			{
				ServerSend.EngineStandSetGroupPacket(fromClient, ServerData.Instance.engineStand.engineGroupItem, ServerData.Instance.engineStand.position, false, true);
				foreach (var part in ServerData.Instance.engineStand.parts) ServerSend.PartScriptPacket(fromClient, part.Value, -1, true);
				MelonLogger.Msg("Sent Engine Stand Resync!");
			}
		}
	}

	public static void ResyncTools(int fromClient)
	{
		foreach (var tool in ServerData.Instance.toolsPosition) ServerSend.ToolsMovePacket(fromClient, tool.Key, tool.Value, false, true);
	}

	public static void ResyncPark(int fromClient)
	{
		foreach (var car in ServerData.Instance.CarOnPark) ServerSend.AddCarToParkPacket(fromClient, car.Value, car.Key, true);
	}

	public static void ResyncUpgrade(int fromClient)
	{
		foreach (var upgrade in ServerData.Instance.garageUpgrades) ServerSend.GarageUpgradePacket(fromClient, upgrade.Value, true);
	}
}