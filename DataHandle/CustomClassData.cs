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
    public class carPartsData
    {
        public Guid UniqueID;
        
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
        public carPartsData(CarPart _part, int _carPartID, int _carLoaderID, Guid _UniqueID)
        {
            this.UniqueID = _UniqueID;
            
            this.carPartID = _carPartID;
            this.carLoaderID = _carLoaderID;
            this.name = _part.name;
            this.switched = _part.Switched;
            this.inprogress = _part.Switched;
            this.condition = _part.Condition;
            this.unmounted = _part.Unmounted;
            this.tunedID = _part.TunedID;
            this.isTinted = _part.IsTinted;
            this.TintColor = new C_Color(_part.TintColor);
            this.colors = new C_Color(_part.Color);
            this.paintType = (int)_part.PaintType;
            this.paintData = new C_PaintData().FromGame(_part.PaintData);
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
    public class carPartsData_info
    {
        public carPartsData _CarPartsData;
        public int _carLoaderID;
        public Guid _UniqueID;
        public int _partCountID;
        public CarPart _originalPart;

        public carPartsData_info(CarPart originalPart, int carLoaderID, int partCountID)
        {
            this._carLoaderID = carLoaderID;
            this._partCountID = partCountID;
            this._originalPart = originalPart;
            this._UniqueID = Guid.NewGuid();
            this._CarPartsData = new carPartsData(this._originalPart, this._partCountID, this._carLoaderID, this._UniqueID);
        }
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

        public Guid _UniqueID;
        public int clientID;
        public bool status;
        public bool isReady;
        
        public int carLoaderID;
        public string carID;
        public int carPosition;
        public int configNumber;
        public bool CarPartFromServer;
        public bool PartFromServer;
        public bool SuspensionFromServer;
        public bool EngineFromServer;

        public C_Color carColor;

        public carData(CarLoader carLoader, int index, bool fromServer)
        {
            this.carLoaderID = index;
            this.carID = carLoader.carToLoad;
            this.carPosition = carLoader.placeNo;
            this.configNumber = carLoader.ConfigVersion;
            this.carColor = new C_Color(carLoader.color.r, carLoader.color.g, carLoader.color.b, carLoader.color.a);
            this._UniqueID = Guid.NewGuid();
            this.status = true;
            this.CarPartFromServer = fromServer;
            this.PartFromServer = fromServer;
            this.SuspensionFromServer = fromServer;
            this.EngineFromServer = fromServer;
            }
    }

    [Serializable]
    public class C_Color
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public C_Color()
        {
        }
        
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