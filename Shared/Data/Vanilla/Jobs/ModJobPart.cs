using System;

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
		var part = new JobPart();
		part.ID = ID;
		part.Name = Name;
		part.Done = Done;
		part.Found = Found;
		return part;
	}
}