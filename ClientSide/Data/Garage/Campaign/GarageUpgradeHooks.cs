using System.Collections;
using System.Linq;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.Shared.Data;
using HarmonyLib;
using Il2CppCMS.UI.Logic;
using Il2CppCMS.UI.Logic.Upgrades;
using MelonLoader;

namespace CMS21Together.ClientSide.Data.Garage.Campaign;

[HarmonyPatch]
public static class GarageUpgradeHooks
{
    public static bool listenToUpgrades = true;
    
    
    [HarmonyPatch(typeof(GarageAndToolsTab), nameof(GarageAndToolsTab.SwitchInteractiveObjects))]
    [HarmonyPrefix]
    public static void  SwitchInteractiveObjectsHook(string upgradeID, bool on)
    {
        if (!Client.Instance.isConnected || !listenToUpgrades) { listenToUpgrades = true; return;}
        
        if(!ClientData.GameReady && !Server.Instance.isRunning) return;
        
        MelonLogger.Msg($"[GarageUpgradeHooks-> SwitchInteractiveObjectsHook] Triggered: {upgradeID}, {on}");
        ClientData.Instance.garageUpgrades[upgradeID] = new GarageUpgrade(upgradeID, on);
        ClientSend.GarageUpgradePacket(ClientData.Instance.garageUpgrades[upgradeID]);

    }
    
    [HarmonyPatch(typeof(GarageAndToolsTab), nameof(GarageAndToolsTab.UpdateSkillState))]
    [HarmonyPrefix]
    public static void UpdateSkillStateHook(UpgradeItem upgradeItem, UpgradeState state)
    {
        if (!Client.Instance.isConnected || !listenToUpgrades) { listenToUpgrades = true; return;}
        
        if(!ClientData.GameReady && !Server.Instance.isRunning) return;
        
        MelonLogger.Msg($"[GarageUpgradeHooks-> Pre-UpdateSkillStateHook] Triggered: {upgradeItem.upgradeID}, {state}");
        /*ClientData.Instance.garageUpgrades[upgradeID] = new GarageUpgrade(upgradeID, on);
        ClientSend.GarageUpgradePacket(ClientData.Instance.garageUpgrades[upgradeID]);*/

    }
    
    [HarmonyPatch(typeof(GarageAndToolsTab), nameof(GarageAndToolsTab.UnlockCurrentSelectedSkillAction))]
    [HarmonyPrefix]
    public static void UnlockCurrentSelectedSkillActionHook()
    {
        if (!Client.Instance.isConnected || !listenToUpgrades) { listenToUpgrades = true; return;}
        
        if(!ClientData.GameReady && !Server.Instance.isRunning) return;
        
        MelonLogger.Msg($"[GarageUpgradeHooks-> Pre-UnlockCurrentSelectedSkillActionHook] Triggered");
        /*ClientData.Instance.garageUpgrades[upgradeID] = new GarageUpgrade(upgradeID, on);
        ClientSend.GarageUpgradePacket(ClientData.Instance.garageUpgrades[upgradeID]);*/

    }

}