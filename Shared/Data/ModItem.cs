using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data
{
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
        public ModMountObject MountObjectData;
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
            this.Color = new ModColor(item.Color.GetColor());
            this.Condition = item.Condition;
            this.Dent = item.Dent;
            //this.GearboxData = item.GearboxData; TODO: Handle class
            this.IsExamined = item.IsExamined;
            this.IsPainted = item.IsPainted;
            this.IsTinted = item.IsTinted;
            this.Livery = item.Livery;
            this.LiveryStrength = item.LiveryStrength;
            //this.LPData = item.LPData; TODO: Handle class
            //this.MountObjectData = item.MountObjectData; TODO: Handle class
            this.NormalID = item.NormalID;
            this.OutsideRustEnabled = item.OutsideRustEnabled;
            this.PaintData = new ModPaintData(item.PaintData);
            this.PaintType = item.PaintType;
            this.Quality = item.Quality;
            this.RepairAmount = item.RepairAmount;
            this.TintColor = new ModColor(item.TintColor.GetColor());
            //this.TuningData = item.TuningData; TODO: Handle class
            this.WashFactor = item.WashFactor;
            this.WheelData = new ModWheelData(item.WheelData);
            this.ID = item.ID;
            this.UID = item.UID;
        }

        public Item ToGame(ModItem item)
        {
            Item original = new Item();
            
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
            //original.MountObjectData = item.MountObjectData; TODO: Handle class
            original.NormalID = item.NormalID;
            original.OutsideRustEnabled = item.OutsideRustEnabled;
            original.PaintData = new ModPaintData().ToGame(item.PaintData);
            original.PaintType = item.PaintType;
            original.Quality = item.Quality;
            original.RepairAmount = item.RepairAmount;
            original.TintColor = new CustomColor( ModColor.ToColor(item.Color));
            //this.TuningData = item.TuningData; TODO: Handle class
            original.WashFactor = item.WashFactor;
            original.WheelData = new ModWheelData().ToGame(item.WheelData);
            original.ID = item.ID;
            original.UID = item.UID;

            return original;
        }
    }
}