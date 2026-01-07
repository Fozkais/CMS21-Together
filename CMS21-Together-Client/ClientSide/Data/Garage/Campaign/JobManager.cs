using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS;
using CMS.FileSupport.INI;
using CMS.Tutorial;
using CMS.UI;
using CMS21Together.ClientSide.Data.Garage.Car;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data.Vanilla.Jobs;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Campaign;

public static class JobManager
{
	public static List<ModJob> selectedJobs = new();

	public static void Reset()
	{
		selectedJobs.Clear();
	}

	public static IEnumerator SelectedJob(ModJob modjob, bool action)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		if (action)
		{
			if (selectedJobs.All(j => modjob.id != j.id))
			{
				selectedJobs.Add(modjob);
				GameData.Instance.orderGenerator.selectedJobs.Add(modjob.ToGame());
			}
		}
		else
		{
			if (selectedJobs.Any(j => modjob.id == j.id))
			{
				var gameJobs = GameData.Instance.orderGenerator.selectedJobs;
				selectedJobs.Remove(selectedJobs.First(j => j.id == modjob.id));
				gameJobs.Remove(gameJobs.ToArray().First(j => j.id == modjob.id));
			}
		}
	}

	public static IEnumerator AddJob(ModJob job)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();
		
		var newJob = job.ToGame();
		newJob.timeToEnd -= 3;
		GameData.Instance.orderGenerator.jobs.Add(newJob);
		if (!newJob.IsMission)
			GameData.Instance.orderGenerator.jobs.ToArray()[GameData.Instance.orderGenerator.jobs.Count-1].StartTimer();
		GlobalData.AddJob(1);
		UIManager.Get().UpdateJobs(GameData.Instance.orderGenerator.jobs, newJob);
		MelonLogger.Msg($"Should have added a Mision! {newJob.id} , {newJob.IsMission}");
	}

	public static IEnumerator JobAction(ModJob modJob, bool takeJob)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		Job job = modJob.ToGame();
		foreach (var _job in GameData.Instance.orderGenerator.jobs.ToArray())
		{
			if (_job.id == modJob.id)
			{
				GameData.Instance.orderGenerator.CancelJob(_job.id);
				break;
			}
		}

		if (takeJob)
		{
			GameData.Instance.orderGenerator.selectedJobs.Add(job);
			UIManager.Get().UpdateJobs(GameData.Instance.orderGenerator.jobs, null);
			MelonLogger.Msg($"CL: Took Job! {job.id} , cLoader {job.carLoaderID}");
		}
	}

	public static IEnumerator OnJobComplete(ModJob job)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		MelonLogger.Msg("[JobManager] -> OnJobComplete");
		MelonLogger.Msg("\n - Job Info received - " +
		                $"\nID:{job.id}" +
		                $"\nIsMission:{job.IsMission}" +
		                $"\nisCompleted:{job.IsCompleted}" +
		                $"\nPayout:{job.TotalPayout}" +
		                $"\nXP:{job.XP}" +
		                $"\nMoneySpent:{job.MoneySpent}" +
		                "\n----------------------------------------");

		GlobalData.AddPlayerExp(job.XP);
		Singleton<GameManager>.Instance.OrderGenerator.CancelJob(job.id);

		CarSpawnHooks.listenToDelete = false;
		GameData.Instance.carLoaders[job.carLoaderID].DeleteCar();
		if (job.IsMission)
		{
			GlobalData.IsStoryMissionInProgress = false;
			GlobalData.MissionsFinished++;
			GlobalData.CurrentMissionDone = true;
			if (GlobalData.MissionsFinished >= GlobalData.MissionsAmount) Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_finish_allmissions", 1);
		}

		if (selectedJobs.Any(j => j.id == job.id))
		{
			var modJob = selectedJobs.First(j => j.id == job.id);
			selectedJobs.Remove(modJob);
			if (job.IsCompleted) Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_finish_order", 1);
			if (job.IsCompleted && job.BonusToExp) Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_bonus_exp", 1);
			if (job.IsCompleted && job.BonusToMoney) Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_bonus_money", 1);
			MelonLogger.Msg("[JobManager] -> OnJobComplete() Finished !");
		}
	}
}