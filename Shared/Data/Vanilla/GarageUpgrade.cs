using System;

namespace CMS21Together.Shared.Data.Vanilla;

[Serializable]
public struct GarageUpgrade
{
	public string upgradeID;
	public bool unlocked;

	public GarageUpgrade(string id, bool _unlocked)
	{
		upgradeID = id;
		unlocked = _unlocked;
	}
}