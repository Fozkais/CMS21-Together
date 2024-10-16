using System;

namespace CMS21Together.Shared.Data.Vanilla.Jobs;

[Serializable]
public class ModNewJobPart
{
	public string ID;
	public bool Done;
	public bool Found;

	public ModNewJobPart(NewJobPart part)
	{
		ID = part.ID;
		Done = part.Done;
		Found = part.Found;
	}

	public NewJobPart ToGame()
	{
		var a = new NewJobPart();
		a.ID = ID;
		a.Done = Done;
		a.Found = Found;
		return a;
	}
}