using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.Shared.Data;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Car;

public static class CarHandle
{
    public static IEnumerator UpdatePartHandle()
    {
        foreach (ModCar car in ClientData.Instance.LoadedCars.Values)
        {
            MelonCoroutines.Start(CarHandle.HandleParts(car));
        }
        yield return new WaitForEndOfFrame();
    }
    
    
    public static IEnumerator HandleParts(ModCar car)
    {
        yield return new WaitForEndOfFrame();
        
        if(!car.isReferenced) yield break;
        
        var handleOtherPartsCoroutine = MelonCoroutines.Start(HandleOtherPartsCoroutine(car));
        var handleEnginePartsCoroutine = MelonCoroutines.Start(HandleEnginePartsCoroutine(car));
        var handleDriveshaftPartsCoroutine = MelonCoroutines.Start(HandleDriveshaftPartsCoroutine(car));
        var handleSuspensionpartsCoroutine = MelonCoroutines.Start(HandleSuspensionpartsCoroutine(car));
        var handleBodyPartsCoroutine = MelonCoroutines.Start(HandleBodyPartsCoroutine(car));
        
        yield return handleOtherPartsCoroutine;
        yield return handleEnginePartsCoroutine;
        yield return handleDriveshaftPartsCoroutine;
        yield return handleSuspensionpartsCoroutine;
        yield return handleBodyPartsCoroutine;

        yield return new WaitForEndOfFrame();

        if (!car.isHandled)
        {
            car.isHandled = true;
            if (car.isFromServer)
            {
                MelonLogger.Msg("Car is from server!");
                int waiter = 0;
                while (!car.CarFullyReceived || waiter < 20)
                {
                    yield return new WaitForSeconds(0.2f);
                    waiter += 1;
                }
                
                yield return new WaitForEndOfFrame();
                if (!car.CarFullyReceived)
                {
                    GameData.Instance.carLoaders[car.carLoaderID].DeleteCar();
                    ClientData.Instance.LoadedCars.Remove(car.carLoaderID);
                    ClientSend.SendResyncCar(car.carLoaderID);
                    yield break;
                }
                
                MelonLogger.Msg("Car is no Longer from server.");
                if (car.partInfo.BodyParts.Any(s => String.IsNullOrEmpty(s.Value.name)))
                {
                    var a = car.partInfo.BodyParts.First(s => String.IsNullOrEmpty(s.Value.name)).Value;
                    MelonLogger.Msg($"Found Empty ID on bodypart : {a.carPartID} , {a.name}");
                    
                }
                
                car.isFromServer = false;

            }
            else
            {
                car.carInfo =  new ModCarInfoData(GameData.Instance.carLoaders[car.carLoaderID].CarInfoData);
                car.fluidsData = new ModFluidsData(GameData.Instance.carLoaders[car.carLoaderID].FluidsData);
                ClientSend.SendCarInfoData(car.carInfo, car.carLoaderID);
                ClientSend.SendCarFluidsData(car.fluidsData, car.carLoaderID);

                List<ModCarPart> carParts = car.partInfo.BodyParts.Values.ToList();

                BodyPartsAdditionalCheck(carParts,car.partInfo.BodyPartsReferences.Values.ToList());
                
                ClientSend.SendPartsScript(car.partInfo.SuspensionParts.SelectMany(s => s.Value).ToList(), car.carLoaderID, ModPartType.suspension); 
                ClientSend.SendPartsScript(car.partInfo.OtherParts.SelectMany(s => s.Value).ToList(), car.carLoaderID, ModPartType.other); 
                ClientSend.SendBodyParts(carParts, car.carLoaderID); 
                ClientSend.SendPartsScript(car.partInfo.DriveshaftParts.Values.ToList(), car.carLoaderID, ModPartType.driveshaft); 
                ClientSend.SendPartsScript(car.partInfo.EngineParts.Values.ToList(), car.carLoaderID, ModPartType.engine); 
                
            }
            
            
        }
    }

    private static void BodyPartsAdditionalCheck(List<ModCarPart> beingSent, List<CarPart> reference)
    {
        MelonLogger.Msg("------ Additional Checks ------");
        MelonLogger.Msg($"ReferenceCount:{reference.Count} , HandleCount:{beingSent.Count}");
        for (int i = 0; i < beingSent.Count; i++)
        {
            beingSent[i].name = reference[beingSent[i].carPartID].name;
            beingSent[i].unmounted = reference[beingSent[i].carPartID].Unmounted;
        }
        MelonLogger.Msg("------ Additional Checks End ------");
    }
    
    private static IEnumerator HandleBodyPartsCoroutine(ModCar car)
    {
        yield return new WaitForEndOfFrame();
            
        var references = car.partInfo.BodyPartsReferences;
        var handle = car.partInfo.BodyParts;

        for (int i = 0; i < references.Count; i++)
        {
            if (!car.isHandled)
            {
                ModCarPart newPart = new ModCarPart(references[i], i);
                MelonLogger.Msg($"New carpart: {newPart.name} , {newPart.unmounted} ");
                if(!handle.ContainsKey(i))
                    handle.Add(i, newPart);
            }
            else
            {
                if (car.isFromServer)  yield break;
                if (CheckForDifferences(handle[i], references[i]))
                {
                    //MelonLogger.Msg($"{references[i].name} , {references[i].Unmounted} : {handle[i].name} , {handle[i].unmounted} ");
                    handle[i] = new ModCarPart(references[i], i);
                    ClientSend.SendBodyPart(car.carLoaderID, handle[i]);
                }
            }
        }
    }
    
    private static IEnumerator HandleSuspensionpartsCoroutine(ModCar car)
    {
        yield return new WaitForEndOfFrame();
            
        var references = car.partInfo.SuspensionPartsReferences;
        var handle = car.partInfo.SuspensionParts;

        for (int i = 0; i < references.Count; i++)
        {
            var parts = references[i];
            for (int j = 0; j < parts.Count; j++)
            {
                if (!car.isHandled)
                {
                    ModPartScript newPart = new ModPartScript(parts[j], i, j, ModPartType.suspension);
                        
                    if(!handle.ContainsKey(i))
                        handle.Add(i, new List<ModPartScript>());
                    if (!handle[i].Any(s => s.partID == newPart.partID && s.partIdNumber == newPart.partIdNumber))
                        handle[i].Add(newPart);
                }
                else
                {
                    if (car.isFromServer)  yield break;
                    if (CheckForDifferences(handle[i][j], parts[j])) 
                    {
                        handle[i][j] =  new ModPartScript(parts[j], i, j, ModPartType.suspension);
                        ClientSend.SendCarPart(car.carLoaderID, handle[i][j]);
                    }
                }
                    
            }
        }
        
    }
    
    private static IEnumerator HandleDriveshaftPartsCoroutine(ModCar car)
    {
        yield return new WaitForEndOfFrame();
            
        var references = car.partInfo.DriveshaftPartsReferences;
        var handle = car.partInfo.DriveshaftParts;

        for (int i = 0; i < references.Count; i++)
        {
            if (!car.isHandled)
            {
                ModPartScript newPart = new ModPartScript(references[i], i, -1, ModPartType.driveshaft);
                        
                if(!handle.ContainsKey(i))
                    handle.Add(i, newPart);
            }
            else
            {
                if (car.isFromServer)  yield break;
                if (CheckForDifferences(handle[i], references[i]))
                {
                    handle[i] = new ModPartScript(references[i], i, -1, ModPartType.driveshaft);
                    ClientSend.SendCarPart(car.carLoaderID, handle[i]);
                }
            }
        }
    }

    private static IEnumerator HandleEnginePartsCoroutine(ModCar car)
    {
        yield return new WaitForEndOfFrame();
            
        var references = car.partInfo.EnginePartsReferences;
        var handle = car.partInfo.EngineParts;

        for (int i = 0; i < references.Count; i++)
        {
            if (!car.isHandled)
            {
                ModPartScript newPart = new ModPartScript(references[i], i, -1, ModPartType.engine);
                        
                if(!handle.ContainsKey(i))
                    handle.Add(i, newPart);
            }
            else
            {
                if (car.isFromServer)  yield break;
                if (CheckForDifferences(handle[i], references[i]))
                {
                    handle[i] =  new ModPartScript(references[i], i, -1, ModPartType.engine);
                    ClientSend.SendCarPart(car.carLoaderID, handle[i]);
                }
            }
        }
    }

    private static IEnumerator HandleOtherPartsCoroutine(ModCar car)
    {
        yield return new WaitForEndOfFrame();
            
        var references = car.partInfo.OtherPartsReferences;
        var handle = car.partInfo.OtherParts;

        for (int i = 0; i < references.Count; i++)
        {
            var parts = references[i];
            for (int j = 0; j < parts.Count; j++)
            {
                if (!car.isHandled)
                {
                    ModPartScript newPart = new ModPartScript(parts[j], i, j, ModPartType.other);
                        
                    if(!handle.ContainsKey(i))
                        handle.Add(i, new List<ModPartScript>());
                    if (!handle[i].Any(s => s.partID == newPart.partID && s.partIdNumber == newPart.partIdNumber))
                        handle[i].Add(newPart);
                }
                else
                {
                    if (car.isFromServer)  yield break;
                    if (CheckForDifferences(handle[i][j], parts[j]))
                    {
                        handle[i][j] = new ModPartScript(parts[j], i, j, ModPartType.other);
                        ClientSend.SendCarPart(car.carLoaderID, handle[i][j]);
                    }
                }
                    
            }
        }
    }
    
    private static bool CheckForDifferences(ModPartScript handled, PartScript toHandle) // TODO:Add Paint check
    {
        if (handled.unmounted != toHandle.IsUnmounted)
            return true;
        if (Math.Abs(handled.dust - toHandle.Dust) > 0.1f)
            return true;
        if (Math.Abs(handled.condition - toHandle.Condition) > 0.1f)
            return true;
        if (Math.Abs(handled.condition - toHandle.Condition) > 0.1f)
            return true;

        return false;
    }
    private static bool CheckForDifferences(ModCarPart handled, CarPart toHandle)
    {
        if (handled == null || toHandle == null) { return false;}

        if (handled.unmounted != toHandle.Unmounted)
            return true;
        if (handled.tunedID != toHandle.TunedID) 
            return true;
        if (Math.Abs(handled.condition - toHandle.Condition) > 0.01f)
            return true;
        if (handled.switched != toHandle.Switched)
            return true;
        if ((Math.Abs(handled.Dust - toHandle.Dust) > 0.01f))
            return true;
        if ((Math.Abs(handled.dent - toHandle.Dent) > 0.01f))
            return true;
        if ((PaintType)handled.paintType != toHandle.PaintType)
            return true;
        if (handled.paintData.ToGame() != toHandle.PaintData)
            return true;
        if ((Math.Abs(handled.conditionPaint - toHandle.ConditionPaint) > 0.01f))
            return true;
         if (handled.colors.IsDifferent(toHandle.Color))
             return true;
         if (handled.TintColor.IsDifferent(toHandle.TintColor))
             return true;
         if (handled.isTinted != toHandle.IsTinted)
             return true;
         if (handled.livery != toHandle.Livery)
             return true;
         if (Math.Abs(handled.liveryStrength - toHandle.LiveryStrength) > 0.01f)
             return true;
         if (Math.Abs(handled.conditionStructure - toHandle.StructureCondition) > 0.01f)
             return true;
         if (Math.Abs(handled.washFactor - toHandle.WashFactor) > 0.01f)
             return true;

        return false;
    }
}