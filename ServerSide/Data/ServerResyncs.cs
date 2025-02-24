using System;
using System.Collections.Generic;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using CMS21Together.Shared.Data.Vanilla.Cars;
using CMS21Together.Shared.Data.Vanilla.GarageTool;
using MelonLoader;

namespace CMS21Together.ServerSide.Data;

public static class ServerResyncs
{
	public static void ResyncCar(int playerID, int carLoaderID)
	{
		ModNewCarData carToResync = ServerData.Instance.CarSpawnDatas[carLoaderID];
		ModCarInfo carInfo = ServerData.Instance.CarPartInfo[carLoaderID];
		
		MelonLogger.Msg($"Sent a resync car from: {carLoaderID}  {carToResync.CarInfoData.CarFrom}");
		
		ServerSend.LoadCarPacket(playerID, carToResync, carLoaderID, true);

		foreach (KeyValuePair<int, ModCarPart> partsReference in carInfo.BodyPartsReferences)
		{
			ServerSend.BodyPartPacket(playerID, partsReference.Value, carLoaderID, true);
		}
		
		foreach (KeyValuePair<int,Dictionary<int,ModPartScript>> partsReference in carInfo.OtherPartsReferences)
		{
			foreach (KeyValuePair<int,ModPartScript> modPartScript in partsReference.Value)
			{
				ServerSend.PartScriptPacket(playerID, modPartScript.Value, carLoaderID, true);
				MelonLogger.Msg("Sent part.");
			}
		}
		
		foreach (KeyValuePair<int, ModPartScript> partsReference in carInfo.DriveshaftPartsReferences)
		{
			ServerSend.PartScriptPacket(playerID, partsReference.Value, carLoaderID, true);
		}
		
		foreach (KeyValuePair<int, ModPartScript> partsReference in carInfo.EnginePartsReferences)
		{
			ServerSend.PartScriptPacket(playerID, partsReference.Value, carLoaderID, true);
		}
		
		foreach (KeyValuePair<int,Dictionary<int,ModPartScript>> partsReference in carInfo.SuspensionPartsReferences)
		{
			foreach (KeyValuePair<int,ModPartScript> modPartScript in partsReference.Value)
			{
				ServerSend.PartScriptPacket(playerID, modPartScript.Value, carLoaderID, true);
				MelonLogger.Msg("Sent part.");
			}
		}
		MelonLogger.Msg("[ServerResyncs->ResyncCar] Resent car info to client!");
	}

	public static void ResyncTools(int fromClient)
	{
		foreach (KeyValuePair<ModIOSpecialType, ModCarPlace> tool in ServerData.toolsPosition)
		{
			ServerSend.ToolsMovePacket(fromClient, tool.Key, tool.Value, false, true);
		}
	}

	public static void ResyncUpgrade(int fromClient)
	{
		foreach (KeyValuePair<string,GarageUpgrade> upgrade in ServerData.Instance.garageUpgrades)
		{
			ServerSend.GarageUpgradePacket(fromClient, upgrade.Value, true);
		}
	}
}