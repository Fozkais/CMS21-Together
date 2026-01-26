using System;
using System.Collections.Generic;

namespace CMS21_Together_Core.Data.GameType;

[Serializable]
public class PlayerUpgradeData
{
	public string ID;
	public List<int> Costs = new List<int>();
	public List<bool> UnlockedLevels = new List<bool>();
}

[Serializable]
public class PlayerUpgrades
{
	public List<PlayerUpgradeData> MoneyUpgrades = new List<PlayerUpgradeData>();
	public List<PlayerUpgradeData> PointUpgrades = new List<PlayerUpgradeData>();
	public List<int> PointsPerLevel = new List<int>();
}