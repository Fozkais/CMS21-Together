using System;
using System.Collections.Generic;

namespace CMS21Together.Shared.Data.Vanilla.Jobs;

[Serializable]
public class ModNewJobTaskWrapper
{
	public string type;
	public string subtype;
	public int IncreaseTuneValue;
	public int partsCount;
	public string desc = "";
	public bool easyMode;
	public int moneySpent;
	public List<ModNewJobPart> Parts;
	public bool _done;

	public ModNewJobTaskWrapper(NewJobTaskWrapper data)
	{
		type = data.type;
		subtype = data.subtype;
		IncreaseTuneValue = data.IncreaseTuneValue;
		partsCount = data.partsCount;
		desc = data.desc;
		easyMode = data.easyMode;
		moneySpent = data.moneySpent;
		Parts = new List<ModNewJobPart>();

		// Copy parts
		if (Parts != null)
			foreach (var part in data.Parts)
				Parts.Add(new ModNewJobPart(part));

		_done = data._done;
	}

	public NewJobTaskWrapper ToGame()
	{
		var jobTaskWrapper = new NewJobTaskWrapper();

		jobTaskWrapper.type = type;
		jobTaskWrapper.subtype = subtype;
		jobTaskWrapper.IncreaseTuneValue = IncreaseTuneValue;
		jobTaskWrapper.partsCount = partsCount;
		jobTaskWrapper.desc = desc;
		jobTaskWrapper.easyMode = easyMode;
		jobTaskWrapper.moneySpent = moneySpent;
		jobTaskWrapper.Parts = new Il2CppSystem.Collections.Generic.List<NewJobPart>();

		// Copy parts
		if (Parts != null)
			foreach (var part in Parts)
				jobTaskWrapper.Parts.Add(part.ToGame());

		jobTaskWrapper._done = _done;

		return jobTaskWrapper;
	}
}