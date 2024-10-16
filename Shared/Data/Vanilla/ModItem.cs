using System;
using CMS21Together.Shared.Data.Vanilla.Cars;
using MelonLoader;

namespace CMS21Together.Shared.Data.Vanilla;

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

	//public ModMountObjectData MountObjectData;
	public string NormalID;
	public bool OutsideRustEnabled;
	public ModPaintData PaintData;
	public PaintType PaintType;
	public int Quality;
	public int RepairAmount;
	public ModColor TintColor;
	public ModTuningData TuningData;
	public float WashFactor;
	public ModWheelData WheelData;
	public string ID;
	public long UID;

	public ModItem()
	{
	}

	public ModItem(Item item)
	{
		if (item != null)
		{
			Color = new ModColor(item.Color.GetColor());
			Condition = item.Condition;
			Dent = item.Dent;
			//this.GearboxData = item.GearboxData; // TODO: Handle class
			IsExamined = item.IsExamined;
			IsPainted = item.IsPainted;
			IsTinted = item.IsTinted;
			Livery = item.Livery;
			LiveryStrength = item.LiveryStrength;
			//this.LPData = item.LPData; // TODO: Handle class
			// this.MountObjectData = new ModMountObjectData(item.MountObjectData);
			NormalID = item.NormalID;
			OutsideRustEnabled = item.OutsideRustEnabled;
			PaintData = new ModPaintData(item.PaintData);
			PaintType = item.PaintType;
			Quality = item.Quality;
			RepairAmount = item.RepairAmount;
			TintColor = new ModColor(item.TintColor.GetColor());
			// this.TuningData = new ModTuningData(item.tuningData);
			WashFactor = item.WashFactor;
			WheelData = new ModWheelData(item.WheelData);
			ID = item.ID;
			UID = item.UID;
		}
		else
		{
			MelonLogger.Msg("Error: Item is null in ModItem constructor.");
		}
	}

	public Item ToGame(ModItem item)
	{
		var original = new Item();

		original.Color = new CustomColor(ModColor.ToColor(item.Color));
		original.Condition = item.Condition;
		original.Dent = item.Dent;
		//this.GearboxData = item.GearboxData; TODO: Handle class
		original.IsExamined = item.IsExamined;
		original.IsPainted = item.IsPainted;
		original.IsTinted = item.IsTinted;
		original.Livery = item.Livery;
		original.LiveryStrength = item.LiveryStrength;
		//this.LPData = item.LPData; TODO: Handle class
		//original.MountObjectData = item.MountObjectData.ToGame(); 
		original.NormalID = item.NormalID;
		original.OutsideRustEnabled = item.OutsideRustEnabled;
		original.PaintData = new ModPaintData().ToGame(item.PaintData);
		original.PaintType = item.PaintType;
		original.Quality = item.Quality;
		original.RepairAmount = item.RepairAmount;
		original.TintColor = new CustomColor(ModColor.ToColor(item.Color));
		//original.tuningData = item.TuningData.ToGame();
		original.WashFactor = item.WashFactor;
		original.WheelData = new ModWheelData().ToGame(item.WheelData);
		original.ID = item.ID;
		original.UID = item.UID;

		return original;
	}

	public Item ToGame()
	{
		var original = new Item();

		if (Color != null) original.Color = new CustomColor(ModColor.ToColor(Color));

		original.Condition = Condition;
		original.Dent = Dent;
		original.IsExamined = IsExamined;
		original.IsPainted = IsPainted;
		original.IsTinted = IsTinted;
		original.Livery = Livery;
		original.LiveryStrength = LiveryStrength;
		original.NormalID = NormalID;
		original.OutsideRustEnabled = OutsideRustEnabled;


		original.PaintData = new ModPaintData().ToGame(PaintData);

		original.PaintType = PaintType;
		original.Quality = Quality;
		original.RepairAmount = RepairAmount;

		if (Color != null) original.TintColor = new CustomColor(ModColor.ToColor(Color));

		original.WashFactor = WashFactor;

		if (WheelData != null) original.WheelData = new ModWheelData().ToGame(WheelData);

		original.ID = ID;
		original.UID = UID;

		return original;
	}
}