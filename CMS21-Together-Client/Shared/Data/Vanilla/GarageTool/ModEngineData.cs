using System;

namespace CMS21Together.Shared.Data.Vanilla.GarageTool;

[Serializable]
public struct ModEngineData
{
	public bool isElectric;
	public float idleRpm;
	public float idleRpmTorque;
	public float idleRpmCurveBias;
	public float peakRpm;
	public float peakRpmTorque;
	public float peakRpmCurveBias;
	public float maxRpm;
	public float inertia;
	public float engineFrictionTorque;
	public float engineFrictionRotational;
	public float engineFrictionViscous;
	public float limiterTriggerRpm;
	public float tuningValue;
	public bool measured;

	public ModEngineData(EngineData data)
	{
		isElectric = data.isElectric;
		idleRpm = data.idleRpm;
		idleRpmTorque = data.idleRpmTorque;
		idleRpmCurveBias = data.idleRpmCurveBias;
		peakRpm = data.peakRpm;
		peakRpmTorque = data.peakRpmTorque;
		peakRpmCurveBias = data.peakRpmCurveBias;
		maxRpm = data.maxRpm;
		inertia = data.inertia;
		engineFrictionTorque = data.engineFrictionTorque;
		engineFrictionRotational = data.engineFrictionRotational;
		engineFrictionViscous = data.engineFrictionViscous;
		limiterTriggerRpm = data.limiterTriggerRpm;
		tuningValue = data.tuningValue;
		measured = data.measured;
	}

	public EngineData ToGame()
	{
		var data = new EngineData();

		data.isElectric = isElectric;
		data.idleRpm = idleRpm;
		data.idleRpmTorque = idleRpmTorque;
		data.idleRpmCurveBias = idleRpmCurveBias;
		data.peakRpm = peakRpm;
		data.peakRpmTorque = peakRpmTorque;
		data.peakRpmCurveBias = peakRpmCurveBias;
		data.maxRpm = maxRpm;
		data.inertia = inertia;
		data.engineFrictionTorque = engineFrictionTorque;
		data.engineFrictionRotational = engineFrictionRotational;
		data.engineFrictionViscous = engineFrictionViscous;
		data.limiterTriggerRpm = limiterTriggerRpm;
		data.tuningValue = tuningValue;
		data.measured = measured;

		return data;
	}
}