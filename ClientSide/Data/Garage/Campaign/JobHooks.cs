using System.Linq;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.Shared.Data.Vanilla.Jobs;
using HarmonyLib;
using Il2Cpp;
using Il2CppCMS.Extensions;
using Il2CppCMS.UI;
using Il2CppCMS.UI.Windows;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Campaign;

[HarmonyPatch]
public static class JobHooks
{
    [HarmonyPatch(typeof(OrderGenerator), nameof(OrderGenerator.GenerateMission))] [HarmonyPrefix]
    public static bool PreGenerateMissionHook(int id, bool forTutorial)
    {
        if (!Client.Instance.isConnected) return true;

        if (!Server.Instance.isRunning)
            return false;
        
        return true;
    }

    [HarmonyPatch(typeof(OrderGenerator), nameof(OrderGenerator.GenerateMission))] [HarmonyPostfix]
    public static void GenerateMissionHook(int id, bool forTutorial, OrderGenerator __instance) 
    {
        if (!Client.Instance.isConnected) return;

        if (Server.Instance.isRunning)
        {
            MelonLogger.Msg($"[Hook->GenerateMissionHook] Generated new mission : {id}");
            Job newJob = __instance.jobs._items[__instance.jobs.Count - 1];
            ModJob job = new ModJob(newJob);
            ClientSend.JobPacket(job);
        }
    }

    [HarmonyPatch(typeof(OrderGenerator), nameof(OrderGenerator.GenerateNewJob))] [HarmonyPrefix]
    public static bool PreGenerateNewJobHook()
    {
        if (!Client.Instance.isConnected) return true;
        
        if (!Server.Instance.isRunning)
            return false;
        return true;
    }

    [HarmonyPatch(typeof(OrderGenerator), nameof(OrderGenerator.GenerateNewJob))] [HarmonyPostfix]
    public static void GenerateNewJobHook() 
    {
        if (!Client.Instance.isConnected) return;
        
        MelonLogger.Msg($"[Hook->GenerateNewJobHook] Generated new job");
        OrderGenerator generator = GameData.Instance.orderGenerator;
        Job newJob = generator.jobs._items[generator.jobs.Count - 1];
        ModJob job = new ModJob(newJob);
        ClientSend.JobPacket(job);
    }
    
    [HarmonyPatch(typeof(OrdersWindow), nameof(OrdersWindow.AcceptOrderAction))] [HarmonyPrefix]
    public static void AcceptOrderActionHook(OrdersWindow __instance)
    {
        if (!Client.Instance.isConnected) return;
        
        MelonLogger.Msg($"[Hook->AcceptOrderActionHook] Accept Order : {__instance.currentJob.id}");
        ClientSend.JobActionPacket(__instance.currentJob.id, true);

    }
    
    [HarmonyPatch(typeof(OrdersWindow), nameof(OrdersWindow.DeclineOrderAction))] [HarmonyPrefix]
    public static void DeclineOrderActionHook(OrdersWindow __instance)
    {
        if (!Client.Instance.isConnected) return;
        
        MelonLogger.Msg($"[Hook->DeclineOrderActionHook] Decline Order : {__instance.currentJob.id}");
        ClientSend.JobActionPacket(__instance.currentJob.id, false);
    }
    
    [HarmonyPatch(typeof(OrderGenerator), nameof(OrderGenerator.Update))] [HarmonyPrefix]
    public static bool UpdateHook(OrderGenerator __instance)
    {
        if (!Client.Instance.isConnected || Server.Instance.isRunning) return true;

        if (!NotificationCenter.IsGameReady || !GameSettings.CanGenerateOrders)
        {
            return false;
        }
        int maxOrdersAmount = GlobalData.GetMaxOrdersAmount();
        if (GlobalData.Jobs < maxOrdersAmount)
        {
            __instance.orderTimer += Time.deltaTime; // enable timer advance and disable job generation for clients
        }
        
        return false;
    }
    
    [HarmonyPatch(typeof(GameScript), nameof(GameScript.EndJob))] [HarmonyPrefix]
    public static bool EndJobHook(Job job, CarLoader carLoader) // TODO:Handle exp gain for everyone
    {
        if (!Client.Instance.isConnected || Server.Instance.isRunning) return true;

        if (!carLoader.CheckCarPartsBolts())
        {
            UIManager.Get().ShowInfoWindow<string>("GUI_SamochodNiezlozony", ("!" + carLoader.GetMissingPartID()).Localize());
            return false;
        }
        if (!carLoader.CheckIfHaveBody())
        {
            UIManager.Get().ShowInfoWindow<string>("GUI_SamochodNiezlozony", carLoader.CheckCarBodyMissingPartID.Localize());
            return false;
        }
        if (!carLoader.EngineData.isElectric && ((job.HaveSubtype("Oil") && carLoader.FluidsData.GetLevel(CarFluidType.EngineOil, 0, false) < job.oilLevel) || (!job.HaveSubtype("Oil") && !carLoader.CheckCarHaveOil())))
        {
            UIManager.Get().ShowInfoWindow("GUI_SamochodOlejBrak");
            return false;
        }
        CarFluidType carFluidType;
        if (!carLoader.CheckCarHaveFluids(out carFluidType, job))
        {
            UIManager.Get().ShowInfoWindow<string>("GUI_SamochodPlynBrak", carFluidType.Localize());
            return false;
        }
        if (carLoader.CheckScam(job))
        {
            UIManager.Get().ShowInfoWindow("GUI_SamochodGorszyStan");
            return false;
        }
        if (!carLoader.FrontWheelsHaveThisSameSize())
        {
            UIManager.Get().ShowInfoWindow("GUI_FrontWheelsDiffSize");
            return false;
        }
        if (!carLoader.RearWheelsHaveThisSameSize())
        {
            UIManager.Get().ShowInfoWindow("GUI_RearWheelsDiffSize");
            return false;
        }
        WindowManager.Instance.Hide(WindowID.CarInfo, true); // hide info panel
        
        // Only accept job end if is valid
        ModJob modJob = JobManager.selectedJobs.First(j => j.id == job.id);
        int carLoaderID = (carLoader.gameObject.name[10] - '0') - 1;

        ClientSend.EndJobPacket(modJob, carLoaderID);
        
        return false;
    }
}