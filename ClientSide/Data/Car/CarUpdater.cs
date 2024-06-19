using System;
using System.Collections;
using System.Collections.Generic;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Car;

public static class CarUpdater
{
    public static IEnumerator UpdatePartScripts(List<ModPartScript> partScripts, int carLoaderID)
    {
        if(ModSceneManager.currentScene() != GameScene.garage) yield break;

        foreach (ModPartScript partScript in partScripts)
        {
            UpdatePartScript(partScript, ClientData.Instance.LoadedCars[carLoaderID]);
        }

        yield return new WaitForEndOfFrame();
    }

    public static void UpdatePartScript(ModPartScript partScript, ModCar car)
    {
        if(ModSceneManager.currentScene() != GameScene.garage) return;
        
        PartScript reference;
        switch (partScript.type)
        {
            case ModPartType.other:
                reference = car.partInfo.OtherPartsReferences[partScript.partID][partScript.partIdNumber];
                car.partInfo.OtherParts[partScript.partID][partScript.partIdNumber] = partScript;
                break;
            case ModPartType.engine:
                reference = car.partInfo.EnginePartsReferences[partScript.partID];
                car.partInfo.EngineParts[partScript.partID] = partScript;
                break;
            case ModPartType.driveshaft:
                reference = car.partInfo.DriveshaftPartsReferences[partScript.partID];
                car.partInfo.DriveshaftParts[partScript.partID] = partScript;
                break;
            case ModPartType.suspension:
                reference = car.partInfo.SuspensionPartsReferences[partScript.partID][partScript.partIdNumber];
                car.partInfo.SuspensionParts[partScript.partID][partScript.partIdNumber] = partScript;
                break;
            default:
                MelonLogger.Msg("Invalid part received!");
                return;
        }
        UpdatePart(car.carLoaderID, partScript, reference);
    }

    public static void UpdatePart(int carLoaderID, ModPartScript partScript, PartScript reference)
    {
        if( partScript == null || reference == null) { MelonLogger.Msg("Invalid part!"); return;}

        if (!String.IsNullOrEmpty(partScript.tunedID) && !String.IsNullOrEmpty(reference.tunedID))
        {
            if (reference.tunedID != partScript.tunedID)
            {
                GameData.Instance.carLoaders[carLoaderID].TunePart(reference.id, partScript.tunedID);
            }
        }

        reference.IsExamined = partScript.isExamined;

        if (!partScript.unmounted)
        {
            reference.IsPainted = partScript.isPainted;
            if (partScript.isPainted)
            {
                reference.CurrentPaintType = (PaintType)partScript.paintType;
                reference.CurrentPaintData = new ModPaintData().ToGame(partScript.paintData);
                reference.SetColor(ModColor.ToColor(partScript.color));
                if ((PaintType)partScript.paintType == PaintType.Custom)
                {
                    PaintHelper.SetCustomPaintType(reference.gameObject, partScript.paintData.ToGame(partScript.paintData), false);
                }
                else
                {
                    PaintHelper.SetPaintType(reference.gameObject, (PaintType)partScript.paintType, false);
                }
            }
            reference.Quality = partScript.quality;
            reference.SetCondition(partScript.condition);
            reference.UpdateDust(partScript.dust, true);
            reference.SetConditionNormal(partScript.condition);
            if (reference.IsUnmounted)
            {
                reference.ShowBySaveGame();
                reference.FastMount();
                reference.ShowMountAnimation();
                
                
                reference.SetCondition(partScript.condition);
                reference.SetConditionNormal(partScript.condition);
                
            }
                
            //Wheel Handle
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
            
            reference.Quality = partScript.quality;
            reference.SetCondition(partScript.condition, true);
            reference.UpdateDust(partScript.dust, true);
            reference.SetConditionNormal(partScript.condition);
            if (reference.IsUnmounted == false)
            {
                if(carLoaderID != -1)
                    reference.HideBySavegame(false, GameData.Instance.carLoaders[carLoaderID]);
                else
                    reference.HideBySavegame(false);
                
            }
        }
    }

    public static IEnumerator UpdateCarParts(List<ModCarPart> carParts, int carLoaderID)
    {
        if(ModSceneManager.currentScene() != GameScene.garage) yield break;

        foreach (ModCarPart carPart in carParts)
        {
            UpdateCarPart(carPart, ClientData.Instance.LoadedCars[carLoaderID]);
        }

        yield return new WaitForEndOfFrame();
    }

    public static void UpdateCarPart(ModCarPart carPart, ModCar car)
    {
        CarPart reference = car.partInfo.BodyPartsReferences[carPart.carPartID];
        car.partInfo.BodyParts[carPart.carPartID] = carPart;
        
        UpdatePart(car.carLoaderID, carPart, reference);
    }
    
    private static void UpdatePart(int carLoaderID, ModCarPart carPart, CarPart reference)
    {
        ModCar car = ClientData.Instance.LoadedCars[carLoaderID];
        
        Color color = Color.black;
        Color tintColor = Color.black;
        
        if (carPart.colors != null)
            color = ModColor.ToColor(carPart.colors);
        
        if (carPart.TintColor != null)
            tintColor = ModColor.ToColor(carPart.TintColor);

        if(reference.TunedID != carPart.tunedID)
            GameData.Instance.carLoaders[car.carLoaderID].TunePart(reference.name, carPart.tunedID);
        
        GameData.Instance.carLoaders[car.carLoaderID].SetDent(reference, carPart.dent);
        GameData.Instance.carLoaders[car.carLoaderID].EnableDust(reference, carPart.Dust);
        GameData.Instance.carLoaders[car.carLoaderID].SetCondition(reference, carPart.condition);
        GameData.Instance.carLoaders[car.carLoaderID].SetCarLivery(reference, carPart.livery, carPart.liveryStrength);

        reference.name = carPart.name;
        reference.IsTinted = carPart.isTinted;
        reference.PaintType = (PaintType)carPart.paintType;
        reference.OutsideRustEnabled = carPart.outsaidRustEnabled;
        reference.AdditionalString = carPart.additionalString;
        reference.Quality = carPart.quality;
        reference.WashFactor = carPart.washFactor;
        reference.StructureCondition = carPart.conditionStructure;
        reference.ConditionPaint = carPart.conditionPaint;
        
        if (!reference.Unmounted && !reference.name.StartsWith("license_plate"))
        {
           if(carPart.colors != null)
                GameData.Instance.carLoaders[car.carLoaderID].SetCarColor(reference, color);
           if(carPart.TintColor != null)
                GameData.Instance.carLoaders[car.carLoaderID].SetCarPaintType(reference, (PaintType)carPart.paintType);
        }
        GameData.Instance.carLoaders[car.carLoaderID].SetCarLivery(reference, carPart.livery, carPart.liveryStrength);

        if (!reference.Unmounted && carPart.unmounted)
            GameData.Instance.carLoaders[car.carLoaderID].TakeOffCarPartFromSave(reference.name);
        
        if (reference.Unmounted && !carPart.unmounted)
            GameData.Instance.carLoaders[car.carLoaderID].TakeOnCarPartFromSave(reference.name);
        
        if (reference.Switched != carPart.switched)
            GameData.Instance.carLoaders[car.carLoaderID].SwitchCarPart(reference, false, carPart.switched);


        if(carPart.isTinted && carPart.TintColor != null)
            PaintHelper.SetWindowProperties(reference.handle, (int)(carPart.TintColor.a * 255), tintColor);
        
        GameData.Instance.carLoaders[car.carLoaderID].SetCondition(reference, carPart.condition);
        GameData.Instance.carLoaders[car.carLoaderID].UpdateCarBodyPart(reference);
    }
}