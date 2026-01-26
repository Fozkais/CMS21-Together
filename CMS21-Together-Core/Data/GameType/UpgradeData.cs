using System;
using System.Collections.Generic;

namespace CMS21_Together_Core.Data.GameType;

[Serializable]
public class UpgradeData
{
	public string upgradeID;
	public int upgradeLevel;
	public List<string> relatedUpgradeIDs = new List<string>();
}