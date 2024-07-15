using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data.Vanilla
{
    [Serializable]
    public class ModPartData
    {
        public ModColor Color;
        public float Condition;
        public float Dust;
        public bool Examined;
        public bool IsPainted;
        public ModMountObjectData MountObjectData;
        public ModPaintData PaintData;
        public ModPaintType PaintType;
        public string Path;
        public int Quality;
        public string TunedID;
        public ModTuningData TuningData;
        public bool Unmounted;

        public ModPartData(PartData data)
        {
            Color = new ModColor(data.Color.element[0], data.Color.element[1], data.Color.element[2], data.Color.element[3]);
            Condition = data.Condition;
            Dust = data.Dust;
            Examined = data.Examined;
            IsPainted = data.IsPainted;
            MountObjectData = new ModMountObjectData(data.MountObjectData);
            PaintData = new ModPaintData(data.PaintData);
            PaintType = (ModPaintType)data.PaintType;
            Path = data.Path;
            Quality = data.Quality;
            TunedID = data.TunedID;
            TuningData = new ModTuningData(data.TuningData);
            Unmounted = data.Unmounted;
        }
        
        public PartData ToGame()
        {
            PartData data = new PartData();

            data.Color = new FloatArrayWrapper()
            {
                element = new float[] { Color.r, Color.g, Color.b, Color.a }
            };
            data.Condition = Condition;
            data.Dust = Dust;
            data.Examined = Examined;
            data.IsPainted = IsPainted;
            data.MountObjectData = MountObjectData.ToGame();
            data.PaintData = PaintData.ToGame(PaintData);
            data.PaintType = (PaintType)PaintType;
            data.Path = Path;
            data.Quality = Quality;
            data.TunedID = TunedID;
            data.Unmounted = Unmounted;

            return data;
        }
    }
}