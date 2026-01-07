using System;
using System.Collections.Generic;

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
		type = task.type;
		subtype = task.subtype;
		IncreaseTuneValue = task.IncreaseTuneValue;
		partsCount = task.partsCount;
		desc = task.desc;
		easyMode = task.easyMode;
		moneySpent = task.moneySpent;
		Parts = new List<ModJobPart>();
		if (task.Parts != null)
			foreach (var part in task.Parts)
				Parts.Add(new ModJobPart(part));
		Done = task.Done;
	}

	public JobTask ToGame()
	{
		var task = new JobTask();
		task.type = type;
		task.subtype = subtype;
		task.IncreaseTuneValue = IncreaseTuneValue;
		task.partsCount = partsCount;
		task.desc = desc;
		task.easyMode = easyMode;
		task.moneySpent = moneySpent;
		task.Parts = new Il2CppSystem.Collections.Generic.List<JobPart>();
		foreach (var part in Parts) task.Parts.Add(part.ToGame());
		task.Done = Done;
		return task;
	}
}