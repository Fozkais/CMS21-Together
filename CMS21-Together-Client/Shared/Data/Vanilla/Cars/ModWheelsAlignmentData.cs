using System;

namespace CMS21Together.Shared.Data.Vanilla.Cars;

[Serializable]
public struct ModWheelsAlignmentData
{
	public float FL;
	public float FR;
	public float RL;
	public float RR;

	public ModWheelsAlignmentData(WheelsAlignmentData data)
	{
		FL = data.FL;
		FR = data.FR;
		RL = data.RL;
		RR = data.RR;
	}

	public WheelsAlignmentData ToGame(ModWheelsAlignmentData _data)
	{
		var data = new WheelsAlignmentData();
		data.FL = _data.FL;
		data.FR = _data.FR;
		data.RL = _data.RL;
		data.RR = _data.RR;
		return data;
	}
}