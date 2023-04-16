using System;
using System.Collections.Generic;
using Il2Cpp;
using UnityEngine;

namespace CMS21MP.DataHandle
{
    [Serializable] 
    public class PartScriptData
    {
        public string id;
        public string tunedID;
        public bool isExamined;
        public bool isPainted;
        public C_PaintData paintData;
        public int paintType;
        public C_Color color;
        public int quality;
        public float condition;
        public float dust;
        public List<String> bolts;
        public bool unmounted;

        public PartScriptData(PartScript data)
        {
            this.id = data.id;
            this.tunedID = data.tunedID;
            this.isExamined = data.IsExamined;
            this.isPainted = data.IsPainted;
            this.paintData = new C_PaintData().FromGame(data.CurrentPaintData);
            this.paintType = (int)data.CurrentPaintType;
            this.color = new C_Color(data.currentColor);
            this.quality = data.Quality;
            this.condition = data.Condition;
            this.dust = data.Dust; 
            //this.bolts = data.bolts;
           this.unmounted = data.IsUnmounted;
        }
    }
    [Serializable]
    public class C_carPartsData
    {
        // public GameObject handle;
       public int carPartID;
       public int carLoaderID;
        public string name = string.Empty;
        public bool switched;
        public bool inprogress;
        public float condition;
        public bool unmounted;
        public string tunedID = string.Empty;
        public bool isTinted;
        public C_Color TintColor;
        public C_Color colors;
        public int paintType;
        public C_PaintData paintData;
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
    }

    [Serializable]
    public class PartScriptInfo
    {
        public PartScriptData _partScriptData;
        public partType _type;
        public int _carLoaderID;
        public int _partCountID;
        public Guid _UniqueID;
        public int _partItemID;

        public PartScriptInfo(partType type, PartScriptData partScriptData, int carLoaderID, Guid UniqueID, int partItemID, int partCountID)
        {
            this._type = type;
            this._partScriptData = partScriptData;
            this._carLoaderID = carLoaderID;
            this._UniqueID = UniqueID;
            this._partCountID = partCountID;
            this._partItemID = partItemID;
        }
    }
    public class PartScript_Info
    {
        public PartScript _partScript;
        public PartScriptInfo _PartScriptInfo;
        public int _partItemID;
        public int _partCountID;
        public int _carLoaderID;
        public Guid _UniqueID;

        public PartScript_Info(partType type, PartScript partScript, int carLoaderID, int partItemID, int partCountID)
        {
            this._partScript = partScript;
            this._UniqueID = Guid.NewGuid();
            this._carLoaderID = carLoaderID;
            this._partItemID = partItemID;
            this._partCountID = partCountID;
            this._PartScriptInfo = new PartScriptInfo(type, new PartScriptData(_partScript), _carLoaderID, _UniqueID, _partItemID, _partCountID);
        }
    }
    
    [Serializable]
    public class carData
    {
        public int clientID;
        public bool status;
        public bool isReady;
        
        public int carLoaderID;
        public string carID;
        public int carPosition;
        public bool fromServer;

        public C_Color carColor;
    }

    [Serializable]
    public class C_Color
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public C_Color(float _r, float _g, float _b, float _a)
        {
            r = _r;
            g = _g;
            b = _b;
            a = _a;
        }
        public C_Color(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public Color ToGame(C_Color color)
        {
            Color _color = new Color();
            _color.r = color.r;
            _color.g = color.g;
            _color.b = color.b;
            _color.a = color.a;
            
            return _color;
        }
    }

    [Serializable]
    public class C_PaintData
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

        public C_PaintData FromGame(PaintData data)
        {
            C_PaintData paintData = new C_PaintData();
            paintData.metal = data.metal;
            paintData.roughness = data.roughness;
            paintData.clearCoat = data.clearCoat;
            paintData.normalStrenght = data.normalStrength;
            paintData.fresnel = data.fresnel;

            paintData.p_metal = data.Metal;
            paintData.p_roughness = data.Roughness;
            paintData.p_clearCoat = data.ClearCoat;
            paintData.p_normalStrenght = data.NormalStrength;
            paintData.p_fresnel = data.Fresnel;
            return paintData;
        }
        public PaintData ToGame(C_PaintData data)
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
    public class MountedObjectData
    {
        
    }

    [Serializable]
    public enum partType
    {
        engine,
        suspensions,
        part
    }
}