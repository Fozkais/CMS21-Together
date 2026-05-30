using System;
using System.Collections.Generic;
using CMS21_Together_Core;
using CMS21_Together_Core.Data.Enum;
using CMS21_Together_Core.Data.GameType;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21_Together_Server.Data;

namespace CMS21_Together_Server.Network.Handlers
{
	public static class GarageUpgradeHandler
	{
		[PacketHandler(PacketTypes.UpgradeRequest)]
		public static void OnUpgradeRequest(long clientId, UpgradeRequest packet)
		{
			var garageState = GameDataManager.CurrentState.GarageState;
		    var worldState = GameDataManager.CurrentState.WorldState;
		    

		    switch (packet.type)
		    {
		        case UpgradeType.Money:
		            var upgradeData = GameDatabase.PlayerUpgrades.MoneyUpgrades.Find(s => s.ID == packet.id);
		            
		            if (upgradeData == null || packet.level < 0 || packet.level >= upgradeData.Costs.Count) return;
		            
		            if (garageState.GarageUpgradeLevels.TryGetValue(packet.id, out bool[] levels))
		            {
		                if (levels[packet.level]) return;
		                
		                int cost = upgradeData.Costs[packet.level];
		                if (worldState.Money >= cost)
		                {
		                    worldState.Money -= cost;
		                    levels[packet.level] = true;
		                    Logger.Info($"Player {clientId} bought {packet.id} Lvl {packet.level} for {cost}$");
		                }
		            }
		            break;

		        case UpgradeType.Points:
			        var skillData = GameDatabase.PlayerUpgrades.PointUpgrades.Find(s => s.ID == packet.id);
			        if (skillData == null || packet.level < 0 || packet.level >= skillData.Costs.Count) return;

			        if (garageState.PlayerUpgradeLevels.TryGetValue(packet.id, out bool[] skillLevels))
			        {
				        if (skillLevels[packet.level]) return; 

				        // Calcul des points
				        int availablePoints = ComputeAvailablePoints(worldState, garageState);

				        int cost = skillData.Costs[packet.level];
				        if (availablePoints >= cost)
				        {
					        skillLevels[packet.level] = true;
					        Logger.Info($"[Server] Skill {packet.id} Lvl {packet.level} validated for client {clientId}");
				        }
			        }
		            break;
		    }
			GameDataManager.CurrentState.GarageState.AvailablePoints = ComputeAvailablePoints(GameDataManager.CurrentState.WorldState, GameDataManager.CurrentState.GarageState);
			Server.SendToClients(GameDataManager.CurrentState.WorldState);
			Server.SendToClients(GameDataManager.CurrentState.GarageState);
		}
		
		public static int ComputeAvailablePoints(WorldState worldState, GarageState garageState)
		{
			int totalPointsEarned = 0;
			var pointsList = GameDatabase.PlayerUpgrades.PointsPerLevel;
			for (int i = 0; i < worldState.Level && i < pointsList.Count; i++)
			{
				totalPointsEarned += pointsList[i];
			}
			int totalPointsSpent = CalculateSpentPoints(garageState.PlayerUpgradeLevels);
			return totalPointsEarned - totalPointsSpent;
		}

		private static int CalculateSpentPoints(Dictionary<string, bool[]> playerSkills)
		{
			int spent = 0;
			foreach (var skill in playerSkills)
			{
				var dbSkill = GameDatabase.PlayerUpgrades.PointUpgrades.Find(s => s.ID == skill.Key);
				if (dbSkill == null) continue;

				for (int i = 0; i < skill.Value.Length; i++)
				{
					if (skill.Value[i]) spent += dbSkill.Costs[i];
				}
			}
			return spent;
		}
	}
	
}