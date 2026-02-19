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
		            
		            // 1. Vérification d'existence et d'index
		            if (upgradeData == null || packet.level < 0 || packet.level >= upgradeData.Costs.Count) return;

		            // 2. Vérification si déjà débloqué
		            if (garageState.GarageUpgradeLevels.TryGetValue(packet.id, out bool[] levels))
		            {
		                if (levels[packet.level]) return; // Déjà acheté !

		                // 3. Vérification du solde
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

				        // 1. Calcul du total des points gagnés via la liste PointsPerLevel
				        int totalPointsEarned = 0;
				        var pointsList = GameDatabase.PlayerUpgrades.PointsPerLevel;
        
				        // On additionne les points pour chaque niveau atteint
				        for (int i = 0; i < worldState.Level && i < pointsList.Count; i++)
				        {
					        totalPointsEarned += pointsList[i];
				        }

				        // 2. Calcul des points déjà dépensés
				        int totalPointsSpent = CalculateSpentPoints(garageState.PlayerUpgradeLevels);
				        int availablePoints = totalPointsEarned - totalPointsSpent;

				        int cost = skillData.Costs[packet.level];
				        if (availablePoints >= cost)
				        {
					        skillLevels[packet.level] = true;
					        Logger.Info($"[Server] Skill {packet.id} Lvl {packet.level} validated for client {clientId}");
				        }
			        }
		            break;
		    }
			Server.SendToClients(GameDataManager.CurrentState.WorldState);
			Server.SendToClients(GameDataManager.CurrentState.GarageState);
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