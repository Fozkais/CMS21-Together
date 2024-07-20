using System;
using System.Collections;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using CMS21Together.Shared.Data.Vanilla.Cars;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Car;

public static class PartsUpdater
{
    private static IEnumerator IsCarReady(int carLoaderID)
    {
        if (ClientData.Instance.loadedCars.TryGetValue(carLoaderID, out ModCar car))
        {
            while (!car.isReady)
                yield return new WaitForSeconds(0.25f);
        }
    }
    
    public static IEnumerator UpdatePartScripts(ModPartScript partScript, int carLoaderID)
    {
        var waitforCar = MelonCoroutines.Start(IsCarReady(carLoaderID));
        yield return waitforCar;

        yield return new WaitForEndOfFrame();
        
        MelonLogger.Msg("[PartsUpdater->UpdatePartScripts] Car ready, updating..");
        if (ClientData.Instance.loadedCars.TryGetValue(carLoaderID, out ModCar car))
        {
            int key = partScript.partID;
            int index = partScript.partIdNumber;

            PartScript reference;
            
            switch (partScript.type)
            {
                case ModPartType.engine:
                    reference = car.partInfo.EnginePartsReferences[key];
                    break;
                case ModPartType.suspension:
                    reference = car.partInfo.SuspensionPartsReferences[key][index];
                    break;
                case ModPartType.other:
                    reference = car.partInfo.OtherPartsReferences[key][index];
                    break;
                case ModPartType.driveshaft:
                    reference = car.partInfo.DriveshaftPartsReferences[key];
                    break;
                default:
                    yield break;
            }
            
            MelonLogger.Msg("[PartsUpdater->UpdatePartScripts] updating PartScript..");
            UpdatePartScript(partScript, reference, carLoaderID);
        }
    }
    private static void UpdatePartScript(ModPartScript part, PartScript reference, int carLoaderID)
    {
        if( part == null || reference == null) { MelonLogger.Msg("Invalid part!"); return;}
        
          if (!String.IsNullOrEmpty(part.tunedID) && !String.IsNullOrEmpty(reference.tunedID))
        {
            if (reference.tunedID != part.tunedID)
            {
                GameData.Instance.carLoaders[carLoaderID].TunePart(reference.id, part.tunedID);
            }
        }

        reference.IsExamined = part.isExamined;

        if (!part.unmounted)
        {
            reference.IsPainted = part.isPainted;
            if (part.isPainted)
            {
                reference.CurrentPaintType = (PaintType)part.paintType;
                reference.CurrentPaintData = new ModPaintData().ToGame(part.paintData);
                reference.SetColor(ModColor.ToColor(part.color));
                if ((PaintType)part.paintType == PaintType.Custom)
                {
                    PaintHelper.SetCustomPaintType(reference.gameObject, part.paintData.ToGame(part.paintData), false);
                }
                else
                {
                    PaintHelper.SetPaintType(reference.gameObject, (PaintType)part.paintType, false);
                }
            }
            reference.Quality = part.quality;
            reference.SetCondition(part.condition);
            reference.UpdateDust(part.dust, true);
            reference.SetConditionNormal(part.condition);
            if (reference.IsUnmounted)
            {
                reference.ShowBySaveGame();
                
                reference.Show();
                MelonCoroutines.Start(CustomPartScriptMethod.ShowMounted(reference));
                reference.ShowMountAnimation();
                
                reference.SetCondition(part.condition);
                reference.SetConditionNormal(part.condition);
                
            }
            
            if (carLoaderID != -1)
            {
                var wheelData =  GameData.Instance.carLoaders[carLoaderID].WheelsData;
                for (int i = 0; i <  GameData.Instance.carLoaders[carLoaderID].WheelsData.Wheels.Count; i++)
                {
                    GameData.Instance.carLoaders[carLoaderID].SetWheelSize((int)wheelData.Wheels[i].Width, 
                        (int)wheelData.Wheels[i].Size, (int)wheelData.Wheels[i].Profile, (WheelType)i);
                    GameData.Instance.carLoaders[carLoaderID].SetET((WheelType)i, wheelData.Wheels[i].ET);
                }
                GameData.Instance.carLoaders[carLoaderID].SetWheelSizes();
            }
        }
        else
        {
            
            reference.Quality = part.quality;
            reference.SetCondition(part.condition, true);
            reference.UpdateDust(part.dust, true);
            reference.SetConditionNormal(part.condition);
            if (reference.IsUnmounted == false)
            {
                if(carLoaderID != -1)
                    reference.HideBySavegame(false, GameData.Instance.carLoaders[carLoaderID]);
                else
                    reference.HideBySavegame(false);
                
            }
        }
    }
    public static IEnumerator UpdateBodyParts(ModCarPart carPart, int carLoaderID)
    {
        var waitforCar = MelonCoroutines.Start(IsCarReady(carLoaderID));
        yield return waitforCar;
        yield return new WaitForEndOfFrame();
        
        MelonLogger.Msg("[PartsUpdater->UpdateBodyParts] Car ready, updating..");
        if (ClientData.Instance.loadedCars.TryGetValue(carLoaderID, out ModCar car))
        {
            int key = carPart.carPartID;
            
            CarPart reference = car.partInfo.BodyPartsReferences[key];
            MelonLogger.Msg("[PartsUpdater->UpdateBodyParts] Updating BodyPart..");
            UpdateBodyPart(carPart, reference, carLoaderID);
        }
    }
    private static void UpdateBodyPart(ModCarPart carPart, CarPart reference, int carLoaderID)
    {
        if( carPart == null || reference == null) { MelonLogger.Msg("Invalid bodyPart!"); return;}
        
        Color color = ModColor.ToColor(carPart.colors);
        Color tintColor =  ModColor.ToColor(carPart.TintColor);

        if(reference.TunedID != carPart.tunedID)
            GameData.Instance.carLoaders[carLoaderID].TunePart(reference.name, carPart.tunedID);
        
        GameData.Instance.carLoaders[carLoaderID].SetDent(reference, carPart.dent);
        GameData.Instance.carLoaders[carLoaderID].EnableDust(reference, carPart.Dust);
        GameData.Instance.carLoaders[carLoaderID].SetCondition(reference, carPart.condition);
        GameData.Instance.carLoaders[carLoaderID].SetCarLivery(reference, carPart.livery, carPart.liveryStrength);

        reference.name = carPart.name;
        reference.IsTinted = carPart.isTinted;
        reference.PaintType = (PaintType)carPart.paintType;
        reference.OutsideRustEnabled = carPart.outsaidRustEnabled;
        reference.AdditionalString = carPart.additionalString;
        reference.Quality = carPart.quality;
        reference.WashFactor = carPart.washFactor;
        reference.StructureCondition = carPart.conditionStructure;
        reference.ConditionPaint = carPart.conditionPaint;
        
        if (!carPart.unmounted && !reference.name.StartsWith("license_plate"))
        {
           if(carPart.colors != null)
                GameData.Instance.carLoaders[carLoaderID].SetCarColor(reference, color);
           if(carPart.TintColor != null)
                GameData.Instance.carLoaders[carLoaderID].SetCarPaintType(reference, (PaintType)carPart.paintType);
        }

        if (!reference.Unmounted && carPart.unmounted)
            GameData.Instance.carLoaders[carLoaderID].TakeOffCarPartFromSave(reference.name);
        
        if (reference.Unmounted && !carPart.unmounted)
            GameData.Instance.carLoaders[carLoaderID].TakeOnCarPartFromSave(reference.name);
        
        if (reference.Switched != carPart.switched)
            GameData.Instance.carLoaders[carLoaderID].SwitchCarPart(reference, false, carPart.switched);

        foreach (var _carPart in carPart.connectedParts)
        {
            int key = _carPart.carPartID;
            CarPart _reference = ClientData.Instance.loadedCars[carLoaderID].partInfo.BodyPartsReferences[key];
            MelonLogger.Msg("[PartsUpdater->UpdateBodyParts] Updating BodyPart..");
            UpdateBodyPart(_carPart, _reference, carLoaderID);
        }

        if(carPart.isTinted && carPart.TintColor != null)
            PaintHelper.SetWindowProperties(reference.handle, (int)(carPart.TintColor.a * 255), tintColor);
        
        GameData.Instance.carLoaders[carLoaderID].SetCondition(reference, carPart.condition);
        GameData.Instance.carLoaders[carLoaderID].UpdateCarBodyPart(reference);
    }
}