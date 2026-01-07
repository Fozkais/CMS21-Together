using System.Collections;
using System.Linq;
using CMS.Containers;
using CMS.Extensions;
using CMS.FileSupport.INI;
using CMS.PartModules;
using CMS.UI;
using CMS.UI.Windows;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.Shared.Data.Vanilla.Cars;
using CMS21Together.Shared.Data.Vanilla.Jobs;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Campaign;

[HarmonyPatch]
public static class JobHooks
{

	[HarmonyPatch(typeof(OrderGenerator), nameof(OrderGenerator.Load))]
	[HarmonyPostfix]
	public static void LoadHook(OrderGenerator __instance)
	{
		if (!Client.Instance.isConnected) return;
		
		if (Server.Instance.isRunning)
		{
			foreach (Job job in __instance.jobs.ToArray())
			{
				//MelonLogger.Msg($"[Hook->LoadHook] Sent new job.");
				ClientSend.JobPacket(new ModJob(job));
			}
			
			foreach (Job job in __instance.selectedJobs.ToArray())
			{
				//MelonLogger.Msg($"[Hook->LoadHook] Sent new selected job.");
				ClientSend.SelectedJobPacket(new ModJob(job), true);
			}
		}
	}
	[HarmonyPatch(typeof(OrderGenerator), nameof(OrderGenerator.GenerateMission))]
	[HarmonyPrefix]
	public static bool PreGenerateMissionHook(int id, bool forTutorial)
	{
		if (!Client.Instance.isConnected) return true;

		if (!Server.Instance.isRunning)
			return false;

		return true;
	}

	[HarmonyPatch(typeof(OrderGenerator), nameof(OrderGenerator.GenerateMission))]
	[HarmonyPostfix]
	public static void GenerateMissionHook(int id, bool forTutorial, OrderGenerator __instance)
	{
		if (!Client.Instance.isConnected) return;
		if (__instance.jobs.Count <= 0 || id == -1) return;
		
		if (Server.Instance.isRunning)
		{
			
			//MelonLogger.Msg($"[Hook->GenerateMissionHook] Generated new mission : {id}");
			var enumerator = __instance.jobs.ToArray();
			var newJob = enumerator[__instance.jobs.Count - 1];
			var job = new ModJob(newJob);
			ClientSend.JobPacket(job);
		}
	}
	
	[HarmonyPatch(typeof(OrderGenerator), nameof(OrderGenerator.GenerateNewJob))]
	[HarmonyPrefix]
	public static bool PreGenerateNewJobHook()
	{
		if (!Client.Instance.isConnected) return true;

		if (!Server.Instance.isRunning)
			return false;
		return true;
	}
	
	[HarmonyPatch(typeof(OrderGenerator), nameof(OrderGenerator.GenerateNewJob))]
	[HarmonyPostfix]
	public static void GenerateNewJobHook(OrderGenerator __instance)
	{
		if (!Client.Instance.isConnected) return;
		
		if (__instance.jobs.Count <= 0) return;
		//MelonLogger.Msg("[Hook->GenerateNewJobHook] Generated new job");
		var enumerator = __instance.jobs.ToArray();
		var newJob = enumerator[__instance.jobs.Count - 1];
		var job = new ModJob(newJob);
		ClientSend.JobPacket(job);
	}

	[HarmonyPatch(typeof(OrdersWindow), nameof(OrdersWindow.AcceptOrderAction))]
	[HarmonyPrefix]
	public static void AcceptOrderActionHook(OrdersWindow __instance)
	{
		if (!Client.Instance.isConnected) return;

		ClientSend.JobActionPacket(new ModJob(__instance.currentJob), false);
		MelonCoroutines.Start(WaitForCarToBeReady(__instance.currentJob));
	}

	private static IEnumerator WaitForCarToBeReady(Job job)
	{
		while (job.carLoaderID == 0)
			yield return new WaitForSeconds(0.2f);
		
		MelonLogger.Msg($"[Hook->AcceptOrderActionHook] Accept Order : {job.id}, {job.carLoaderID}");
		ClientSend.JobActionPacket(new ModJob(job), true);
	}
	

	[HarmonyPatch(typeof(OrdersWindow), nameof(OrdersWindow.DeclineOrderAction))]
	[HarmonyPrefix]
	public static void DeclineOrderActionHook(OrdersWindow __instance)
	{
		if (!Client.Instance.isConnected) return;

		MelonLogger.Msg($"[Hook->DeclineOrderActionHook] Decline Order : {__instance.currentJob.id}");
		ClientSend.JobActionPacket(new ModJob(__instance.currentJob), false);
	}
	
	[HarmonyPatch(typeof(OrderGenerator), nameof(OrderGenerator.Update))]
	[HarmonyPrefix]
	public static bool UpdateHook(OrderGenerator __instance)
	{
		if (!Client.Instance.isConnected || Server.Instance.isRunning) return true;
		if (!NotificationCenter.IsGameReady || !GameSettings.CanGenerateOrders) return false;
		
		var maxOrdersAmount = GlobalData.GetMaxOrdersAmount();
		if (GlobalData.Jobs < maxOrdersAmount) __instance.orderTimer += Time.deltaTime; // enable timer and disable job generation for clients

		return false;
	}

	[HarmonyPatch(typeof(GameScript), nameof(GameScript.EndJob))]
	[HarmonyPrefix]
	public static bool EndJobHook(Job job, CarLoader carLoader)
	{
		if (!Client.Instance.isConnected) return true;

		if (!carLoader.CheckCarPartsBolts())
		{
			UIManager.Get().ShowInfoWindow("GUI_SamochodNiezlozony", ("!" + Singleton<GameManager>.Instance.Localization.GetLocalizedValue(carLoader.GetMissingPartID())));
			return false;
		}

		if (!carLoader.CheckIfHaveBody())
		{
			UIManager.Get().ShowInfoWindow("GUI_SamochodNiezlozony", Singleton<GameManager>.Instance.Localization.GetLocalizedValue(carLoader.CheckCarBodyMissingPartID));
			return false;
		}

		if (!carLoader.EngineData.isElectric && ((job.HaveSubtype("Oil") && carLoader.FluidsData.GetLevel(CarFluidType.EngineOil) < job.oilLevel) || (!job.HaveSubtype("Oil") && !carLoader.CheckCarHaveOil())))
		{
			UIManager.Get().ShowInfoWindow("GUI_SamochodOlejBrak");
			return false;
		}

		CarFluidType carFluidType;
		if (!carLoader.CheckCarHaveFluids(out carFluidType, job))
		{
			UIManager.Get().ShowInfoWindow("GUI_SamochodPlynBrak", Singleton<GameManager>.Instance.Localization.GetLocalizedValue(carFluidType.ToString()));
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
		
		MelonLogger.Msg("\n - Job Info before sending it to server - " + 
			                $"\nID:{job.id}" +
			                $"\nIsMission:{job.IsMission}" +
			                $"\nisCompleted:{job.IsCompleted}" +
			                $"\nPayout:{job.TotalPayout}" +
			                $"\nXP:{job.XP}" +
			                $"\nMoneySpent:{job.MoneySpent}" +
			                "\n----------------------------------------");

		Singleton<GameManager>.Instance.Inventory.TryAddSpecialCase(job.IsMission);
		GlobalData.AddPlayerMoney(job.TotalPayout);
		GlobalData.AddPlayerExp(job.XP);
		
		var modJob = new ModJob(job);
		if (JobManager.selectedJobs.Any(j => j.id == job.id)) JobManager.selectedJobs.Remove(modJob);
		ClientSend.EndJobPacket(modJob);
		
		Singleton<GameManager>.Instance.OrderGenerator.CancelJob(job.id);
		if (job.IsMission)
		{
			GlobalData.IsStoryMissionInProgress = false;
			GlobalData.MissionsFinished++;
			GlobalData.CurrentMissionDone = true;
			if (GlobalData.MissionsFinished >= GlobalData.MissionsAmount)
			{
				Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_finish_allmissions", 1);
			}
			
		}
		
		carLoader.DeleteCar(true);
		GameScript.Get().SetCarLoaderOverNull();
		GameScript.Get().GarageOnFootWithoutFader();
		
		GarageLoader.Get().Save();
		if (job.IsCompleted) Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_finish_order", 1);
		if (job.IsCompleted && job.BonusToExp) Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_bonus_exp", 1);
		if (job.IsCompleted && job.BonusToMoney) Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_bonus_money", 1);
		
		return false;
	}
}