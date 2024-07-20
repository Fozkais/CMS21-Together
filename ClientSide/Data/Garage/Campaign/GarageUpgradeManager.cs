using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.Shared.Data;
using Il2Cpp;
using Il2CppCMS;
using Il2CppCMS.UI.Logic;
using Il2CppCMS.UI.Logic.Upgrades;
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
        GarageAndToolsTab gt = GameData.Instance.upgradeTools;

        string upgradeID;
        if (upgrade.upgradeID == "bus")
	        upgradeID = "lifter";
        else
			upgradeID = upgrade.upgradeID;
        
        bool unlockUpgrade = upgrade.unlocked;
        
        UpgradeObjectsData objectsData = default;
        foreach (UpgradeObjectsData objData in gt.upgradeObjectsData._items)
        {
	        if (objData.ID == upgradeID)
	        {
		        objectsData = objData;
		        break;
	        }
        }

        if (objectsData == null) yield break;
        
		Il2CppSystem.Collections.Generic.List<GameObject> objectsToTurnOn = objectsData.objectsToTurnOn;
		if (objectsToTurnOn.Count > 0)
		{
			for (int i = 0; i < objectsToTurnOn.Count; i++)
			{
				objectsToTurnOn._items[i].SetActive(unlockUpgrade);
			}
		}

		Il2CppSystem.Collections.Generic.List<GameObject> objectsToTurnOff = objectsData.objectsToTurnOff;
		if (objectsToTurnOff.Count > 0)
		{
			for (int j = 0; j < objectsToTurnOff.Count; j++)
			{
				objectsToTurnOff._items[j].SetActive(!unlockUpgrade);
			}
		}
		InteractiveObject[] highlightOnUnlock = objectsData.HighlightOnUnlock;
		if (highlightOnUnlock.Length != 0)
		{
			for (int k = 0; k < highlightOnUnlock.Length; k++)
			{
				highlightOnUnlock[k].SetLayerRecursively(Layers.NewHighlighterActiveObjects);
			}
		}

		if (unlockUpgrade)
		{
			bool breakLoop = false;
			foreach (UpgradeItem item in gt.upgradeItems)
			{
				if (breakLoop) break;
				
				if (item.upgradeID == upgradeID)
				{
					breakLoop = true;
					
					gt.UpdateSkillState(item, UpgradeState.Unlocked);
					gt.UpdateRelatedSkillState(item);
					gt.upgradeSystem.UnlockUpgrade(item.UpgradeID, item.UpgradeLevel, UpgradeType.Money);
					GarageUpgradeHooks.listenToUpgrades = false;
					gt.StartCoroutine(gt.SwitchObjectsUnlock(item.UpgradeID, unlockUpgrade: true, false));
					
					MelonLogger.Msg($"UnlockedItem : {item.UpgradeID} {upgradeID}");
					
				}
			}
		}

		
		if (upgradeID == "paintshop" || upgradeID == "path_test")
		{
			Il2CppSystem.Collections.Generic.List<GameObject> relatedObjectsToTurnOn = objectsData.relatedObjectsToTurnOn;
			if (relatedObjectsToTurnOn != null && relatedObjectsToTurnOn.Count > 0)
			{
				for (int l = 0; l < relatedObjectsToTurnOn.Count; l++)
				{
					relatedObjectsToTurnOn._items[l].SetActive(true);
				}
			}

			Il2CppSystem.Collections.Generic.List<GameObject> relatedObjectsToTurnOff = objectsData.relatedObjectsToTurnOff;
			if (relatedObjectsToTurnOff != null && relatedObjectsToTurnOff.Count > 0)
			{
				for (int m = 0; m < relatedObjectsToTurnOff.Count; m++)
				{
					relatedObjectsToTurnOff._items[m].SetActive(false);
				}
			}
		}
    }
}