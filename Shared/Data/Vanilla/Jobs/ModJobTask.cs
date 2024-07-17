using System;
using System.Collections.Generic;
using Il2Cpp;
using MelonLoader;

namespace CMS21Together.Shared.Data.Vanilla.Jobs;

[Serializable]
public class ModJobTask
{
    public string type;
    public string subtype;
    public int IncreaseTuneValue;
    public int partsCount;
    public string desc = "";
    public bool easyMode;
    public int moneySpent;
    public List<ModJobPart> Parts;
    public bool Done;

    public ModJobTask(JobTask task)
    {
        this.type = task.type;
        this.subtype = task.subtype;
        this.IncreaseTuneValue = task.IncreaseTuneValue;
        this.partsCount = task.partsCount;
        this.desc = task.desc;
        this.easyMode = task.easyMode;
        this.moneySpent = task.moneySpent;
        this.Parts = new List<ModJobPart>();
        if (task.Parts != null)
        {
            foreach (JobPart part in task.Parts)
            {
                this.Parts.Add(new ModJobPart(part));
            }
        }
        this.Done = task.Done;
    }

    public JobTask ToGame()
    {
        JobTask task = new JobTask();
        task.type = this.type;
        task.subtype = this.subtype;
        task.IncreaseTuneValue = this.IncreaseTuneValue;
        task.partsCount = this.partsCount;
        task.desc = this.desc;
        task.easyMode = this.easyMode;
        task.moneySpent = this.moneySpent;
        task.Parts = new Il2CppSystem.Collections.Generic.List<JobPart>();
        foreach (ModJobPart part in this.Parts)
        {
            task.Parts.Add(part.ToGame());
        }
        task.Done = this.Done;
        return task;
    }
}