using System;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;

namespace CMS21Together.Shared.Data.Vanilla;

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
        IDs = data.IDs;
        IsPainted = data.IsPainted;
        Color = new ModCustomColor[data.Color.Count];
        for (int i = 0; i < data.Color.Count; i++)
        {
            Color[i] = new ModCustomColor(data.Color[i]);
        }

        PaintType = new ModPaintType[data.PaintType.Count];
        for (int i = 0; i < data.PaintType.Count; i++)
        {
            PaintType[i] = (ModPaintType)data.PaintType[i];
        }

        PaintData = new ModPaintData[data.PaintData.Count];
        for (int i = 0; i < data.PaintData.Count; i++)
        {
            PaintData[i] = new ModPaintData(data.PaintData[i]);
        }

        IdFromConfig = data.IdFromConfig;
    }

    public BonusPartsData ToGame()
    {
        BonusPartsData data = new BonusPartsData();
        data.IDs = this.IDs;
        data.IsPainted = this.IsPainted;

        data.Color = new Il2CppReferenceArray<CustomColor>(this.Color.Length);
        for (var index = 0; index < this.Color.Length; index++)
        {
            var modColor = this.Color[index].ToGame();
            data.Color[index] = modColor;
        }

        data.PaintType = new Il2CppStructArray<PaintType>(this.PaintType.Length);
        for (var index = 0; index < this.PaintType.Length; index++)
        {
            var modPaintType = this.PaintType[index];
            data.PaintType[index] = (PaintType)modPaintType;
        }

        data.PaintData = new Il2CppStructArray<PaintData>(this.PaintData.Length);
        for (var index = 0; index < this.PaintData.Length; index++)
        {
            var modPaintData = this.PaintData[index].ToGame();
            data.PaintData[index] = modPaintData;
        }

        data.IdFromConfig = this.IdFromConfig;
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
        CustomColor _color = new CustomColor();
        _color.Color = this.Color;
        return _color;
    }
}