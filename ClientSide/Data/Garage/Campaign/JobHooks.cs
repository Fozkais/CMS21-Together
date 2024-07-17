using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.Shared.Data.Vanilla.Jobs;
using HarmonyLib;
using Il2Cpp;
using Il2CppCMS.UI.Windows;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Campaign;

[HarmonyPatch]
public static class JobHooks
{
    [HarmonyPatch(typeof(OrderGenerator), nameof(OrderGenerator.GenerateMission))]
    [HarmonyPostfix]
    public static void GenerateMissionHook(int id, bool forTutorial, OrderGenerator __instance) 
    {
        if (!Client.Instance.isConnected) return;
        
        MelonLogger.Msg($"[Hook->GenerateMissionHook] Generated new mission : {id}");
        Job newJob = __instance.jobs._items[__instance.jobs.Count - 1];
        ModJob job = new ModJob(newJob);
        ClientSend.JobPacket(job);
    }
         
    [HarmonyPatch(typeof(OrderGenerator), nameof(OrderGenerator.GenerateNewJob))]
    [HarmonyPostfix]
    public static void GenerateNewJobHook() 
    {
        if (!Client.Instance.isConnected) return;
        
        MelonLogger.Msg($"[Hook->GenerateNewJobHook] Generated new job");
        OrderGenerator generator = GameData.Instance.orderGenerator;
        MelonLogger.Msg($"pass1");
        Job newJob = generator.jobs._items[generator.jobs.Count - 1];
        MelonLogger.Msg($"pass2");
        ModJob job = new ModJob(newJob);
        MelonLogger.Msg($"pass3");
        ClientSend.JobPacket(job);
        MelonLogger.Msg($"pass4");
    }
    
    [HarmonyPatch(typeof(OrdersWindow), nameof(OrdersWindow.AcceptOrderAction))]
    [HarmonyPrefix]
    public static void AcceptOrderActionHook(OrdersWindow __instance)
    {
        if (!Client.Instance.isConnected) return;
        
        MelonLogger.Msg($"[Hook->AcceptOrderActionHook] Accept Order : {__instance.currentJob.id}");
        ClientSend.JobActionPacket(__instance.currentJob.id, true);

    }
    
    [HarmonyPatch(typeof(OrdersWindow), nameof(OrdersWindow.DeclineOrderAction))]
    [HarmonyPrefix]
    public static void DeclineOrderActionHook(OrdersWindow __instance)
    {
        if (!Client.Instance.isConnected) return;
        
        MelonLogger.Msg($"[Hook->DeclineOrderActionHook] Decline Order : {__instance.currentJob.id}");
        ClientSend.JobActionPacket(__instance.currentJob.id, false);
    }
    
    
    
    [HarmonyPatch(typeof(OrderGenerator), nameof(OrderGenerator.Update))]
    [HarmonyPrefix]
    public static bool UpdateHook(OrderGenerator __instance) // Disable JobSystem for clients
    {
        if (!Client.Instance.isConnected) return true;

        if (!Server.Instance.isRunning)
            return false;

        return true;
    }
}