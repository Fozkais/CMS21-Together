using System.Collections.Generic;
using CMS21Together.Shared.Data.Vanilla.Cars;
using MelonLoader;

namespace CMS21Together.ServerSide.Data;

public static class ServerResyncs
{
	public static void ResyncCar(int playerID, int carLoaderID)
	{
		ModNewCarData carToResync = ServerData.Instance.CarSpawnDatas[carLoaderID];
		ModCarInfo carInfo = ServerData.Instance.CarPartInfo[carLoaderID];
		
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
			}
		}
		MelonLogger.Msg("[ServerResyncs->ResyncCar] Resent car info to client!");
	}
}