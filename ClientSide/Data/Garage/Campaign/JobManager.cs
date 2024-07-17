using System.Collections;
using CMS21Together.Shared.Data.Vanilla.Jobs;
using Il2Cpp;
using Il2CppCMS.UI.Windows;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Campaign;

public static class JobManager
{
    public static IEnumerator AddJob(ModJob job)
    {
        while (!ClientData.GameReady)
            yield return new WaitForSeconds(0.25f);
        yield return new WaitForEndOfFrame();

        Job newJob = job.ToGame();
        
        GameData.Instance.orderGenerator.jobs.Add(newJob);
        GlobalData.AddJob(1);
        UIManager.Get().UpdateJobs(GameData.Instance.orderGenerator.jobs, newJob);
    }

    public static IEnumerator JobAction(int jobID, bool takeJob)
    {
        while (!ClientData.GameReady)
            yield return new WaitForSeconds(0.25f);
        yield return new WaitForEndOfFrame();
        
        bool exist = false;
        Job job = new Job();
        foreach (Job _job in GameData.Instance.orderGenerator.jobs)
        {
            if (_job.id == jobID)
            {
                exist = true;
                job = _job;
                break;
            }
        }
        if (!exist) yield break;
        
        if (takeJob)
        {
            if (job.IsMission)
                MainMod.StartCoroutine(GameData.Instance.orderGenerator.TakeMission(job.id, false));
            else
                MainMod.StartCoroutine(GameData.Instance.orderGenerator.TakeJob(job.id, false));
        }
        else
            GameData.Instance.orderGenerator.CancelJob(job.id);
    }
    
    public static void GenerateJob(bool reset)
    {
        if(reset) ClearJobs();
        GameData.Instance.orderGenerator.GenerateNewJob();
    }
    
    public static void GenerateMission(int id, bool reset)
    {
        if(reset) ClearJobs();
        GameData.Instance.orderGenerator.GenerateMission(id);
    }

    public static void ClearJobs()
    {
        GameData.Instance.orderGenerator.jobs.Clear();
        GlobalData.Jobs = 0;
        GlobalData.PrevJobsAmount = 0;
        GlobalData.AddJobsAmount = 0;
        
        UIManager.Get().UpdateJobs( GameData.Instance.orderGenerator.jobs, null);
    }
}
