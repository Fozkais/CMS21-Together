using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.Shared.Data.Vanilla.Jobs;
using Il2Cpp;
using Il2CppCMS.UI.Windows;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Campaign;

public static class JobManager
{

    public static List<ModJob> selectedJobs = new List<ModJob>();

    public static void Reset()
    {
        selectedJobs.Clear();
    }
    
    public static void UpdateSelectedJob()
    {
        if (GameData.Instance == null || GameData.Instance.orderGenerator == null) return;
        if (GameData.Instance.orderGenerator.selectedJobs == null) return;

        for (int i = 0; i < GameData.Instance.orderGenerator.selectedJobs.Count; i++)
        {
            Job job = GameData.Instance.orderGenerator.selectedJobs._items[i];
            if (job != null && selectedJobs.All(j => j != null && job.id != j.id))
            {
                ModJob newJob = new ModJob(job);
                selectedJobs.Add(newJob);
                ClientSend.SelectedJobPacket(newJob, true);
            }
        }

        for (var index = 0; index < selectedJobs.Count; index++)
        {
            var job = selectedJobs[index];
            if (GameData.Instance.orderGenerator.selectedJobs == null || 
                GameData.Instance.orderGenerator.selectedJobs._items.All(j => j != null && job.id != j.id))
            {
                ClientSend.SelectedJobPacket(job, false);
                selectedJobs.Remove(job);
            }
        }
    }
    
    public static IEnumerator SelectedJob(ModJob modjob, bool action)
    {
        while (!ClientData.GameReady)
            yield return new WaitForSeconds(0.25f);
        yield return new WaitForEndOfFrame();
        
        if (action)
        {
            if (selectedJobs.All(j => modjob.id != j.id))
            {
                selectedJobs.Add(modjob);
                GameData.Instance.orderGenerator.selectedJobs.Add(modjob.ToGame());
            }
        }
        else
        {
            if (selectedJobs.Any(j => modjob.id == j.id))
            {
                var gameJobs = GameData.Instance.orderGenerator.selectedJobs;
                selectedJobs.Remove(selectedJobs.First(j => j.id == modjob.id));
                gameJobs.Remove(gameJobs._items.First(j => j.id == modjob.id));
            }
        }
    }
    
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
