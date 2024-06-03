using System;
using System.Collections.Generic;
using Il2Cpp;
using MelonLoader;

namespace CMS21Together.Shared.Data
{
    [Serializable]
    public class ModCarPart
    {
        public int carPartID;
        
        public string name = string.Empty;
        public bool switched;
        public bool inprogress;
        public float condition;
        public bool unmounted;
        public string tunedID = string.Empty;
        public bool isTinted;
        public ModColor TintColor;
        public ModColor colors;
        public int paintType;
        public ModPaintData paintData;
        public float conditionStructure;
        public float conditionPaint;
        public string livery;
        public float liveryStrength;
        public bool outsaidRustEnabled;
        public float dent;
        public string additionalString;
        public List<string> mountUnmountWith = new List<string>();
        public int quality;
        public float Dust;
        public float washFactor;
        
        public ModCarPart(CarPart _part, int _carPartID)
        {
            if (_part == null) { return; }

            this.carPartID = _carPartID;
            this.name = _part.name;
            this.switched = _part.Switched;
            this.inprogress = _part.Switched;
            this.condition = _part.Condition;
            this.unmounted = _part.Unmounted;
            this.tunedID = _part.TunedID;
            this.isTinted = _part.IsTinted;
            this.TintColor = new ModColor(_part.TintColor);
            this.colors = new ModColor(_part.Color);
            this.paintType = (int)_part.PaintType;
            this.paintData = new ModPaintData(_part.PaintData);
            this.conditionStructure = _part.StructureCondition;
            this.conditionPaint = _part.ConditionPaint;
            this.livery = _part.Livery;
            this.liveryStrength = _part.LiveryStrength;
            this.outsaidRustEnabled = _part.OutsideRustEnabled;
            this.dent = _part.Dent;
            this.additionalString = _part.AdditionalString;
            this.Dust = _part.Dust;
            this.washFactor = _part.WashFactor;

            foreach (String partAttached in _part.ConnectedParts)
            {
                this.mountUnmountWith.Add(partAttached);
            }

            this.quality = _part.Quality;
        }

    }
}