using System;

namespace CMS21_Together_Core.Data.Vanilla.Cars;

[Serializable]
public class ModNewCarLifterData
{
	public int lifterData;

	public ModNewCarLifterData(NewCarLifterData data)
	{
		lifterData = data.lifterData;
	}

	public NewCarLifterData ToGame()
	{
		var a = new NewCarLifterData();
		a.lifterData = lifterData;
		return a;
	}
}