using System;
using System.Collections.Generic;
using Il2Cpp;

namespace CMS21Together.Shared.Data
{
    [Serializable]
    public class ModNewJobsData
    {
        public List<ModNewJobWrapper> jobs = new List<ModNewJobWrapper>();
        public List<ModNewJobWrapper> selectedJobs = new List<ModNewJobWrapper>();
        public float nextOrderTime;
        public float orderTimer;
        public int LastUId;
        public bool CurrentMissionDone = true;

        public ModNewJobsData(NewJobsData data)
        {
            jobs = new List<ModNewJobWrapper>();
            for (int i = 0; i < data.jobs.Count; i++)
            {
                jobs.Add(new ModNewJobWrapper(data.jobs._items[i]));
            }
            selectedJobs = new List<ModNewJobWrapper>();
            for (int i = 0; i < data.selectedJobs.Count; i++)
            {
                selectedJobs.Add(new ModNewJobWrapper(data.jobs._items[i]));
            }

            nextOrderTime = data.nextOrderTime;
            orderTimer = data.orderTimer;
            LastUId = data.LastUId;
            CurrentMissionDone = data.CurrentMissionDone;
        }

        public NewJobsData ToGame()
        {
            NewJobsData a = new NewJobsData();
    
            if (this.jobs != null)
            {
                a.jobs = new Il2CppSystem.Collections.Generic.List<NewJobWrapper>();
                for (int i = 0; i < this.jobs.Count; i++)
                {
                    if (this.jobs[i] != null)
                    {
                        a.jobs.Add(this.jobs[i].ToGame());
                    }
                }
            }

            if (this.selectedJobs != null)
            {
                a.selectedJobs = new Il2CppSystem.Collections.Generic.List<NewJobWrapper>();
                for (int i = 0; i < this.selectedJobs.Count; i++)
                {
                    if (this.selectedJobs[i] != null)
                    {
                        a.selectedJobs.Add(this.selectedJobs[i].ToGame());
                    }
                }
            }

            a.nextOrderTime = this.nextOrderTime;
            a.orderTimer = this.orderTimer;
            a.LastUId = this.LastUId;
            a.CurrentMissionDone = this.CurrentMissionDone;

            return a;
        }

    }
}