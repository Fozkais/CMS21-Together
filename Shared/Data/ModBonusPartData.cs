using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data
{
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