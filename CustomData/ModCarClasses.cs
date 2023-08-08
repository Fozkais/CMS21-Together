using System;
using System.Collections.Generic;
using Il2Cpp;
using UnityEngine;

namespace CMS21MP.CustomData
{

    public enum partType
    {
        engine,
        suspension,
        body,
        other
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
}