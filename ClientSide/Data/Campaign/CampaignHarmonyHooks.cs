using CMS21Together.ClientSide.Handle;
using HarmonyLib;
using Il2Cpp;
using Il2CppCMS.UI.Logic;
using Il2CppCMS.UI.Logic.Navigation;
using Il2CppCMS.UI.Logic.Upgrades;
using MelonLoader;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace CMS21Together.ClientSide.Data.Campaign
{
    [HarmonyPatch]
    public static class CampaignHarmonyHooks
    {

        public static bool ListenToUpgrades = true;
        
        [HarmonyPatch(typeof(GarageAndToolsTab), nameof(GarageAndToolsTab.SwitchInteractiveObjects))]
        [HarmonyPrefix]
        public static void SwitchInteractiveObjectsHook(string upgradeID, bool on, GarageAndToolsTab __instance)
        {
            if (!Client.Instance.isConnected) return;
            
            if (ListenToUpgrades)
            {
                MelonLogger.Msg($"[Hook->GarageAndToolsTab] SwitchInteractive : {upgradeID}, {on}");
                ClientData.Instance.garageUpgrades.Add((true, upgradeID), on);
                ClientSend.GarageUpdgrade(true, upgradeID, on);
            }
            else
            {
                ListenToUpgrades = true;
            }
        }
        
        [HarmonyPatch(typeof(GarageAndToolsTab), nameof(GarageAndToolsTab.SwitchObjectsUnlock))]
        [HarmonyPrefix]
        public static void SwitchObjectsUnlocksHook(string upgradeID, bool unlockUpgrade, bool withFade=false)
        {
            if (!Client.Instance.isConnected) return;
            
            if (ListenToUpgrades)
            {
                MelonLogger.Msg($"[Hook->GarageAndToolsTab] SwitchObjectsUnlock : {upgradeID}, {unlockUpgrade}, {withFade}");
                ClientData.Instance.garageUpgrades.Add((false, upgradeID), unlockUpgrade);
                ClientSend.GarageUpdgrade(false, upgradeID, unlockUpgrade);
            }
            else
            {
                ListenToUpgrades = true;
            }
        }
        
        [HarmonyPatch(typeof(OrderGenerator), nameof(OrderGenerator.TakeJob))]
        [HarmonyPrefix]
        public static void TakeJobHook(int id, bool movePlayerToCar = true)
        {
           MelonLogger.Msg($"[Hook->OrderGenerator] TakeJob : {id}, {movePlayerToCar}");
        }
         
        [HarmonyPatch(typeof(OrderGenerator), nameof(OrderGenerator.PrepareJob))]
        [HarmonyPrefix]
        public static void PrepareJobHook(CarLoader carLoader, Job job)
        {
            MelonLogger.Msg($"[Hook->OrderGenerator] PrepareJob : {carLoader.carToLoad}, {job.id}");
        }
        
        [HarmonyPatch(typeof(OrderGenerator), nameof(OrderGenerator.TakeMission))]
        [HarmonyPrefix]
        public static void TakeMissionHook(int id, bool movePlayerToCar = true)
        {
            MelonLogger.Msg($"[Hook->OrderGenerator] TakeMission : {id}, {movePlayerToCar}");
        }
    }
}