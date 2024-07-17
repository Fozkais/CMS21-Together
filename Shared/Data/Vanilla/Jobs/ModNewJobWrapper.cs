using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data.Vanilla.Jobs
{
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
            this.id = jobsItem.id;
            this.carLoaderID = jobsItem.carLoaderID;
            this.forXP = jobsItem.forXP;
            this.carFile = jobsItem.carFile;
            this.configVersion = jobsItem.configVersion;
            this.carColor = jobsItem.carColor;
            this.PaintType = (ModPaintType)jobsItem.PaintType;
            this.timeToEnd = jobsItem.timeToEnd;
            this.jobType = jobsItem.jobType;

            // Copy job tasks
            if (jobsItem.jobTasks != null)
            {
                this.jobTasks = new ModNewJobTaskWrapper[jobsItem.jobTasks.Length];
                for (int i = 0; i < jobsItem.jobTasks.Length; i++)
                {
                    this.jobTasks[i] = new ModNewJobTaskWrapper(jobsItem.jobTasks[i]);
                }
            }

            this.jobPartsCount = jobsItem.jobPartsCount;
            this.Mileage = jobsItem.Mileage;
            this.globalCondition = jobsItem.globalCondition;
            this.otherPartsCondition = jobsItem.otherPartsCondition;
            this.BonusToExp = jobsItem.BonusToExp;
            this.BonusToMoney = jobsItem.BonusToMoney;
            this.LocalizationID = jobsItem.LocalizationID;
            this.IsMission = jobsItem.IsMission;
            this.MissionID = jobsItem.MissionID;
            this.IconTypeEngine = jobsItem.IconTypeEngine;
            this.IconTypeTiming = jobsItem.IconTypeTiming;
            this.IconTypeSuspension = jobsItem.IconTypeSuspension;
            this.IconTypeBrakes = jobsItem.IconTypeBrakes;
            this.IconTypeExhaust = jobsItem.IconTypeExhaust;
            this.IconTypeGearbox = jobsItem.IconTypeGearbox;
            this.IconTypeOil = jobsItem.IconTypeOil;
            this.IconTypeBody = jobsItem.IconTypeBody;
            this.IconTypeTuning = jobsItem.IconTypeTuning;
        }


        public NewJobWrapper ToGame()
        {
            NewJobWrapper newJobWrapper = new NewJobWrapper();

            newJobWrapper.id = this.id;
            newJobWrapper.carLoaderID = this.carLoaderID;
            newJobWrapper.forXP = this.forXP;
            newJobWrapper.carFile = this.carFile;
            newJobWrapper.configVersion = this.configVersion;
            newJobWrapper.carColor = this.carColor;
            newJobWrapper.PaintType = (PaintType)this.PaintType;
            newJobWrapper.timeToEnd = this.timeToEnd;
            newJobWrapper.jobType = this.jobType;

            // Copy job tasks
            if (this.jobTasks != null)
            {
                newJobWrapper.jobTasks = new NewJobTaskWrapper[this.jobTasks.Length];
                for (int i = 0; i < this.jobTasks.Length; i++)
                {
                    if (this.jobTasks[i] != null)
                    {
                        newJobWrapper.jobTasks[i] = this.jobTasks[i].ToGame();
                    }
                }
            }

            newJobWrapper.jobPartsCount = this.jobPartsCount;
            newJobWrapper.Mileage = this.Mileage;
            newJobWrapper.globalCondition = this.globalCondition;
            newJobWrapper.otherPartsCondition = this.otherPartsCondition;
            newJobWrapper.BonusToExp = this.BonusToExp;
            newJobWrapper.BonusToMoney = this.BonusToMoney;
            newJobWrapper.LocalizationID = this.LocalizationID;
            newJobWrapper.IsMission = this.IsMission;
            newJobWrapper.MissionID = this.MissionID;
            newJobWrapper.IconTypeEngine = this.IconTypeEngine;
            newJobWrapper.IconTypeTiming = this.IconTypeTiming;
            newJobWrapper.IconTypeSuspension = this.IconTypeSuspension;
            newJobWrapper.IconTypeBrakes = this.IconTypeBrakes;
            newJobWrapper.IconTypeExhaust = this.IconTypeExhaust;
            newJobWrapper.IconTypeGearbox = this.IconTypeGearbox;
            newJobWrapper.IconTypeOil = this.IconTypeOil;
            newJobWrapper.IconTypeBody = this.IconTypeBody;
            newJobWrapper.IconTypeTuning = this.IconTypeTuning;

            return newJobWrapper;
        }

    }
}