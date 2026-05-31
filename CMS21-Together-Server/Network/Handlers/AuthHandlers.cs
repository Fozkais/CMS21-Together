using System;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21_Together_Server.Data;
using CMS21_Together_Server.Log;

namespace CMS21_Together_Server.Network.Handlers
{
	public static class AuthHandler
	{
		[PacketHandler(PacketTypes.Heartbeat)]
		public static void OnHeartbeat(long clientId, HeartbeatPacket packet)
		{
			Server.Clients[(int)clientId].LastHeartbeatTime = ServerTime.Time;
		}
		
		[PacketHandler(PacketTypes.Connect)]
		public static void OnConnected(long clientId, ConnectPacket packet)
		{
			Logger.Debug($"Reiceved Connection callback from {packet.username}");
			Logger.Debug($"Received info: {packet.modVersion}, {packet.username}, {packet.playerID}");

			if (packet.modVersion != Program.MOD_VERSION)
			{
				Server.SendToClient(new DisconnectPacket()
				{
					message = $"Server require mod version {Program.MOD_VERSION}."
				},(int)clientId);
				return;
			}
			Server.Clients[(int)clientId].OnConnectedSuccessfully.Invoke();
		}

		[PacketHandler(PacketTypes.AskForSync)]
		public static void OnAskForSync(long clientId, AskForSync packet)
		{
			GameDataManager.CurrentState.WorldState.updateGamemode = true;
			Server.SendToClient(GameDataManager.CurrentState.WorldState, (int)clientId);
			GameDataManager.CurrentState.WorldState.updateGamemode = false;
			GameDataManager.CurrentState.GarageState.AvailablePoints = GarageUpgradeHandler.ComputeAvailablePoints(GameDataManager.CurrentState.WorldState, GameDataManager.CurrentState.GarageState);
			Server.SendToClient(GameDataManager.CurrentState.GarageState, (int)clientId);
			
			// --- Inventory Sync Logic ---
			var invState = GameDataManager.CurrentState.InventoryState;
			int batchSize = 50;
			
			var allInvItems = invState.InventoryItems ?? new System.Collections.Generic.List<CMS21_Together_Core.Data.GameType.ModItem>();
			var allInvGroups = invState.InventoryGroupItems ?? new System.Collections.Generic.List<CMS21_Together_Core.Data.GameType.ModGroupItem>();
			var allWhItems = invState.WarehouseItems ?? new System.Collections.Generic.List<CMS21_Together_Core.Data.GameType.ModItem>();
			var allWhGroups = invState.WarehouseGroupItems ?? new System.Collections.Generic.List<CMS21_Together_Core.Data.GameType.ModGroupItem>();

			int totalItems = allInvItems.Count + allInvGroups.Count + allWhItems.Count + allWhGroups.Count;
			
			if (totalItems == 0)
			{
				Server.SendToClient(new InventorySyncPacket
				{
					IsFirstBatch = true,
					IsLastBatch = true,
					InventoryItems = new System.Collections.Generic.List<CMS21_Together_Core.Data.GameType.ModItem>(),
					InventoryGroupItems = new System.Collections.Generic.List<CMS21_Together_Core.Data.GameType.ModGroupItem>(),
					WarehouseItems = new System.Collections.Generic.List<CMS21_Together_Core.Data.GameType.ModItem>(),
					WarehouseGroupItems = new System.Collections.Generic.List<CMS21_Together_Core.Data.GameType.ModGroupItem>()
				}, (int)clientId);
			}
			else
			{
				bool isFirst = true;
				int invItemIdx = 0, invGroupIdx = 0, whItemIdx = 0, whGroupIdx = 0;
				
				while (invItemIdx < allInvItems.Count || invGroupIdx < allInvGroups.Count || whItemIdx < allWhItems.Count || whGroupIdx < allWhGroups.Count)
				{
					var batch = new InventorySyncPacket
					{
						IsFirstBatch = isFirst,
						IsLastBatch = false,
						InventoryItems = new System.Collections.Generic.List<CMS21_Together_Core.Data.GameType.ModItem>(),
						InventoryGroupItems = new System.Collections.Generic.List<CMS21_Together_Core.Data.GameType.ModGroupItem>(),
						WarehouseItems = new System.Collections.Generic.List<CMS21_Together_Core.Data.GameType.ModItem>(),
						WarehouseGroupItems = new System.Collections.Generic.List<CMS21_Together_Core.Data.GameType.ModGroupItem>()
					};
					
					int currentBatchCount = 0;
					
					while (currentBatchCount < batchSize && invItemIdx < allInvItems.Count)
					{
						batch.InventoryItems.Add(allInvItems[invItemIdx++]);
						currentBatchCount++;
					}
					while (currentBatchCount < batchSize && invGroupIdx < allInvGroups.Count)
					{
						batch.InventoryGroupItems.Add(allInvGroups[invGroupIdx++]);
						currentBatchCount++;
					}
					while (currentBatchCount < batchSize && whItemIdx < allWhItems.Count)
					{
						batch.WarehouseItems.Add(allWhItems[whItemIdx++]);
						currentBatchCount++;
					}
					while (currentBatchCount < batchSize && whGroupIdx < allWhGroups.Count)
					{
						batch.WarehouseGroupItems.Add(allWhGroups[whGroupIdx++]);
						currentBatchCount++;
					}
					
					if (invItemIdx >= allInvItems.Count && invGroupIdx >= allInvGroups.Count && whItemIdx >= allWhItems.Count && whGroupIdx >= allWhGroups.Count)
					{
						batch.IsLastBatch = true;
					}
					
					Server.SendToClient(batch, (int)clientId);
					isFirst = false;
				}
			}
			
			Server.SendToClient(new SyncEnd(), (int)clientId);
		}
	}
}