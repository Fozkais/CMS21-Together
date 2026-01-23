using CMS.Difficulty;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Data;

namespace CMS21Together.Network.Handlers;

public static class WorldStatesPackets
{
	[PacketHandler(PacketTypes.WorldState)]
	public static void HandleWorldState(long senderId, WorldState packet)
	{
		DifficultyManager difficultyManager = Singleton<GameManager>.Instance.DifficultyManager;
		difficultyManager.SetDifficultyLevel((DifficultyLevel)packet.Gamemode);
		difficultyManager.ActivateDifficultyLevel();
		
		ClientData.IsWorldStateSynced = true;
	}
	
	[PacketHandler(PacketTypes.GarageState)]
	public static void HandleGarageState(long senderId, GarageState packet)
	{
		ClientData.IsGarageStateSynced = true;
	}
	
	[PacketHandler(PacketTypes.SyncEnd)]
	public static void HandleSyncEnd(long senderId, SyncEnd packet)
	{
		if (ClientData.IsWorldStateSynced && ClientData.IsGarageStateSynced)
			ClientData.IsInitialSyncFinished = true;
	}
}