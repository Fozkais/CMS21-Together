using System;
using System.Linq;
using Il2Cpp;
using MelonLoader;

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
        this.id = job.id;
        this.carLoaderID = job.carLoaderID;
        this.forXP = job.forXP;
        this.carFile = job.carFile;
        this.configVersion = job.configVersion;
        this.timeToEnd = job.timeToEnd;
        this.PaintType = (ModPaintType)job.PaintType;
        this.carColor = new ModColor(job.carColor);
        this.carFactoryColor = new ModColor(job.carFactoryColor);
        this.carFactoryPaintType = (ModPaintType)job.carFactoryPaintType;
        this.Mileage = job.Mileage;
        this.oilLevel = job.oilLevel;
        this.jobType = job.jobType;
        this.jobTasks = new ModJobTask[job.jobTasks.Count];
        for (var index = 0; index < job.jobTasks.Count; index++)
        {
            this.jobTasks[index] = new ModJobTask(job.jobTasks[index]);
        }
        this.jobPartsCount = job.jobPartsCount;
        this.globalCondition = job.globalCondition;
        this.otherPartsCondition = job.otherPartsCondition;
        this.BonusToExp = job.BonusToExp;
        this.BonusToMoney = job.BonusToMoney;
        this.LocalizationID = job.LocalizationID;
        this.IsMission = job.IsMission;
        this.MissionID = job.MissionID;
        this.CanDelete = job.CanDelete;
        this.TaskBonus = job.TaskBonus;
        this.JobBonus = job.JobBonus;
        this.TotalPayout = job.TotalPayout;
        this.MoneySpent = job.MoneySpent;
        this.MoneySpentWithDifficultyMod = job.MoneySpentWithDifficultyMod;
        this.XP = job.XP;
        this.IsCompleted = job.IsCompleted;
        this.IconTypeEngine = job.IconTypeEngine;
        this.IconTypeTiming = job.IconTypeTiming;
        this.IconTypeSuspension = job.IconTypeSuspension;
        this.IconTypeBrakes = job.IconTypeBrakes;
        this.IconTypeExhaust = job.IconTypeExhaust;
        this.IconTypeGearbox = job.IconTypeGearbox;
        this.IconTypeOil = job.IconTypeOil;
        this.IconTypeBody = job.IconTypeBody;
        this.IconTypeTuning = job.IconTypeTuning;
        this.haveToStopTimer = job.haveToStopTimer;
    }

    public Job ToGame()
    {
        Job job = new Job();
        job.id = this.id;
        job.carLoaderID = this.carLoaderID;
        job.forXP = this.forXP;
        job.carFile = this.carFile;
        job.configVersion = this.configVersion;
        job.timeToEnd = this.timeToEnd;
        job.PaintType = (PaintType)this.PaintType;
        job.carColor = this.carColor.ToGame();
        job.carFactoryColor = this.carFactoryColor.ToGame();
        job.carFactoryPaintType = (PaintType)this.carFactoryPaintType;
        job.Mileage = this.Mileage;
        job.oilLevel = this.oilLevel;
        job.jobType = (bool[])this.jobType.Clone();
        job.jobTasks = this.jobTasks.Select(task => task.ToGame()).ToArray();
        job.jobPartsCount = this.jobPartsCount;
        job.globalCondition = this.globalCondition;
        job.otherPartsCondition = this.otherPartsCondition;
        job.BonusToExp = this.BonusToExp;
        job.BonusToMoney = this.BonusToMoney;
        job.LocalizationID = this.LocalizationID;
        job.IsMission = this.IsMission;
        job.MissionID = this.MissionID;
        job.CanDelete = this.CanDelete;
        job.TaskBonus = this.TaskBonus;
        job.JobBonus = this.JobBonus;
        job.TotalPayout = this.TotalPayout;
        job.MoneySpent = this.MoneySpent;
        job.MoneySpentWithDifficultyMod = this.MoneySpentWithDifficultyMod;
        job.XP = this.XP;
        job.IsCompleted = this.IsCompleted;
        job.IconTypeEngine = this.IconTypeEngine;
        job.IconTypeTiming = this.IconTypeTiming;
        job.IconTypeSuspension = this.IconTypeSuspension;
        job.IconTypeBrakes = this.IconTypeBrakes;
        job.IconTypeExhaust = this.IconTypeExhaust;
        job.IconTypeGearbox = this.IconTypeGearbox;
        job.IconTypeOil = this.IconTypeOil;
        job.IconTypeBody = this.IconTypeBody;
        job.IconTypeTuning = this.IconTypeTuning;
        job.haveToStopTimer = this.haveToStopTimer;
        return job;
    }
}