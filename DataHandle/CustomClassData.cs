using System;
using System.Collections.Generic;
using Il2Cpp;
using Il2CppCMS.Containers;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CMS21MP.DataHandle
{
    [Serializable]
    public class PartScriptData
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
       // public List<ModMountObject> MountObjects;

        public PartScriptData(PartScript data)
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
          //  foreach (MountObject dataMountObject in data.MountObjects)
           // {
            //    this.MountObjects.Add();
           // }
            this.unmounted = data.IsUnmounted;
        }
    }

    [Serializable]
    public class ModMountObject
    {
        public bool alternativeSFX;
        public bool canAction;
        public bool canBeUnmount;
        public bool canUpdate;
        public float Condition;
        public bool IsStuck;
        public float length;
        public float mountState;
        public bool reverseMode;
        public bool unmounted;


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
            this._CarPartsData =
                new carPartsData(this._originalPart, this._partCountID, this._carLoaderID, this._UniqueID);
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

        public PartScriptInfo(partType type, PartScriptData partScriptData, int carLoaderID, Guid UniqueID,
            int partItemID, int partCountID)
        {
            this._type = type;
            this._partScriptData = partScriptData;
            this._carLoaderID = carLoaderID;
            this._UniqueID = UniqueID;
            this._partCountID = partCountID;
            this._partItemID = partItemID;
        }
    }

    public class ModPartScript_Info
    {
        public PartScript _partScript;
        public PartScriptInfo _PartScriptInfo;
        public int _partItemID;
        public int _partCountID;
        public int _carLoaderID;
        public Guid _UniqueID;

        public ModPartScript_Info(partType type, PartScript partScript, int carLoaderID, int partItemID, int partCountID)
        {
            this._partScript = partScript;
            this._UniqueID = Guid.NewGuid();
            this._carLoaderID = carLoaderID;
            this._partItemID = partItemID;
            this._partCountID = partCountID;
            this._PartScriptInfo = new PartScriptInfo(type, new PartScriptData(_partScript), _carLoaderID, _UniqueID,
                _partItemID, _partCountID);
        }
    }

    [Serializable]
    public class carData
    {
        public int clientID;
        public bool status;
        public bool isReady;
        public string Scene;

        public bool isOnLifter;
        public CarLifterState LifterState;

        public int carLoaderID;
        public string carID;
        public int carPosition;
        public int configNumber;
        public bool CarPartFromServer;
        public bool PartFromServer;
        public bool SuspensionFromServer;
        public bool EngineFromServer;

        public bool FinishedPreHandlingPart;
        public bool FinishedPreHandlingEngine;
        public bool FinishedPreHandlingSuspension;
        public bool FinishedPreHandlingBodyPart;

        public ModColor carColor;

        public carData(CarLoader carLoader, int index, bool fromServer)
        {
            this.Scene = SceneManager.GetActiveScene().name;
            this.carLoaderID = index;
            this.carID = carLoader.carToLoad;
            this.carPosition = carLoader.placeNo;
            this.configNumber = carLoader.ConfigVersion;
            this.carColor = new ModColor(carLoader.color.r, carLoader.color.g, carLoader.color.b, carLoader.color.a);
            this.status = true;
            this.CarPartFromServer = fromServer;
            this.PartFromServer = fromServer;
            this.SuspensionFromServer = fromServer;
            this.EngineFromServer = fromServer;
            this.FinishedPreHandlingPart = false;
            this.FinishedPreHandlingEngine = false;
            this.FinishedPreHandlingSuspension = false;
            this.FinishedPreHandlingBodyPart = false;
        }
    }

    [Serializable]
    public class ModColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public ModColor()
        {
        }

        public ModColor(float _r, float _g, float _b, float _a)
        {
            r = _r;
            g = _g;
            b = _b;
            a = _a;
        }

        public ModColor(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public Color ToGame(ModColor color)
        {
            Color _color = new Color();
            _color.r = color.r;
            _color.g = color.g;
            _color.b = color.b;
            _color.a = color.a;

            return _color;
        }

        public Color Convert(ModColor color)
        {
            Color _color = new Color();
            ModColor modColor = new ModColor();
            _color = modColor.ToGame(color);
            return _color;
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

        public ModPaintData()
        {
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
        
        public PaintData Convert(ModPaintData data)
        {
            PaintData paintData = new PaintData();
            ModPaintData _newData = new ModPaintData();
            paintData = _newData.ToGame(data);
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
        public ModItemGroup.ModLPData LPData;
        public ModItemGroup.ModMountObjectData MountObjectData;
        public string NormalID;
        public bool OutsideRustEnabled;
        public ModPaintData PaintData;
        public PaintType PaintType;
        public int Quality;
        public int RepairAmount;
        public ModColor TintColor;
        public ModItemGroup.ModTuningData TuningData;
        public float WashFactor;
        public ModItemGroup.ModWheelData WheelData;
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
            this.WheelData = new ModItemGroup.ModWheelData(item.WheelData);
            this.ID = item.ID;
            this.UID = item.UID;
        }

        public void ToGame(ModItem item, Item original)
        {
            original.Color = new CustomColor(new ModColor().Convert(item.Color));
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
            original.PaintData = new ModPaintData().Convert(item.PaintData);
            original.PaintType = item.PaintType;
            original.Quality = item.Quality;
            original.RepairAmount = item.RepairAmount;
            original.TintColor = new CustomColor(new ModColor().Convert(item.Color));
            //this.TuningData = item.TuningData; TODO: Handle class
            original.WashFactor = item.WashFactor;
            original.WheelData = new ModItemGroup.ModWheelData().ToGame(item.WheelData);
            original.ID = item.ID;
            original.UID = item.UID;
        }
    }

    [Serializable]
    public class ModGearboxData
    {
        // TODO: Handle class
    }
    

    [Serializable]
    public class ModItemGroup
    {
        public int Index;
        
        public bool IsNormalGroup;
        public List<ModItem> ItemList = new List<ModItem>();
        public float Size;
        public string ID;
        public long UID;

        public ModItemGroup()
        {
        }

        public ModItemGroup(GroupItem item, int Index)
        {
            this.IsNormalGroup = item.IsNormalGroup;
            foreach (Item _item in item.ItemList)
            {
                this.ItemList.Add(new ModItem(_item));
            }

            this.Size = item.Size;
            this.ID = item.ID;
            this.UID = item.UID;
        }

        public void ToGame(ModItemGroup ModGroupItem, GroupItem original)
        {
            original.IsNormalGroup = ModGroupItem.IsNormalGroup;
            original.ItemList = Convert(ModGroupItem.ItemList);
            original.Size = ModGroupItem.Size;
            original.ID = ModGroupItem.ID;
            original.UID = ModGroupItem.UID;
        }

        public Il2CppSystem.Collections.Generic.List<Item> Convert(List<ModItem> items)
        {
            Il2CppSystem.Collections.Generic.List<Item> FinalItem = new Il2CppSystem.Collections.Generic.List<Item>();
            foreach (var item in items)
            {
                Item newItem = new Item();
                item.ToGame(item, newItem);
                FinalItem.Add(newItem);
            }

            return FinalItem;
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
        public class ModMountObjectData
        {
            // TODO: Handle MountObjectData Class
        }
    }
}