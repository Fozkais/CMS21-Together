using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS;
using CMS.FileSupport.INI;
using CMS.Tutorial;
using CMS.UI;
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
				MelonCoroutines.Start(TakeMission(job.id));
			else
				TakeJob(job);
			
			MelonLogger.Msg("CL: Took Job!");
		}
		else
		{
			GameData.Instance.orderGenerator.CancelJob(job.id);
		}
	}

	public static IEnumerator TakeMission(int id)
	{
		Job job = GameData.Instance.orderGenerator.jobs.ToArray().First(x => x.id == id);
		CarLoader carLoader = ((GameScript.Get().CurrentSceneType == SceneType.Tutorial) ? SceneManager.Get().GetPlaceForLoadCar() : CarLoaderPlaces.Get().GetPlaceForLoadCar());
		bool forTutorial = GameScript.Get().CurrentSceneType == SceneType.Tutorial;
		TextAsset textAsset = Resources.Load<TextAsset>(forTutorial ? string.Format("MissionsTutorial/TutorialMission{0}", id) : string.Format("Missions/Mission{0}", GlobalData.MissionsFinished));
		IniData ini = IniParser.Parse(textAsset);
		if (job.carFile != ini.GetString("carToLoad", "General"))
		{
			GameData.Instance.orderGenerator.jobs.Remove(job);
			UIManager.Get().UpdateJobs(GameData.Instance.orderGenerator.jobs, null);
			GameData.Instance.orderGenerator.GenerateMission(GlobalData.GetMissionID(), false);
			job.CanDelete = true;
			yield break;
		}
		carLoader.ConfigVersion = ini.GetInt("carConfigVersion", "General", 0);
		if (Singleton<GameManager>.Instance.CarBundleLoader.GetConfigCounts(job.carFile) < carLoader.ConfigVersion + 1)
		{
			Debug.LogWarning(string.Format("[OrderGenerator] -> TakeMission() Car has {0} config files. We need config {1}. Loading config 0.", Singleton<GameManager>.Instance.CarBundleLoader.GetConfigCounts(job.carFile), carLoader.ConfigVersion + 1));
			carLoader.ConfigVersion = 0;
		}
		string licensePlateNumber = ini.GetString("carLicensePlate", "General", "");
		carLoader.SetCustomerCar(true, job.id);
		carLoader.CarInfoData = carLoader.CarInfoData with { CarFrom = CarFrom.Mission };
		GameData.Instance.orderGenerator.StartCoroutine(carLoader.LoadCar(job.carFile));
		carLoader.CarInfoData = carLoader.CarInfoData with { Mileage = ini.GetInt("carMileage", "General", 82000) };
		while (!carLoader.IsCarLoaded())
		{
			yield return YieldInstructions.WaitForEndOfFrame;
		}
		yield return YieldInstructions.WaitForEndOfFrame;
		carLoader.PlaceAtPosition(true, true);
		carLoader.SetNewLicensePlateNumber(licensePlateNumber, true);
		carLoader.SetNewLicensePlateNumber(licensePlateNumber, false);
		
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		if (ini.GetBool("carPartsRepair", "General", false))
		{
			num++;
		}
		if (ini.GetBool("carBodyRepair", "General"))
		{
			num++;
		}
		if (ini.GetBool("carChangeOil", "General"))
		{
			num++;
		}
		if (ini.GetBool("carPaintJob", "General"))
		{
			num++;
		}
		if (ini.GetBool("wheelsAlignment", "General", false) || ini.GetBool("headlampAlignment", "General", false) || ini.GetInt("increaseTuneValue", "General", 0) > 0 || ini.GetBool("brakeRefill", "General", false) || ini.GetBool("coolantRefill", "General", false) || ini.GetBool("windscreenWashRefill", "General", false) || ini.GetBool("powerSteeringRefill", "General", false) || ini.GetBool("brakeChange", "General", false) || ini.GetBool("coolantChange", "General", false) || ini.GetBool("windscreenWashChange", "General", false) || ini.GetBool("powerSteeringChange", "General", false))
		{
			num++;
		}
		int num4 = -1;
		job.jobTasks = new JobTask[num];
		if (ini.GetBool("carPartsRepair", "General", false))
		{
			num4++;
			num3 = num4;
			job.jobTasks[num4] = new JobTask
			{
				Parts = new Il2CppSystem.Collections.Generic.List<JobPart>(),
				type = "Mission",
				subtype = "General"
			};
		}
		if (ini.GetBool("carBodyRepair", "General", false))
		{
			num4++;
			num2 = num4;
			job.jobTasks[num4] = new JobTask
			{
				Parts = new Il2CppSystem.Collections.Generic.List<JobPart>(),
				type = "Body",
				subtype = "General",
				desc = ini.GetString("localizationIDBody", "General")
			};
		}
		if (ini.GetBool("carChangeOil", "General", false))
		{
			num4++;
			job.jobTasks[num4] = new JobTask
			{
				Parts = new Il2CppSystem.Collections.Generic.List<JobPart>(),
				type = "Engine",
				subtype = "Oil",
				desc = ini.GetString("localizationIDoil", "General")
			};
		}
		if (ini.GetBool("carPaintJob", "General", false))
		{
			num4++;
			job.jobTasks[num4] = new JobTask
			{
				Parts = new Il2CppSystem.Collections.Generic.List<JobPart>(),
				type = "Body",
				subtype = "PaintOriginal",
				desc = ini.GetString("localizationIDpaint", "General")
			};
		}
		if (ini.GetBool("wheelsAlignment", "General", false) || ini.GetBool("headlampAlignment", "General", false) || ini.GetInt("increaseTuneValue", "General", 0) > 0 || ini.GetBool("brakeRefill", "General", false) || ini.GetBool("coolantRefill", "General", false) || ini.GetBool("windscreenWashRefill", "General", false) || ini.GetBool("powerSteeringRefill", "General", false) || ini.GetBool("brakeChange", "General", false) || ini.GetBool("coolantChange", "General", false) || ini.GetBool("windscreenWashChange", "General", false) || ini.GetBool("powerSteeringChange", "General", false))
		{
			num4++;
			job.jobTasks[num4] = new JobTask
			{
				Parts = new Il2CppSystem.Collections.Generic.List<JobPart>(),
				type = "Additionals",
				subtype = "Additionals",
				desc = ini.GetString("localizationIDadditionals", "General")
			};
			if (ini.GetBool("wheelsAlignment", "General", false))
			{
				job.jobTasks[num4].Parts.Add(new JobPart("WheelsAlignment"));
				carLoader.SetRandomWheelsAlignment();
			}
			if (ini.GetBool("headlampAlignment", "General", false))
			{
				job.jobTasks[num4].Parts.Add(new JobPart("HeadlampAlignment"));
				carLoader.SetRandomHeadlampAlignment();
			}
			if (ini.GetInt("increaseTuneValue", "General", 0) > 0)
			{
				job.jobTasks[num4].Parts.Add(new JobPart("IncreaseTuneValue"));
				job.jobTasks[num4].IncreaseTuneValue = ini.GetInt("increaseTuneValue", "General");
			}
			if (ini.GetBool("brakeRefill", "General", false))
			{
				job.jobTasks[num4].Parts.Add(new JobPart("BrakeRefill"));
			}
			if (ini.GetBool("coolantRefill", "General", false))
			{
				job.jobTasks[num4].Parts.Add(new JobPart("CoolantRefill"));
			}
			if (ini.GetBool("windscreenWashRefill", "General", false))
			{
				job.jobTasks[num4].Parts.Add(new JobPart("WindscreenWashRefill"));
			}
			if (ini.GetBool("powerSteeringRefill", "General", false))
			{
				job.jobTasks[num4].Parts.Add(new JobPart("PowerSteeringRefill"));
			}
			if (ini.GetBool("brakeChange", "General", false))
			{
				job.jobTasks[num4].Parts.Add(new JobPart("BrakeChange"));
			}
			if (ini.GetBool("coolantChange", "General", false))
			{
				job.jobTasks[num4].Parts.Add(new JobPart("CoolantChange"));
			}
			if (ini.GetBool("windscreenWashChange", "General", false))
			{
				job.jobTasks[num4].Parts.Add(new JobPart("WindscreenWashChange"));
			}
			if (ini.GetBool("powerSteeringChange", "General", false))
			{
				job.jobTasks[num4].Parts.Add(new JobPart("PowerSteeringChange"));
			}
		}
		carLoader.FluidsData.SetLevelAndConditionOnAll(ini.GetFloat("powerSteeringLevel", "General", 1f), ini.GetFloat("powerSteeringCondition", "General", 1f), CarFluidType.PowerSteering);
		carLoader.FluidsData.SetLevelAndConditionOnAll(ini.GetFloat("coolantLevel", "General", 1f), ini.GetFloat("coolantCondition", "General", 1f), CarFluidType.EngineCoolant);
		carLoader.FluidsData.SetLevelAndConditionOnAll(ini.GetFloat("windscreenWashLevel", "General", 1f), ini.GetFloat("windscreenWashCondition", "General", 1f), CarFluidType.WindscreenWash);
		carLoader.FluidsData.SetLevelAndConditionOnAll(ini.GetFloat("brakeLevel", "General", 1f), ini.GetFloat("brakeCondition", "General", 1f), CarFluidType.Brake);
		carLoader.FluidsData.SetLevelAndConditionOnAll(ini.GetFloat("carOilLevel", "General", 1f), ini.GetFloat("carOilCondition", "General", 1f), CarFluidType.EngineOil);
		carLoader.SetFactoryColor(job.carFactoryColor);
		carLoader.SetFactoryPaintType(job.carFactoryPaintType);
		carLoader.SetCarColor(null, job.carColor);
		carLoader.SetCarPaintType(null, job.PaintType);
		if (ini.GetString("carOtherPartsConditionRange", "General", "").Equals(""))
		{
			float @float = ini.GetFloat("carOtherPartsCondition", "General", 1f);
			carLoader.SetRandomPartsConditions(@float - 0.05f, @float + 0.05f);
		}
		else
		{
			Vector2 vector = ini.GetVector2("carOtherPartsConditionRange", "General", Vector2.zero);
			carLoader.SetRandomPartsConditions(vector.x, vector.y);
		}
		float float2 = ini.GetFloat("carBodyCondition", "General", 1f);
		float float3 = ini.GetFloat("carInteriorCondition", "General", float2);
		carLoader.SetRustRandomParts(float2, float2);
		carLoader.SetConditionOnDetails(float3);
		float float4 = ini.GetFloat("dustValue", "General", 0f);
		if (float4 > 0f)
		{
			carLoader.EnableDust(null, float4);
		}
		float float5 = ini.GetFloat("washFactor", "General", 0f);
		if (float5 > 0f)
		{
			carLoader.SetWashFactor(null, float5);
		}
		if (ini.GetBool("outsideRust", "General", false))
		{
			carLoader.EnableRustOutside(null, true);
		}
		job.jobTasks[0].desc = ini.GetString("localizationID", "General");
		float float6 = ini.GetFloat("bodyDentValue", "General", 1f);
		if (float6 < 1f)
		{
			carLoader.SetDent(carLoader.GetCarPart("body"), float6);
		}
		DifficultyLevel difficultyLevel = Singleton<GameManager>.Instance.DifficultyManager.GetDifficultyLevel();
		for (int i = 0; i < 1000; i++)
		{
			if (string.IsNullOrEmpty(ini.GetString("id", string.Format("part{0}", i))))
			{
				job.jobTasks[0].partsCount = i;
				Debug.Log(string.Format("[OrderGenerator] -> TakeMission() There was {0} parts in file", i));
				break;
			}
			string @string = ini.GetString("id", string.Format("part{0}", i));
			Transform transform = carLoader.GetRoot().transform.FindChild(@string);
			if (!transform)
			{
				Debug.LogError("[OrderGenerator] -> TakeMission() Part " + @string + " not found in this car.");
			}
			else
			{
				PartScript component = transform.GetComponent<PartScript>();
				if (component != null)
				{
					float float7 = ini.GetFloat("condition", string.Format("part{0}", i));
					component.SetConditionNormal(float7);
					if (difficultyLevel == DifficultyLevel.Easy)
					{
						component.IsExamined = true;
					}
					else
					{
						component.IsExamined = ini.GetBool("examined", string.Format("part{0}", i));
					}
					if (float7 < 1f && ini.GetBool("addToMission", string.Format("part{0}", i), true))
					{
						job.jobTasks[num3].Parts.Add(new JobPart(@string));
					}
					if (!ini.GetBool("exist", string.Format("part{0}", i)))
					{
						component.IsExamined = true;
						component.HideBySavegame(true, null);
					}
				}
				else
				{
					CarPart carPart = carLoader.GetCarPart(@string);
					AllowedColor allowedColor;
					CarHelper.ProcessThumbnailColor(out allowedColor, ini.GetString("color", string.Format("part{0}", i)).Trim());
					carLoader.SetCarColor(carPart, allowedColor.Color);
					carLoader.SetCarPaintType(carPart, allowedColor.PaintType);
					float float8 = ini.GetFloat("condition", string.Format("part{0}", i));
					carLoader.SetCondition(carPart, float8);
					carLoader.UpdateCarBodyParts();
					if (float8 < 1f && ini.GetBool("addToMission", string.Format("part{0}", i), true))
					{
						job.jobTasks[num2].Parts.Add(new JobPart(@string));
					}
					if (!ini.GetBool("exist", string.Format("part{0}", i)))
					{
						carLoader.TakeOffCarPartFromSave(@string);
					}
					float float9 = ini.GetFloat("dentWeight", string.Format("part{0}", i), 1f);
					if (float9 < 1f)
					{
						carLoader.SetDent(carPart, float9);
					}
				}
			}
		}
		carLoader.PlaceAtPosition(true, true);
		yield return YieldInstructions.WaitForEndOfFrame;
		GlobalData.AddJob(-1);
		job.carLoaderID = CarLoaderPlaces.Get().GetCarLoaderId(carLoader);
		GameData.Instance.orderGenerator.selectedJobs.Add(job);
		GameData.Instance.orderGenerator.jobs.Remove(job);
		UIManager.Get().UpdateJobs(GameData.Instance.orderGenerator.jobs, null);
		job.otherPartsCondition = carLoader.GetOtherPartsCondition(job);
		job.globalCondition = job.otherPartsCondition;
		GlobalData.IsStoryMissionInProgress = true;
		if (GameData.Instance.orderGenerator.OnTakeMission != null)
			GameData.Instance.orderGenerator.OnTakeMission.Invoke();
	}

	public static void TakeJob(Job job)
	{
		GameData.Instance.orderGenerator.selectedJobs.Add(job);
		GameData.Instance.orderGenerator.CancelJob(job.id);
		UIManager.Get().UpdateJobs(GameData.Instance.orderGenerator.jobs, null);
	}
	
	public static IEnumerator OnJobComplete(ModJob job)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		MelonLogger.Msg("[JobManager] -> OnJobComplete");
		MelonLogger.Msg("- Job Info received by Host -");
		MelonLogger.Msg($"ID:{job.id}");
		MelonLogger.Msg($"IsMission:{job.IsMission}");
		MelonLogger.Msg($"isCompleted:{job.IsCompleted}");
		MelonLogger.Msg($"Payout:{job.TotalPayout}");
		MelonLogger.Msg($"XP:{job.XP}");
		MelonLogger.Msg($"MoneySpent:{job.MoneySpent}");
		
		GlobalData.AddPlayerExp(job.XP);

		Singleton<GameManager>.Instance.OrderGenerator.CancelJob(job.id);
		if (job.IsMission)
		{
			GlobalData.IsStoryMissionInProgress = false;
			GlobalData.MissionsFinished++;
			GlobalData.CurrentMissionDone = true;
			if (GlobalData.MissionsFinished >= GlobalData.MissionsAmount) Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_finish_allmissions", 1);
		}
		
		if ( selectedJobs.Any(j => j.id == job.id))
		{
			var modJob = selectedJobs.First(j => j.id == job.id);
			selectedJobs.Remove(modJob);
		}
		
		if (job.IsCompleted) Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_finish_order", 1);
		if (job.IsCompleted && job.BonusToExp) Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_bonus_exp", 1);
		if (job.IsCompleted && job.BonusToMoney) Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_bonus_money", 1);
		MelonLogger.Msg("[JobManager] -> OnJobComplete() Finished !");
	}
}