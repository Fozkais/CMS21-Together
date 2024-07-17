using System;
using Il2Cpp;
using MelonLoader;

namespace CMS21Together.Shared.Data.Vanilla.Jobs;

[Serializable]
public class ModJobPart
{
    public string ID;
    public string Name;
    public bool Done;
    public bool Found;

    public ModJobPart(JobPart part)
    {
        ID = part.ID;
        Name = part.Name;
        Done = part.Done;
        Found = part.Found;
    }

    public JobPart ToGame()
    {
        JobPart part = new JobPart();
        part.ID = this.ID;
        part.Name = this.Name;
        part.Done = this.Done;
        part.Found = this.Found;
        return part;
    }
}