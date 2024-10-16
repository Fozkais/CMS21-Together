using System.Collections;
using CMS;
using CMS.UI.Logic;
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

		string upgradeID;
		if (upgrade.upgradeID == "bus")
			upgradeID = "lifter";
		else
			upgradeID = upgrade.upgradeID;

		var unlockUpgrade = upgrade.unlocked;

		UpgradeObjectsData objectsData = default;
		foreach (var objData in gt.upgradeObjectsData._items)
			if (objData.ID == upgradeID)
			{
				objectsData = objData;
				break;
			}

		if (objectsData == null) yield break;

		var objectsToTurnOn = objectsData.objectsToTurnOn;
		if (objectsToTurnOn.Count > 0)
			for (var i = 0; i < objectsToTurnOn.Count; i++)
				objectsToTurnOn._items[i].SetActive(unlockUpgrade);

		var objectsToTurnOff = objectsData.objectsToTurnOff;
		if (objectsToTurnOff.Count > 0)
			for (var j = 0; j < objectsToTurnOff.Count; j++)
				objectsToTurnOff._items[j].SetActive(!unlockUpgrade);
		InteractiveObject[] highlightOnUnlock = objectsData.HighlightOnUnlock;
		if (highlightOnUnlock.Length != 0)
			for (var k = 0; k < highlightOnUnlock.Length; k++)
				highlightOnUnlock[k].SetLayerRecursively(Layers.NewHighlighterActiveObjects);

		if (unlockUpgrade)
		{
			var breakLoop = false;
			foreach (var item in gt.upgradeItems)
			{
				if (breakLoop) break;

				if (item.upgradeID == upgradeID)
				{
					breakLoop = true;

					gt.UpdateSkillState(item, UpgradeState.Unlocked);
					gt.UpdateRelatedSkillState(item);
					gt.upgradeSystem.UnlockUpgrade(item.UpgradeID, item.UpgradeLevel, UpgradeType.Money);
					GarageUpgradeHooks.listenToUpgrades = false;
					gt.StartCoroutine(gt.SwitchObjectsUnlock(item.UpgradeID, true));

					MelonLogger.Msg($"UnlockedItem : {item.UpgradeID} {upgradeID}");
				}
			}
		}


		if (upgradeID == "paintshop" || upgradeID == "path_test")
		{
			var relatedObjectsToTurnOn = objectsData.relatedObjectsToTurnOn;
			if (relatedObjectsToTurnOn != null && relatedObjectsToTurnOn.Count > 0)
				for (var l = 0; l < relatedObjectsToTurnOn.Count; l++)
					relatedObjectsToTurnOn._items[l].SetActive(true);

			var relatedObjectsToTurnOff = objectsData.relatedObjectsToTurnOff;
			if (relatedObjectsToTurnOff != null && relatedObjectsToTurnOff.Count > 0)
				for (var m = 0; m < relatedObjectsToTurnOff.Count; m++)
					relatedObjectsToTurnOff._items[m].SetActive(false);
		}
	}
}