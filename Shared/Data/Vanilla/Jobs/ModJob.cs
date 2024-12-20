using System;
using System.Linq;

namespace CMS21Together.Shared.Data.Vanilla.Jobs;

[Serializable]
public class ModJob
{
	public int id;
	public int carLoaderID;
	public int forXP;
	public string carFile = "";
	public int configVersion;
	public float timeToEnd;
	public ModPaintType PaintType;
	public ModColor carColor;
	public ModColor carFactoryColor;
	public ModPaintType carFactoryPaintType;
	public int Mileage;
	public float oilLevel;
	public bool[] jobType = new bool[6];
	public ModJobTask[] jobTasks;
	public int jobPartsCount;
	public float globalCondition = 1f;
	public float otherPartsCondition = 1f;
	public bool BonusToExp;
	public bool BonusToMoney;
	public string LocalizationID;
	public bool IsMission;
	public int MissionID;
	public bool CanDelete = true;
	public int TaskBonus;
	public int JobBonus;
	public int TotalPayout;
	public int MoneySpent;
	public int MoneySpentWithDifficultyMod;
	public int XP;
	public bool IsCompleted;
	public bool IconTypeEngine;
	public bool IconTypeTiming;
	public bool IconTypeSuspension;
	public bool IconTypeBrakes;
	public bool IconTypeExhaust;
	public bool IconTypeGearbox;
	public bool IconTypeOil;
	public bool IconTypeBody;
	public bool IconTypeTuning;
	private bool haveToStopTimer;

	public ModJob(Job job)
	{
		id = job.id;
		carLoaderID = job.carLoaderID;
		forXP = job.forXP;
		carFile = job.carFile;
		configVersion = job.configVersion;
		timeToEnd = job.timeToEnd;
		PaintType = (ModPaintType)job.PaintType;
		carColor = new ModColor(job.carColor);
		carFactoryColor = new ModColor(job.carFactoryColor);
		carFactoryPaintType = (ModPaintType)job.carFactoryPaintType;
		Mileage = job.Mileage;
		oilLevel = job.oilLevel;
		jobType = job.jobType;
		jobTasks = new ModJobTask[job.jobTasks.Count];
		for (var index = 0; index < job.jobTasks.Count; index++) jobTasks[index] = new ModJobTask(job.jobTasks[index]);
		jobPartsCount = job.jobPartsCount;
		globalCondition = job.globalCondition;
		otherPartsCondition = job.otherPartsCondition;
		BonusToExp = job.BonusToExp;
		BonusToMoney = job.BonusToMoney;
		LocalizationID = job.LocalizationID;
		IsMission = job.IsMission;
		MissionID = job.MissionID;
		CanDelete = job.CanDelete;
		TaskBonus = job.TaskBonus;
		JobBonus = job.JobBonus;
		TotalPayout = job.TotalPayout;
		MoneySpent = job.MoneySpent;
		MoneySpentWithDifficultyMod = job.MoneySpentWithDifficultyMod;
		XP = job.XP;
		IsCompleted = job.IsCompleted;
		IconTypeEngine = job.IconTypeEngine;
		IconTypeTiming = job.IconTypeTiming;
		IconTypeSuspension = job.IconTypeSuspension;
		IconTypeBrakes = job.IconTypeBrakes;
		IconTypeExhaust = job.IconTypeExhaust;
		IconTypeGearbox = job.IconTypeGearbox;
		IconTypeOil = job.IconTypeOil;
		IconTypeBody = job.IconTypeBody;
		IconTypeTuning = job.IconTypeTuning;
		haveToStopTimer = job.haveToStopTimer;
	}

	public Job ToGame()
	{
		var job = new Job();
		job.id = id;
		job.carLoaderID = carLoaderID;
		job.forXP = forXP;
		job.carFile = carFile;
		job.configVersion = configVersion;
		job.timeToEnd = timeToEnd;
		job.PaintType = (PaintType)PaintType;
		job.carColor = carColor.ToGame();
		job.carFactoryColor = carFactoryColor.ToGame();
		job.carFactoryPaintType = (PaintType)carFactoryPaintType;
		job.Mileage = Mileage;
		job.oilLevel = oilLevel;
		job.jobType = (bool[])jobType.Clone();
		job.jobTasks = jobTasks.Select(task => task.ToGame()).ToArray();
		job.jobPartsCount = jobPartsCount;
		job.globalCondition = globalCondition;
		job.otherPartsCondition = otherPartsCondition;
		job.BonusToExp = BonusToExp;
		job.BonusToMoney = BonusToMoney;
		job.LocalizationID = LocalizationID;
		job.IsMission = IsMission;
		job.MissionID = MissionID;
		job.CanDelete = CanDelete;
		job.TaskBonus = TaskBonus;
		job.JobBonus = JobBonus;
		job.TotalPayout = TotalPayout;
		job.MoneySpent = MoneySpent;
		job.MoneySpentWithDifficultyMod = MoneySpentWithDifficultyMod;
		job.XP = XP;
		job.IsCompleted = IsCompleted;
		job.IconTypeEngine = IconTypeEngine;
		job.IconTypeTiming = IconTypeTiming;
		job.IconTypeSuspension = IconTypeSuspension;
		job.IconTypeBrakes = IconTypeBrakes;
		job.IconTypeExhaust = IconTypeExhaust;
		job.IconTypeGearbox = IconTypeGearbox;
		job.IconTypeOil = IconTypeOil;
		job.IconTypeBody = IconTypeBody;
		job.IconTypeTuning = IconTypeTuning;
		job.haveToStopTimer = haveToStopTimer;
		return job;
	}
}