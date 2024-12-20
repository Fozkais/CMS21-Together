using System;
using System.Collections.Generic;

namespace CMS21Together.Shared.Data.Vanilla.Jobs;

[Serializable]
public class ModNewJobsData
{
	public List<ModNewJobWrapper> jobs = new();
	public List<ModNewJobWrapper> selectedJobs = new();
	public float nextOrderTime;
	public float orderTimer;
	public int LastUId;
	public bool CurrentMissionDone = true;

	public ModNewJobsData(NewJobsData data)
	{
		jobs = new List<ModNewJobWrapper>();
		for (var i = 0; i < data.jobs.Count; i++) jobs.Add(new ModNewJobWrapper(data.jobs._items[i]));
		selectedJobs = new List<ModNewJobWrapper>();
		for (var i = 0; i < data.selectedJobs.Count; i++) selectedJobs.Add(new ModNewJobWrapper(data.jobs._items[i]));

		nextOrderTime = data.nextOrderTime;
		orderTimer = data.orderTimer;
		LastUId = data.LastUId;
		CurrentMissionDone = data.CurrentMissionDone;
	}

	public NewJobsData ToGame()
	{
		var a = new NewJobsData();

		if (jobs != null)
		{
			a.jobs = new Il2CppSystem.Collections.Generic.List<NewJobWrapper>();
			for (var i = 0; i < jobs.Count; i++)
				if (jobs[i] != null)
					a.jobs.Add(jobs[i].ToGame());
		}

		if (selectedJobs != null)
		{
			a.selectedJobs = new Il2CppSystem.Collections.Generic.List<NewJobWrapper>();
			for (var i = 0; i < selectedJobs.Count; i++)
				if (selectedJobs[i] != null)
					a.selectedJobs.Add(selectedJobs[i].ToGame());
		}

		a.nextOrderTime = nextOrderTime;
		a.orderTimer = orderTimer;
		a.LastUId = LastUId;
		a.CurrentMissionDone = CurrentMissionDone;

		return a;
	}
}