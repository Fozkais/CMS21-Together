using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data
{
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
            Color = new ModColor(data.Color.element[0],data.Color.element[1],data.Color.element[2],data.Color.element[3]);
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
            TintColor = new ModColor(data.TintColor.element[0], data.TintColor.element[1], data.TintColor.element[2], data.TintColor.element[3]);
            TunedID = data.TunedID;
            Unmounted = data.Unmounted;
            WashFactor = data.WashFactor;
        }
        
        public BodyPartData ToGame()
        {
            BodyPartData data = new BodyPartData();

            data.Color = new FloatArrayWrapper()
            {
                element = new float[] { Color.r, Color.g, Color.b, Color.a }
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
            data.TintColor = new FloatArrayWrapper()
            {
                element = new float[] { TintColor.r, TintColor.g, TintColor.b, TintColor.a }
            };
            data.TunedID = TunedID;
            data.Unmounted = Unmounted;
            data.WashFactor = WashFactor;

            return data;
        }
    }
}