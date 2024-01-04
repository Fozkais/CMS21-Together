using System;
using System.Collections.Generic;
using Il2Cpp;
using UnityEngine;

namespace CMS21Together.BothSide
{

    public enum partType
    {
        engine,
        suspension,
        body,
        other
    }

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
    
    [Serializable]
    public class ModPartScript
    {
        public string id;
        public string tunedID;
        public bool isExamined;
        public bool isPainted;
        public ModPaintData paintData;
        public int paintType;
        public ModColor color;
        public int quality;
        public float condition;
        public float dust;
        public List<String> bolts;
        public bool unmounted;

        public int partID;
        public int partIdNumber;
        public partType type;

        public Guid GUID;

        public ModPartScript(PartScript data, int _partID, int _partIdNumber, partType _type)
        {
            this.id = data.id;
            this.tunedID = data.tunedID;
            this.isExamined = data.IsExamined;
            this.isPainted = data.IsPainted;
            this.paintData = new ModPaintData(data.CurrentPaintData);
            this.paintType = (int)data.CurrentPaintType;
            this.color = new ModColor(data.currentColor);
            this.quality = data.Quality;
            this.condition = data.Condition;
            this.dust = data.Dust;
            this.unmounted = data.IsUnmounted;
            
            this.partID = _partID;
            this.partIdNumber = _partIdNumber;
            this.type = _type;

            this.GUID = new Guid();
        }

        public PartScript ToGame()
        {
            PartScript data = new PartScript();
            
            data.id = this.id;
            data.tunedID = this.tunedID;
            data.IsExamined = this.isExamined;
            data.IsPainted = this.isPainted;
            data.CurrentPaintData = this.paintData.ToGame(this.paintData);
            data.CurrentPaintType = (PaintType)this.paintType;
            data.currentColor = ModColor.ToColor(this.color);
            data.Quality = this.quality;
            data.Condition = this.condition;
            data.Dust = this.dust;
            data.IsUnmounted = this.unmounted;

            return data;
        }
    }
    
    [Serializable]
    public class ModColor
    {
        public float r;
        public float g;
        public float b;
        public float a;
        
        public ModColor(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
        
        public ModColor(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }
        
        public static Color ToColor(ModColor color)
        {
            return new Color(color.r, color.g, color.b, color.a);
        }
    }

    [Serializable]
    public class ModPaintData
    {
        public float metal;
        public float roughness;
        public float clearCoat;
        public float normalStrenght;
        public float fresnel;
        public float p_metal;
        public float p_roughness;
        public float p_clearCoat;
        public float p_normalStrenght;
        public float p_fresnel;

        public ModPaintData() { }

        public ModPaintData(PaintData data)
        {
            this.metal = data.metal;
            this.roughness = data.roughness;
            this.clearCoat = data.clearCoat;
            this.normalStrenght = data.normalStrength;
            this.fresnel = data.fresnel;

            this.p_metal = data.Metal;
            this.p_roughness = data.Roughness;
            this.p_clearCoat = data.ClearCoat;
            this.p_normalStrenght = data.NormalStrength;
            this.p_fresnel = data.Fresnel;
        }
        
        public PaintData ToGame(ModPaintData data)
        {
            PaintData paintData = new PaintData();
            paintData.metal = data.metal;
            paintData.roughness = data.roughness;
            paintData.clearCoat = data.clearCoat;
            paintData.normalStrength = data.normalStrenght;
            paintData.fresnel = data.fresnel;

            paintData.Metal = data.p_metal;
            paintData.Roughness = data.p_roughness;
            paintData.ClearCoat = data.p_clearCoat;
            paintData.NormalStrength = data.p_normalStrenght;
            paintData.Fresnel = data.p_fresnel;
            return paintData;
        }
    }

    [Serializable]
    public class ModTuningData
    {
        // TODO: Handle TuningData Class
    }
    
    [Serializable]
    public class ModWheelData
    {
        public int ET;
        public bool IsBalanced;
        public int Profile;
        public int Size;
        public int Width;

        public ModWheelData(WheelData itemWheelData)
        {
            this.ET = itemWheelData.ET;
            this.IsBalanced = itemWheelData.IsBalanced;
            this.Profile = itemWheelData.Profile;
            this.Size = itemWheelData.Size;
            this.Width = itemWheelData.Width;
        }

        public ModWheelData()
        {
        }

        public WheelData ToGame(ModWheelData itemWheelData)
        {
            WheelData data = new WheelData();
            data.ET = itemWheelData.ET;
            data.IsBalanced = itemWheelData.IsBalanced;
            data.Profile = itemWheelData.Profile;
            data.Size = itemWheelData.Size;
            data.Width = itemWheelData.Width;
            return data;
        }
    }
    
    [Serializable]
    public class ModLPData
    {
        // TODO: Handle LPData Class
    }

    [Serializable]
    public class ModMountObject
    {
        // TODO: Handle MountObject Class
    }

    [Serializable]
    public class ModGearboxData
    {
        // TODO: Handle ModGearbox Class
    }
}