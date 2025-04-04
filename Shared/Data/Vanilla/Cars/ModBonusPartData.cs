using System;
using MelonLoader;
using UnhollowerBaseLib;

namespace CMS21Together.Shared.Data.Vanilla.Cars;

[Serializable]
public struct ModBonusPartsData
{
	public string[] IDs;
	public bool[] IsPainted;
	public ModCustomColor[] Color;
	public ModPaintType[] PaintType;
	public ModPaintData[] PaintData;
	public int[] IdFromConfig;

	public ModBonusPartsData(BonusPartsData data)
	{
		if (data == null)
		{
			MelonLogger.Error("ModBonusPartsData: data is null");
			return;
		}

		IDs = data.IDs;
		IsPainted = data.IsPainted;
		
		if (data.Color != null)
		{
			Color = new ModCustomColor[data.Color.Count];
			for (var i = 0; i < data.Color.Count; i++)
			{
				if (data.Color[i] != null)
					Color[i] = new ModCustomColor(data.Color[i]);
				else
					MelonLogger.Warning($"ModBonusPartsData: Color[{i}] is null");
			}
		}
		else
		{
			MelonLogger.Warning("ModBonusPartsData: data.Color is null");
			Color = new ModCustomColor[0];
		}
		
		if (data.PaintType != null)
		{
			PaintType = new ModPaintType[data.PaintType.Count];
			for (var i = 0; i < data.PaintType.Count; i++)
			{
				PaintType[i] = (ModPaintType)data.PaintType[i];
			}
		}
		else
		{
			MelonLogger.Warning("ModBonusPartsData: data.PaintType is null");
			PaintType = [];
		}
		
		if (data.PaintData != null)
		{
			PaintData = new ModPaintData[data.PaintData.Count];
			for (var i = 0; i < data.PaintData.Count; i++)
			{
				PaintData[i] = new ModPaintData(data.PaintData[i]);
			}
		}
		else
		{
			MelonLogger.Warning("ModBonusPartsData: data.PaintData is null");
			PaintData = [];
		}
		IdFromConfig = data.IdFromConfig;
	}


	public BonusPartsData ToGame()
	{
		var data = new BonusPartsData();
		data.IDs = IDs;
		data.IsPainted = IsPainted;

		data.Color = new Il2CppReferenceArray<CustomColor>(Color.Length);
		for (var index = 0; index < Color.Length; index++)
		{
			var modColor = Color[index].ToGame();
			data.Color[index] = modColor;
		}

		data.PaintType = new Il2CppStructArray<PaintType>(PaintType.Length);
		for (var index = 0; index < PaintType.Length; index++)
		{
			var modPaintType = PaintType[index];
			data.PaintType[index] = (PaintType)modPaintType;
		}

		data.PaintData = new Il2CppStructArray<PaintData>(PaintData.Length);
		for (var index = 0; index < PaintData.Length; index++)
		{
			var modPaintData = PaintData[index].ToGame();
			data.PaintData[index] = modPaintData;
		}

		data.IdFromConfig = IdFromConfig;
		return data;
	}
}

[Serializable]
public struct ModCustomColor
{
	private float[] Color;

	public ModCustomColor(CustomColor color)
	{
		Color = color.Color;
	}

	public CustomColor ToGame()
	{
		var _color = new CustomColor();
		_color.Color = Color;
		return _color;
	}
}