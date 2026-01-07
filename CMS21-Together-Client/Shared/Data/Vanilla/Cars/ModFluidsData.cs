using System;
using System.Collections.Generic;

namespace CMS21Together.Shared.Data.Vanilla.Cars;

[Serializable]
public struct ModFluidsData
{
	public ModFluidData Oil;
	public List<ModFluidData> Brake;
	public List<ModFluidData> EngineCoolant;
	public List<ModFluidData> PowerSteering;
	public List<ModFluidData> WindscreenWash;

	public ModFluidsData(FluidsData data)
	{
		if (data.Oil != null)
			Oil = new ModFluidData(data.Oil);

		if (data.Brake != null)
		{
			Brake = new List<ModFluidData>();
			foreach (var fluidData in data.Brake) Brake.Add(new ModFluidData(fluidData));
		}

		if (data.EngineCoolant != null)
		{
			EngineCoolant = new List<ModFluidData>();
			foreach (var fluidData in data.EngineCoolant) EngineCoolant.Add(new ModFluidData(fluidData));
		}

		if (data.PowerSteering != null)
		{
			PowerSteering = new List<ModFluidData>();
			foreach (var fluidData in data.PowerSteering) PowerSteering.Add(new ModFluidData(fluidData));
		}

		if (data.WindscreenWash != null)
		{
			WindscreenWash = new List<ModFluidData>();
			foreach (var fluidData in data.WindscreenWash) WindscreenWash.Add(new ModFluidData(fluidData));
		}
	}

	public FluidsData ToGame()
	{
		var _data = new FluidsData();

		_data.Oil = Oil?.ToGame();

		_data.Brake = new Il2CppSystem.Collections.Generic.List<FluidData>();
		if (Brake != null)
			foreach (var fluidData in Brake)
				if (fluidData != null)
					_data.Brake.Add(fluidData.ToGame());
		_data.EngineCoolant = new Il2CppSystem.Collections.Generic.List<FluidData>();
		if (EngineCoolant != null)
			foreach (var fluidData in EngineCoolant)
				if (fluidData != null)
					_data.EngineCoolant.Add(fluidData.ToGame());
		_data.PowerSteering = new Il2CppSystem.Collections.Generic.List<FluidData>();
		if (PowerSteering != null)
			foreach (var fluidData in PowerSteering)
				if (fluidData != null)
					_data.PowerSteering.Add(fluidData.ToGame());
		_data.WindscreenWash = new Il2CppSystem.Collections.Generic.List<FluidData>();
		if (WindscreenWash != null)
			foreach (var fluidData in WindscreenWash)
				if (fluidData != null)
					_data.WindscreenWash.Add(fluidData.ToGame());

		return _data;
	}
}