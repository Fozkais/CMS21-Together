using System.Collections.Generic;
using CMS21Together.Shared.Data.Vanilla.Cars;
using MelonLoader;

namespace CMS21Together.ServerSide.Data;

public static class ServerResyncs
{
	public static void ResyncCars(int playerID)
	{
		foreach (KeyValuePair<int, ModNewCarData> carSpawnData in ServerData.Instance.CarSpawnDatas)
		{
			ServerSend.LoadCarPacket(playerID, carSpawnData.Value, carSpawnData.Key, true);
		}

		foreach (KeyValuePair<int,ModCarInfo> modCarInfo in ServerData.Instance.CarPartInfo)
		{
			foreach (KeyValuePair<int, ModCarPart> partsReference in modCarInfo.Value.BodyPartsReferences)
			{
				ServerSend.BodyPartPacket(playerID, partsReference.Value, modCarInfo.Key, true);
			}
			
			foreach (KeyValuePair<int,Dictionary<int,ModPartScript>> partsReference in modCarInfo.Value.OtherPartsReferences)
			{
				foreach (KeyValuePair<int,ModPartScript> modPartScript in partsReference.Value)
				{
					ServerSend.PartScriptPacket(playerID, modPartScript.Value, modCarInfo.Key, true);
				}
			}
			
			foreach (KeyValuePair<int, ModPartScript> partsReference in modCarInfo.Value.DriveshaftPartsReferences)
			{
				ServerSend.PartScriptPacket(playerID, partsReference.Value, modCarInfo.Key, true);
			}
			
			foreach (KeyValuePair<int, ModPartScript> partsReference in modCarInfo.Value.EnginePartsReferences)
			{
				ServerSend.PartScriptPacket(playerID, partsReference.Value, modCarInfo.Key, true);
			}
			
			foreach (KeyValuePair<int,Dictionary<int,ModPartScript>> partsReference in modCarInfo.Value.SuspensionPartsReferences)
			{
				foreach (KeyValuePair<int,ModPartScript> modPartScript in partsReference.Value)
				{
					ServerSend.PartScriptPacket(playerID, modPartScript.Value, modCarInfo.Key, true);
				}
			}
		}
		MelonLogger.Msg("[ServerResyncs->ResyncCars] Resent all car info to client!");
	}
}