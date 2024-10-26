using System.Linq;
using CMS.Extensions;
using CMS.FileSupport.INI;
using CMS.UI;
using CMS.UI.Windows;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.Shared.Data.Vanilla.Jobs;
using HarmonyLib;
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
		if (__instance.jobs.Count <= 0) return;
		
		if (Server.Instance.isRunning)
		{
			foreach (Job job in __instance.jobs.ToArray())
			{
				MelonLogger.Msg($"[Hook->LoadHook] Sent new job.");
				ClientSend.JobPacket(new ModJob(job));
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
		if (__instance.jobs.Count <= 0) return;
		
		if (Server.Instance.isRunning)
		{
			
			MelonLogger.Msg($"[Hook->GenerateMissionHook] Generated new mission : {id}");
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
		MelonLogger.Msg("[Hook->GenerateNewJobHook] Generated new job");
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
	public static bool UpdateHook(OrderGenerator __instance)
	{
		if (!Client.Instance.isConnected || Server.Instance.isRunning) return true;

		if (!NotificationCenter.IsGameReady || !GameSettings.CanGenerateOrders) return false;
		var maxOrdersAmount = GlobalData.GetMaxOrdersAmount();
		if (GlobalData.Jobs < maxOrdersAmount) __instance.orderTimer += Time.deltaTime; // enable timer advance and disable job generation for clients

		return false;
	}

	[HarmonyPatch(typeof(GameScript), nameof(GameScript.EndJob))]
	[HarmonyPrefix]
	public static bool EndJobHook(Job job, CarLoader carLoader) // TODO:Handle exp gain for everyone
	{
		if (!Client.Instance.isConnected || Server.Instance.isRunning) return true;

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
		
		MelonLogger.Msg("- Job Info before sending it to Host -");
		MelonLogger.Msg($"ID:{job.id}");
		MelonLogger.Msg($"IsMission:{job.IsMission}");
		MelonLogger.Msg($"isCompleted:{job.IsCompleted}");
		MelonLogger.Msg($"Mileage:{job.Mileage}");
		MelonLogger.Msg($"forXP:{job.forXP}");
		MelonLogger.Msg($"jobBonus:{job.JobBonus}");
		MelonLogger.Msg($"moneySpent:{job.MoneySpent}");
		MelonLogger.Msg($"BonusEXP:{job.BonusToExp}");
		MelonLogger.Msg($"BonusMoney:{job.BonusToMoney}");
		MelonLogger.Msg($"taskBonus:{job.TaskBonus}");

		// Only accept job end if is valid
		var modJob = JobManager.selectedJobs.First(j => j.id == job.id);
		var carLoaderID = carLoader.gameObject.name[10] - '0' - 1;

		ClientSend.EndJobPacket(modJob, carLoaderID);

		return false;
	}
}