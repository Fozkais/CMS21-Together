using System.Collections;
using System.Linq;
using CMS;
using CMS.UI.Logic;
using CMS.UI.Logic.Upgrades;
using CMS21Together.Shared.Data;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Campaign;

public static class GarageUpgradeManager
{
	public static IEnumerator SetUpgrade(GarageUpgrade upgrade)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);

		yield return new WaitForEndOfFrame();

		ClientData.Instance.garageUpgrades[upgrade.upgradeID] = upgrade;
		var gt = GameData.Instance.upgradeTools;

		foreach (UpgradeItem item in gt.upgradeItems.ToArray())
		{
			if (item.upgradeID == upgrade.upgradeID)
			{
				gt.UnlockSkill(item);
				break;
			}
		}
	}
}