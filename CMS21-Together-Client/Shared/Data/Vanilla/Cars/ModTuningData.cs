using System;
using CMS.Containers;

namespace CMS21Together.Shared.Data.Vanilla.Cars;

[Serializable]
public class ModTuningData
{
	public bool IsTuned;
	public short[] Values;
	public float TuningValue;

	public ModTuningData(TuningData data)
	{
		if (data != null)
		{
			IsTuned = data.IsTuned;
			Values = data.Values;
			TuningValue = data.TuningValue;
		}
	}

	public TuningData ToGame()
	{
		var data = new TuningData();
		data.IsTuned = IsTuned;
		data.Values = Values;
		data.TuningValue = TuningValue;
		return data;
	}
}