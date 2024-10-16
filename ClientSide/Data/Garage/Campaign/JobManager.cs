using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

	public static void UpdateSelectedJob()
	{
		if (GameData.Instance == null || GameData.Instance.orderGenerator == null) return;
		if (GameData.Instance.orderGenerator.selectedJobs == null) return;

		for (var i = 0; i < GameData.Instance.orderGenerator.selectedJobs._items.Count; i++)
		{
			if (i >= GameData.Instance.orderGenerator.selectedJobs._items.Length)
				break;
			
			var job = GameData.Instance.orderGenerator.selectedJobs._items[i];
			if (job != null && selectedJobs.All(j => j != null && job.id != j.id))
			{
				var newJob = new ModJob(job);
				selectedJobs.Add(newJob);
				ClientSend.SelectedJobPacket(newJob, true);
			}
		}

		for (var index = 0; index < selectedJobs.Count; index++)
		{
			var job = selectedJobs[index];
			if (GameData.Instance.orderGenerator.selectedJobs == null ||
			    GameData.Instance.orderGenerator.selectedJobs._items.All(j => j != null && job.id != j.id))
			{
				ClientSend.SelectedJobPacket(job, false);
				selectedJobs.Remove(job);
			}
		}
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
				gameJobs.Remove(gameJobs._items.First(j => j.id == modjob.id));
			}
		}
	}

	public static IEnumerator AddJob(ModJob job)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		var newJob = job.ToGame();

		GameData.Instance.orderGenerator.jobs.Add(newJob);
		GlobalData.AddJob(1);
		UIManager.Get().UpdateJobs(GameData.Instance.orderGenerator.jobs, newJob);
	}

	public static IEnumerator JobAction(int jobID, bool takeJob)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		var exist = false;
		var job = new Job();
		foreach (var _job in GameData.Instance.orderGenerator.jobs)
			if (_job.id == jobID)
			{
				exist = true;
				job = _job;
				break;
			}

		if (!exist) yield break;

		if (takeJob)
		{
			if (job.IsMission)
				MainMod.StartCoroutine(GameData.Instance.orderGenerator.TakeMission(job.id, false));
			else
				MainMod.StartCoroutine(GameData.Instance.orderGenerator.TakeJob(job.id, false));
		}
		else
		{
			GameData.Instance.orderGenerator.CancelJob(job.id);
		}
	}

	public static void GenerateJob(bool reset)
	{
		if (reset) ClearJobs();
		GameData.Instance.orderGenerator.GenerateNewJob();
	}

	public static void GenerateMission(int id, bool reset)
	{
		if (reset) ClearJobs();
		GameData.Instance.orderGenerator.GenerateMission(id);
	}

	public static void ClearJobs()
	{
		GameData.Instance.orderGenerator.jobs.Clear();
		GlobalData.Jobs = 0;
		GlobalData.PrevJobsAmount = 0;
		GlobalData.AddJobsAmount = 0;

		UIManager.Get().UpdateJobs(GameData.Instance.orderGenerator.jobs, null);
	}

	public static IEnumerator OnJobComplete(ModJob job, int carloaderID)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		MelonLogger.Msg("[JobManager] -> OnJobComplete");

		var script = GameScript.Get();
		var _job = Singleton<GameManager>.Instance.OrderGenerator.selectedJobs._items.First(j => j.id == job.id);

		var flag = script.CurrentSceneType == SceneType.Tutorial;
		if (!flag && _job.IsCompleted) Singleton<GameManager>.Instance.Inventory.TryAddSpecialCase(_job.IsMission);
		if (!flag)
		{
			GlobalData.AddPlayerMoney(_job.TotalPayout);
			MelonLogger.Msg($"[JobManager] -> AddPlayerMoney() {_job.TotalPayout}");
		}

		if (!flag && _job.IsCompleted)
		{
			MelonLogger.Msg("[JobManager] -> AddPlayerExp()");
			GlobalData.AddPlayerExp(_job.XP);
		}

		Singleton<GameManager>.Instance.OrderGenerator.CancelJob(_job.id);
		if (!flag && _job.IsMission)
		{
			MelonLogger.Msg("[JobManager] -> OnJobComplete() Finish mission");
			GlobalData.IsStoryMissionInProgress = false;
			GlobalData.MissionsFinished++;
			GlobalData.CurrentMissionDone = true;
			if (GlobalData.MissionsFinished >= GlobalData.MissionsAmount) Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_finish_allmissions", 1);
		}

		GameData.Instance.carLoaders[carloaderID].DeleteCar(true);
		script.SetCarLoaderOverNull();
		script.GarageOnFootWithoutFader();
		if (!flag) GarageLoader.Get().Save();
		GameScript.Get().raycast.enabled = true;
		if (_job.IsCompleted) Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_finish_order", 1);
		if (_job.IsCompleted && _job.BonusToExp) Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_bonus_exp", 1);
		if (_job.IsCompleted && _job.BonusToMoney) Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_bonus_money", 1);
		MelonLogger.Msg("[JobManager] -> OnJobComplete() Finished !");
	}
}