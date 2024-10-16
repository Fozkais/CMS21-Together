using System;

namespace CMS21Together.Shared.Data.Vanilla.Cars;

[Serializable]
public class ModBodyPartData
{
	public ModColor Color;
	public float Condition;
	public float Dent;
	public float Dust;
	public string Id;
	public bool isTinted;
	public string Livery;
	public float LiveryStrength;
	public bool OutsaidRustEnabled;
	public ModPaintData PaintData;
	public int PaintType;
	public int Quality;
	public bool Switched;
	public ModColor TintColor;
	public string TunedID;
	public bool Unmounted;
	public float WashFactor;

	public ModBodyPartData(BodyPartData data)
	{
		if (data == null) return;

		Color = data.Color != null
			? new ModColor(data.Color.element[0], data.Color.element[1], data.Color.element[2], data.Color.element[3])
			: new ModColor(0, 0, 0, 0); // Default color if data.Color is null

		Condition = data.Condition;
		Dent = data.Dent;
		Dust = data.Dust;
		Id = data.Id;
		isTinted = data.IsTinted;
		Livery = data.Livery;
		LiveryStrength = data.LiveryStrength;
		OutsaidRustEnabled = data.OutsaidRustEnabled;
		PaintData = new ModPaintData(data.PaintData);
		PaintType = data.PaintType;
		Quality = data.Quality;
		Switched = data.Switched;
		TintColor = data.TintColor != null
			? new ModColor(data.TintColor.element[0], data.TintColor.element[1], data.TintColor.element[2], data.TintColor.element[3])
			: new ModColor(0, 0, 0, 0); // Default tint color if data.TintColor is null

		TunedID = data.TunedID;
		Unmounted = data.Unmounted;
		WashFactor = data.WashFactor;
	}


	public BodyPartData ToGame()
	{
		var data = new BodyPartData();

		data.Color = new FloatArrayWrapper
		{
			element = new[] { Color.r, Color.g, Color.b, Color.a }
		};
		data.Condition = Condition;
		data.Dent = Dent;
		data.Dust = Dust;
		data.Id = Id;
		data.IsTinted = isTinted;
		data.Livery = Livery;
		data.LiveryStrength = LiveryStrength;
		data.OutsaidRustEnabled = OutsaidRustEnabled;
		data.PaintData = PaintData.ToGame(PaintData);
		data.PaintType = PaintType;
		data.Quality = Quality;
		data.Switched = Switched;
		data.TintColor = new FloatArrayWrapper
		{
			element = new[] { TintColor.r, TintColor.g, TintColor.b, TintColor.a }
		};
		data.TunedID = TunedID;
		data.Unmounted = Unmounted;
		data.WashFactor = WashFactor;

		return data;
	}
}