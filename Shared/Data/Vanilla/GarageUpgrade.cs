using System;

namespace CMS21Together.Shared.Data.Vanilla;

[Serializable]
public struct GarageUpgrade
{
	public string upgradeID;
	public int upgradeLevel;
	public bool unlocked;

	public GarageUpgrade(string id, int level, bool _unlocked)
	{
		upgradeID = id;
		upgradeLevel = level;
		unlocked = _unlocked;
	}
}