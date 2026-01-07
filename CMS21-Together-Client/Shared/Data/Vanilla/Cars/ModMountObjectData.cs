using System;

namespace CMS21Together.Shared.Data.Vanilla.Cars;

[Serializable]
public class ModMountObjectData
{
	public string ParentPath;

	public float[] Condition;

	public bool[] IsStuck;

	public ModMountObjectData() {}
	
	public ModMountObjectData(MountObjectData data)
	{
		if (data == null) return;
		
		ParentPath = data.ParentPath;
		Condition = data.Condition;
		IsStuck = data.IsStuck;
	}

	public MountObjectData ToGame()
	{
		var data = new MountObjectData();
		data.Condition = Condition;
		data.IsStuck = IsStuck;
		data.ParentPath = ParentPath;

		return data;
	}
}