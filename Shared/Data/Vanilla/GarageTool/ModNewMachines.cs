using System;

namespace CMS21Together.Shared.Data.Vanilla.GarageTool;

[Serializable]
public class ModNewMachines
{
	public ModGroupItem GroupOnWheelBalancer;

	public bool WheelWasBalanced;

	public ModGroupItem GroupOnTireChanger;

	public bool GroupOnTireChangerIsMounting;

	public ModGroupItem GroupOnSpringClamp;

	public bool GroupOnSpringClampIsMounting;

	public ModGroupItem GroupOnEngineStand;

	public float EngineStandAngle;

	public ModItem ItemOnBatteryCharger;

	public ModItem ItemOnBrakeLathe;

	public ModNewMachines(NewMachines data)
	{
		GroupOnWheelBalancer = new ModGroupItem(data.GroupOnWheelBalancer);
		WheelWasBalanced = data.WheelWasBalanced;
		GroupOnTireChanger = new ModGroupItem(data.GroupOnTireChanger);
		GroupOnTireChangerIsMounting = data.GroupOnTireChangerIsMounting;
		GroupOnSpringClamp = new ModGroupItem(data.GroupOnSpringClamp);
		GroupOnSpringClampIsMounting = data.GroupOnSpringClampIsMounting;
		GroupOnEngineStand = new ModGroupItem(data.GroupOnEngineStand);
		EngineStandAngle = data.EngineStandAngle;
		ItemOnBatteryCharger = new ModItem(data.ItemOnBatteryCharger);
	}

	public NewMachines ToGame()
	{
		var a = new NewMachines();
		a.GroupOnWheelBalancer = GroupOnWheelBalancer.ToGame();
		a.WheelWasBalanced = WheelWasBalanced;
		a.GroupOnTireChanger = GroupOnTireChanger.ToGame();
		a.GroupOnTireChangerIsMounting = GroupOnTireChangerIsMounting;
		a.GroupOnSpringClamp = GroupOnSpringClamp.ToGame();
		a.GroupOnSpringClampIsMounting = GroupOnSpringClampIsMounting;
		a.GroupOnEngineStand = GroupOnEngineStand.ToGame();
		a.EngineStandAngle = EngineStandAngle;
		a.ItemOnBatteryCharger = ItemOnBatteryCharger.ToGame();
		return a;
	}
}