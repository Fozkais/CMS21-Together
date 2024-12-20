using System;

namespace CMS21Together.Shared.Data.Vanilla.Jobs;

[Serializable]
public class ModNewJobWrapper
{
	public int id;
	public int carLoaderID;
	public int forXP;
	public string carFile;
	public int configVersion;
	public float[] carColor;
	public ModPaintType PaintType;
	public float timeToEnd;
	public bool[] jobType;
	public ModNewJobTaskWrapper[] jobTasks;
	public int jobPartsCount;
	public int Mileage;
	public float globalCondition;
	public float otherPartsCondition;
	public bool BonusToExp;
	public bool BonusToMoney;
	public string LocalizationID;
	public bool IsMission;
	public int MissionID;
	public bool IconTypeEngine;
	public bool IconTypeTiming;
	public bool IconTypeSuspension;
	public bool IconTypeBrakes;
	public bool IconTypeExhaust;
	public bool IconTypeGearbox;
	public bool IconTypeOil;
	public bool IconTypeBody;
	public bool IconTypeTuning;

	public ModNewJobWrapper(NewJobWrapper jobsItem)
	{
		id = jobsItem.id;
		carLoaderID = jobsItem.carLoaderID;
		forXP = jobsItem.forXP;
		carFile = jobsItem.carFile;
		configVersion = jobsItem.configVersion;
		carColor = jobsItem.carColor;
		PaintType = (ModPaintType)jobsItem.PaintType;
		timeToEnd = jobsItem.timeToEnd;
		jobType = jobsItem.jobType;

		// Copy job tasks
		if (jobsItem.jobTasks != null)
		{
			jobTasks = new ModNewJobTaskWrapper[jobsItem.jobTasks.Length];
			for (var i = 0; i < jobsItem.jobTasks.Length; i++) jobTasks[i] = new ModNewJobTaskWrapper(jobsItem.jobTasks[i]);
		}

		jobPartsCount = jobsItem.jobPartsCount;
		Mileage = jobsItem.Mileage;
		globalCondition = jobsItem.globalCondition;
		otherPartsCondition = jobsItem.otherPartsCondition;
		BonusToExp = jobsItem.BonusToExp;
		BonusToMoney = jobsItem.BonusToMoney;
		LocalizationID = jobsItem.LocalizationID;
		IsMission = jobsItem.IsMission;
		MissionID = jobsItem.MissionID;
		IconTypeEngine = jobsItem.IconTypeEngine;
		IconTypeTiming = jobsItem.IconTypeTiming;
		IconTypeSuspension = jobsItem.IconTypeSuspension;
		IconTypeBrakes = jobsItem.IconTypeBrakes;
		IconTypeExhaust = jobsItem.IconTypeExhaust;
		IconTypeGearbox = jobsItem.IconTypeGearbox;
		IconTypeOil = jobsItem.IconTypeOil;
		IconTypeBody = jobsItem.IconTypeBody;
		IconTypeTuning = jobsItem.IconTypeTuning;
	}


	public NewJobWrapper ToGame()
	{
		var newJobWrapper = new NewJobWrapper();

		newJobWrapper.id = id;
		newJobWrapper.carLoaderID = carLoaderID;
		newJobWrapper.forXP = forXP;
		newJobWrapper.carFile = carFile;
		newJobWrapper.configVersion = configVersion;
		newJobWrapper.carColor = carColor;
		newJobWrapper.PaintType = (PaintType)PaintType;
		newJobWrapper.timeToEnd = timeToEnd;
		newJobWrapper.jobType = jobType;

		// Copy job tasks
		if (jobTasks != null)
		{
			newJobWrapper.jobTasks = new NewJobTaskWrapper[jobTasks.Length];
			for (var i = 0; i < jobTasks.Length; i++)
				if (jobTasks[i] != null)
					newJobWrapper.jobTasks[i] = jobTasks[i].ToGame();
		}

		newJobWrapper.jobPartsCount = jobPartsCount;
		newJobWrapper.Mileage = Mileage;
		newJobWrapper.globalCondition = globalCondition;
		newJobWrapper.otherPartsCondition = otherPartsCondition;
		newJobWrapper.BonusToExp = BonusToExp;
		newJobWrapper.BonusToMoney = BonusToMoney;
		newJobWrapper.LocalizationID = LocalizationID;
		newJobWrapper.IsMission = IsMission;
		newJobWrapper.MissionID = MissionID;
		newJobWrapper.IconTypeEngine = IconTypeEngine;
		newJobWrapper.IconTypeTiming = IconTypeTiming;
		newJobWrapper.IconTypeSuspension = IconTypeSuspension;
		newJobWrapper.IconTypeBrakes = IconTypeBrakes;
		newJobWrapper.IconTypeExhaust = IconTypeExhaust;
		newJobWrapper.IconTypeGearbox = IconTypeGearbox;
		newJobWrapper.IconTypeOil = IconTypeOil;
		newJobWrapper.IconTypeBody = IconTypeBody;
		newJobWrapper.IconTypeTuning = IconTypeTuning;

		return newJobWrapper;
	}
}