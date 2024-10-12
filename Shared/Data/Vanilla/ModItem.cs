using System;
using CMS21Together.Shared.Data.Vanilla.Cars;
using Il2Cpp;
using MelonLoader;

namespace CMS21Together.Shared.Data.Vanilla
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
                this.Color = new ModColor(item.Color.GetColor());
                this.Condition = item.Condition;
                this.Dent = item.Dent;
                //this.GearboxData = item.GearboxData; // TODO: Handle class
                this.IsExamined = item.IsExamined;
                this.IsPainted = item.IsPainted;
                this.IsTinted = item.IsTinted;
                this.Livery = item.Livery;
                this.LiveryStrength = item.LiveryStrength;
                //this.LPData = item.LPData; // TODO: Handle class
               // this.MountObjectData = new ModMountObjectData(item.MountObjectData);
                this.NormalID = item.NormalID;
                this.OutsideRustEnabled = item.OutsideRustEnabled;
                this.PaintData = new ModPaintData(item.PaintData);
                this.PaintType = item.PaintType;
                this.Quality = item.Quality;
                this.RepairAmount = item.RepairAmount;
                this.TintColor = new ModColor(item.TintColor.GetColor());
               // this.TuningData = new ModTuningData(item.tuningData);
                this.WashFactor = item.WashFactor;
                this.WheelData = new ModWheelData(item.WheelData);
                this.ID = item.ID;
                this.UID = item.UID;
            }
            else
            {
                MelonLogger.Msg("Error: Item is null in ModItem constructor.");
            }
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
            //original.MountObjectData = item.MountObjectData.ToGame(); 
            original.NormalID = item.NormalID;
            original.OutsideRustEnabled = item.OutsideRustEnabled;
            original.PaintData = new ModPaintData().ToGame(item.PaintData);
            original.PaintType = item.PaintType;
            original.Quality = item.Quality;
            original.RepairAmount = item.RepairAmount;
            original.TintColor = new CustomColor( ModColor.ToColor(item.Color));
            //original.tuningData = item.TuningData.ToGame();
            original.WashFactor = item.WashFactor;
            original.WheelData = new ModWheelData().ToGame(item.WheelData);
            original.ID = item.ID;
            original.UID = item.UID;

            return original;
        }
        public Item ToGame()
        {
            Item original = new Item();
    
            if (this.Color != null)
            {
                original.Color = new CustomColor(ModColor.ToColor(this.Color));
            }
    
            original.Condition = this.Condition;
            original.Dent = this.Dent;
            original.IsExamined = this.IsExamined;
            original.IsPainted = this.IsPainted;
            original.IsTinted = this.IsTinted;
            original.Livery = this.Livery;
            original.LiveryStrength = this.LiveryStrength;
            original.NormalID = this.NormalID;
            original.OutsideRustEnabled = this.OutsideRustEnabled;


            original.PaintData = new ModPaintData().ToGame(this.PaintData);
    
            original.PaintType = this.PaintType;
            original.Quality = this.Quality;
            original.RepairAmount = this.RepairAmount;
    
            if (this.Color != null)
            {
                original.TintColor = new CustomColor(ModColor.ToColor(this.Color));
            }
    
            original.WashFactor = this.WashFactor;

            if (this.WheelData != null)
            {
                original.WheelData = new ModWheelData().ToGame(this.WheelData);
            }
    
            original.ID = this.ID;
            original.UID = this.UID;

            return original;
        }

    }
}