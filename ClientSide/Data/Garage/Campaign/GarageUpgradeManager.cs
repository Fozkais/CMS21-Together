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

		if (upgrade.upgradeID == "initialSent")
		{
			GarageUpgradeHooks.listenToUpgrades = true;
			GarageUpgradeHooks.receivedInitial = true;
			yield break;
		}
		
		ClientData.Instance.garageUpgrades[upgrade.upgradeID] = upgrade;
		
		GarageLevelManager glm = Object.FindObjectOfType<GarageLevelManager>();
		if (glm.garageAndToolsTab.upgradeItems.ToArray().Any(u => u.upgradeID == upgrade.upgradeID))
		{
			UpgradeItem item = glm.garageAndToolsTab.upgradeItems.ToArray().First(u => u.upgradeID == upgrade.upgradeID);
			if (upgrade.unlocked)
				glm.garageAndToolsTab.UnlockSkill(item);
		}
	}
}