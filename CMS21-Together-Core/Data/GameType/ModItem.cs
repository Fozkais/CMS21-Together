using System;

namespace CMS21_Together_Core.Data.GameType;

[Serializable]
public class ModItem
{
	public ModColor Color;
	public float Condition;
	public float ConditionToShow;
	public float Dent;
	public ModGearboxData GearboxData;
	public bool IsExamined;
	public bool IsPainted;
	public bool IsTinted;
	public string Livery;
	public float LiveryStrength;

	public ModLPData LPData;

	public ModMountObjectData MountObjectData;
	public string NormalID;
	public bool OutsideRustEnabled;
	public ModPaintData PaintData;
	public ModPaintType PaintType;
	public int Quality;
	public int RepairAmount;
	public ModColor TintColor;
	public ModTuningData TuningData;
	public float WashFactor;
	public ModWheelData WheelData;
	public string ID;
	public long UID;

}

