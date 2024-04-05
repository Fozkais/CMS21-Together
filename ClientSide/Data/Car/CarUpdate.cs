using System;
using System.Collections;
using System.Linq;
using CMS21Together.Shared.Data;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Car
{
    public static class CarUpdate
    {
        public static IEnumerator CarSpawnFade(ModCar car, CarLoader carLoader)
        {
            ScreenFader.Get().NormalFadeIn();

            carLoader.gameObject.GetComponentInChildren<CarDebug>().LoadCar(car.carID, car.carVersion);
            carLoader.ChangePosition(car.carPosition);

            int count = 0;
            while (!(car.isReferenced && car.isHandled) && count < 20)
            {
                count += 1;
                yield return new WaitForSeconds(0.1f);
            }
            int _count = 0;
            while (!(car.receivedOtherParts && car.receivedEngineParts 
                                           && car.receivedSuspensionParts && car.receivedBodyParts) && _count < 25)
            {
                _count += 1;
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForEndOfFrame();
            
            ScreenFader.Get().NormalFadeOut();
        }


        public static IEnumerator HandleNewPart(ModPartScript part, int carLoaderID)
        {
            bool partIsValid = ClientData.LoadedCars.Any(s => s.Value.carLoaderID == carLoaderID);
            if (partIsValid)
            {
                var car = ClientData.LoadedCars.First(s => s.Value.carLoaderID == carLoaderID).Value;
                
                
                int count = 0;
                while (!(car.isReferenced && car.isHandled) && count < 20)
                {
                    count += 1;
                    yield return new WaitForSeconds(0.1f);
                }
                
                int _count = 0;
                while (!(car.receivedOtherParts && car.receivedEngineParts && car.receivedSuspensionParts) && _count < 20)
                {
                    _count += 1;
                    yield return new WaitForSeconds(0.1f);
                }
                
                yield return new WaitForEndOfFrame();
                
                PrepareUpdatePart(car, part);
            }
        }

        public static void PrepareUpdatePart(ModCar car, ModPartScript part)
        {
            
            PartScript reference;
            switch (part.type)
            {
                case ModPartType.other:
                    reference = car.partInfo.OtherPartsReferences[part.partID][part.partIdNumber];
                    car.partInfo.OtherParts[part.partID][part.partIdNumber] = part;
                    break;
                case ModPartType.engine:
                    reference = car.partInfo.EnginePartsReferences[part.partID];
                    car.partInfo.EngineParts[part.partID] = part;
                    break;
                case ModPartType.driveshaft:
                    reference = car.partInfo.DriveshaftPartsReferences[part.partID];
                    car.partInfo.DriveshaftParts[part.partID] = part;
                    break;
                case ModPartType.suspension:
                    reference = car.partInfo.SuspensionPartsReferences[part.partID][part.partIdNumber];
                    car.partInfo.SuspensionParts[part.partID][part.partIdNumber] = part;
                    break;
                default:
                    MelonLogger.Msg("Invalid part received!");
                    return;
            }
            UpdatePart(car.carLoaderID, part, reference);
            
        }

        private static void UpdatePart(int carLoaderID, ModPartScript part, PartScript reference)
        {
            if( part == null || reference == null) { MelonLogger.Msg("Invalid part!"); return;}
            
            MelonLogger.Msg("Updating Part.");
            
            if (!String.IsNullOrEmpty(part.tunedID))
            {
                if (reference.tunedID != part.tunedID)
                {
                    GameData.Instance.carLoaders[carLoaderID].TunePart(reference.tunedID, part.tunedID);
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
                if (reference.IsUnmounted)
                {
                    MelonCoroutines.Start(CarHarmonyPatches.ResetCursorBlockCoroutine());
                    reference.ShowBySaveGame();
                    reference.ShowMountAnimation();
                    reference.FastMount();
                }
                    
                //Wheel Handle
                var wheelData =  GameData.Instance.carLoaders[carLoaderID].WheelsData;
                for (int i = 0; i <  GameData.Instance.carLoaders[carLoaderID].WheelsData.Wheels.Count; i++)
                {
                    GameData.Instance.carLoaders[carLoaderID].SetWheelSize((int)wheelData.Wheels[i].Width, 
                        (int)wheelData.Wheels[i].Size, (int)wheelData.Wheels[i].Profile, (WheelType)i);
                    GameData.Instance.carLoaders[carLoaderID].SetET((WheelType)i, wheelData.Wheels[i].ET);
                }
                GameData.Instance.carLoaders[carLoaderID].SetWheelSizes();
            }
            else
            {
                
                reference.Quality = part.quality;
                reference.SetCondition(part.condition, true);
                reference.UpdateDust(part.dust, true);
                if (reference.IsUnmounted == false)
                {
                    MelonCoroutines.Start(CarHarmonyPatches.ResetCursorBlockCoroutine());
                    reference.HideBySavegame(false, GameData.Instance.carLoaders[carLoaderID]);
                }
            }
        }

        public static IEnumerator HandleNewBodyPart(ModCarPart carPart, int carLoaderID)
        {
            bool partIsValid = ClientData.LoadedCars.Any(s => s.Value.carLoaderID == carLoaderID);
            if (partIsValid)
            {
                var car = ClientData.LoadedCars.First(s => s.Value.carLoaderID == carLoaderID).Value;
                
                int count = 0;
                while (!(car.isReferenced && car.isHandled) && count < 20)
                {
                    count += 1;
                    yield return new WaitForSeconds(0.1f);
                }
                
                int _count = 0;
                while (!car.receivedBodyParts && _count < 20)
                {
                    _count += 1;
                    yield return new WaitForSeconds(0.1f);
                }
                yield return new WaitForEndOfFrame();
                
                UpdateBodyPart(car, carPart);
            }
        }

        private static void UpdateBodyPart(ModCar car, ModCarPart carPart)
        {
            CarPart reference = car.partInfo.BodyPartsReferences[carPart.carPartID];
            car.partInfo.BodyParts[carPart.carPartID] = carPart;
            
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
            
            reference.IsTinted = carPart.isTinted;
            //reference.TintColor = tintColor;
            //reference.Color = color;
            reference.PaintType = (PaintType)carPart.paintType;
            reference.OutsideRustEnabled = carPart.outsaidRustEnabled;
            reference.AdditionalString = carPart.additionalString;
            reference.Quality = carPart.quality;
            reference.WashFactor = carPart.washFactor;
            
            if (!reference.Unmounted && !reference.name.StartsWith("license_plate"))
            {
               // GameData.carLoaders[carLoaderId].SetCustomCarPaintType(originalPart, updatedPart.paintData.ToGame(updatedPart.paintData));  
               // GameData.carLoaders[carLoaderId].SetCarColorAndPaintType(originalPart, color, (PaintType)updatedPart.paintType);
               if(carPart.colors != null)
                    GameData.Instance.carLoaders[car.carLoaderID].SetCarColor(reference, color);
               if(carPart.TintColor != null)
                    GameData.Instance.carLoaders[car.carLoaderID].SetCarPaintType(reference, (PaintType)carPart.paintType);
            }
            GameData.Instance.carLoaders[car.carLoaderID].SetCarLivery(reference, carPart.livery, carPart.liveryStrength);

            if (!reference.Unmounted && carPart.unmounted)
            {
                GameData.Instance.carLoaders[car.carLoaderID].TakeOffCarPartFromSave(carPart.name);
            }
            
            if (reference.Unmounted && !carPart.unmounted)
            {
                GameData.Instance.carLoaders[car.carLoaderID].TakeOnCarPartFromSave(carPart.name);
            }
            
            if (reference.Switched != carPart.switched)
                GameData.Instance.carLoaders[car.carLoaderID].SwitchCarPart(reference, false, carPart.switched);


            if(carPart.isTinted && carPart.TintColor != null)
                PaintHelper.SetWindowProperties(reference.handle, (int)(carPart.TintColor.a * 255), tintColor);
            
            GameData.Instance.carLoaders[car.carLoaderID].UpdateCarBodyPart(reference);
        }
    }
}