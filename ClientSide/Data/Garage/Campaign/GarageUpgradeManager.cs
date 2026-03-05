using System.Collections;
using System.Linq;
using CMS;
using CMS.UI.Logic;
using CMS.UI.Logic.Upgrades;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Campaign;

public static class GarageUpgradeManager
{
	public static IEnumerator SetUpgrade(GarageUpgrade upgrade)
	{
		while (!GameLoadHook.IsGameReady())
			yield return new WaitForSeconds(0.25f);

		yield return new WaitForEndOfFrame();

		if (upgrade.upgradeID == "initialSent")
		{
			GarageUpgradeHooks.listenToUpgrades = true;
			GarageUpgradeHooks.receivedInitial = true;
			yield break;
		}
		string upgradeName = upgrade.upgradeID + upgrade.upgradeLevel;
		ClientData.Instance.garageUpgrades[upgradeName] = upgrade;
		
		if (upgrade.upgradeID == "crane" && !upgrade.unlocked)
			GameData.Instance.engineStandLogic2.gameObject.SetActive(false);
		else if (upgrade.upgradeID == "crane" && upgrade.unlocked)
			GameData.Instance.engineStandLogic2.gameObject.SetActive(true);

		GarageAndToolsTab upgradeTools = GameData.Instance.upgradeTools;
		
		upgradeTools.PrepareItems();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		
		if (upgradeTools.upgradeItems.ToArray().Any(u => u.upgradeID == upgrade.upgradeID && u.UpgradeLevel == upgrade.upgradeLevel))
		{
			UpgradeItem item = upgradeTools.upgradeItems.ToArray().First(u => u.upgradeID == upgrade.upgradeID && u.UpgradeLevel == upgrade.upgradeLevel);
			if (upgrade.unlocked && item != null && item.upgradeState != UpgradeState.Unlocked)
			{
				//MelonLogger.Msg($"Unlock : {upgrade.upgradeID} , {item == null}");
				yield return new WaitForEndOfFrame();
				upgradeTools.UpdateSkillState(item, UpgradeState.Unlocked);
				upgradeTools.UpdateRelatedSkillState(item);
				upgradeTools.upgradeSystem.UnlockUpgrade(item.UpgradeID, item.UpgradeLevel, UpgradeType.Money);
				MainMod.StartCoroutine(upgradeTools.SwitchObjectsUnlock(item.UpgradeID, true));
			}
		}
	}
}