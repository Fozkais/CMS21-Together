using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.ClientSide.Handle;
using CMS21Together.Shared.Data;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Car
{
    public static class PartsHandle
    {
        public static IEnumerator HandleParts(ModCar car)
        {
            yield return new WaitForEndOfFrame();

            if (car.isReferenced)
            {
                List<ModPartScript> otherPartsBuffer = null;
                List<ModPartScript> enginePartsBuffer = null;
                List<ModPartScript> driveshaftPartsBuffer = null;
                List<ModPartScript> suspensionPartsBuffer = null;
                List<ModCarPart> bodyPartsBuffer = null;
                
                object handleOtherPartsCoroutine;
                object handleEnginePartsCoroutine;
                object handleDriveshaftPartsCoroutine;
                object handleSuspensionpartsCoroutine;
                object handleBodyPartsCoroutine;
                
                if (!car.isFromServer && !car.isHandled)
                {
                    otherPartsBuffer = new List<ModPartScript>();
                    enginePartsBuffer = new List<ModPartScript>();
                    driveshaftPartsBuffer = new List<ModPartScript>();
                    suspensionPartsBuffer = new List<ModPartScript>();
                    bodyPartsBuffer = new List<ModCarPart>();
                    
                    handleOtherPartsCoroutine = MelonCoroutines.Start(HandleOtherPartsCoroutine(car,otherPartsBuffer));
                    handleEnginePartsCoroutine = MelonCoroutines.Start(HandleEnginePartsCoroutine(car, enginePartsBuffer));
                    handleDriveshaftPartsCoroutine = MelonCoroutines.Start(HandleDriveshaftPartsCoroutine(car, driveshaftPartsBuffer));
                    handleSuspensionpartsCoroutine = MelonCoroutines.Start(HandleSuspensionpartsCoroutine(car, suspensionPartsBuffer));
                    handleBodyPartsCoroutine = MelonCoroutines.Start(HandleBodyPartsCoroutine(car, bodyPartsBuffer));
                }
                else
                {
                    handleOtherPartsCoroutine = MelonCoroutines.Start(HandleOtherPartsCoroutine(car));
                    handleEnginePartsCoroutine = MelonCoroutines.Start(HandleEnginePartsCoroutine(car));
                    handleDriveshaftPartsCoroutine = MelonCoroutines.Start(HandleDriveshaftPartsCoroutine(car));
                    handleSuspensionpartsCoroutine = MelonCoroutines.Start(HandleSuspensionpartsCoroutine(car));
                    handleBodyPartsCoroutine = MelonCoroutines.Start(HandleBodyPartsCoroutine(car));
                }
                

                yield return handleOtherPartsCoroutine;
                yield return handleEnginePartsCoroutine;
                yield return handleDriveshaftPartsCoroutine;
                yield return handleSuspensionpartsCoroutine;
                yield return handleBodyPartsCoroutine;

                yield return new WaitForEndOfFrame();
                
                if (!car.isHandled)
                {
                    car.isHandled = true;
                    if (!car.isFromServer)
                    {
                        ClientSend.SendPartsScript(otherPartsBuffer, car.carLoaderID, ModPartType.other);
                        ClientSend.SendPartsScript(enginePartsBuffer, car.carLoaderID, ModPartType.engine);
                        ClientSend.SendPartsScript(driveshaftPartsBuffer, car.carLoaderID, ModPartType.driveshaft);
                        ClientSend.SendPartsScript(suspensionPartsBuffer, car.carLoaderID, ModPartType.suspension);
                        ClientSend.SendBodyParts(bodyPartsBuffer, car.carLoaderID);
                        MelonLogger.Msg("Sent parts !!");
                    }
                    MelonLogger.Msg($"{car.carID} is handled !");

                    if (car.isFromServer)
                    {
                        int count = 0;
                        while (!car.CarFullyReceived || count < 40)
                        {
                            yield return new WaitForSeconds(0.25f);
                            count += 1;
                        }

                        yield return new WaitForEndOfFrame();
                        MelonLogger.Msg("Car is no Longer from server.");
                        car.isFromServer = false;
                    }
                }
            }
        }

        private static IEnumerator HandleDriveshaftPartsCoroutine(ModCar car, List<ModPartScript> buffer=null)
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
                    if (!car.isFromServer)
                        buffer.Add(newPart);
                }
                else
                {
                    if (CheckDifferences(handle[i], references[i])) // TODO: Modify when paint check added
                    {
                        handle[i] = new ModPartScript(references[i], i, -1, ModPartType.driveshaft);
                        ClientSend.SendCarPart(car.carLoaderID, handle[i]);
                    }
                }
            }
        }

        private static IEnumerator HandleOtherPartsCoroutine(ModCar car, List<ModPartScript> buffer=null)
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
                        {
                            handle[i].Add(newPart);
                            if (!car.isFromServer)
                                buffer.Add(newPart);
                        }
                        
                        
                    }
                    else
                    {
                        if (CheckDifferences(handle[i][j], parts[j])) // TODO: Modify when paint check added
                        {
                            handle[i][j] = new ModPartScript(parts[j], i, j, ModPartType.other);
                           ClientSend.SendCarPart(car.carLoaderID, handle[i][j]);
                        }
                    }
                    
                }
            }
        }

        private static IEnumerator HandleEnginePartsCoroutine(ModCar car, List<ModPartScript> buffer=null)
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
                    if (!car.isFromServer)
                        buffer.Add(newPart);
                }
                else
                {
                    if (CheckDifferences(handle[i], references[i])) // TODO: Modify when paint check added
                    {
                        handle[i] =  new ModPartScript(references[i], i, -1, ModPartType.engine);
                        ClientSend.SendCarPart(car.carLoaderID, handle[i]);
                    }
                }
            }
        }
        private static IEnumerator HandleSuspensionpartsCoroutine(ModCar car, List<ModPartScript> buffer=null)
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
                        {
                            handle[i].Add(newPart);
                            if (!car.isFromServer)
                                buffer.Add(newPart);
                        }
                        
                        
                    }
                    else
                    {
                        if (CheckDifferences(handle[i][j], parts[j])) // TODO: Modify when paint check added
                        {
                            //MelonLogger.Msg("Differences Found!");
                            handle[i][j] =  new ModPartScript(parts[j], i, j, ModPartType.suspension);
                            ClientSend.SendCarPart(car.carLoaderID, handle[i][j]);
                        }
                    }
                    
                }
            }
        }

        private static IEnumerator HandleBodyPartsCoroutine(ModCar car, List<ModCarPart> buffer=null)
        {
            yield return new WaitForEndOfFrame();
            
            var references = car.partInfo.BodyPartsReferences;
            var handle = car.partInfo.BodyParts;

            for (int i = 0; i < references.Count; i++)
            {
                if (!car.isHandled)
                {
                    ModCarPart newPart = new ModCarPart(references[i], i);
                        
                    if(!handle.ContainsKey(i))
                        handle.Add(i, newPart);
                    if (!car.isFromServer)
                        buffer.Add(newPart);
                }
                else
                {
                    if (CheckDifferences(handle[i], references[i]))
                    {
                       // MelonLogger.Msg("Differences Found!");
                        handle[i] = new ModCarPart(references[i], i);
                        ClientSend.SendBodyPart(car.carLoaderID, handle[i]);
                    }
                }
            }
        }
        
        
        private static bool CheckDifferences(ModPartScript handled, PartScript toHandle) // TODO:Add Paint check
        {
            if (handled.unmounted != toHandle.IsUnmounted)
                return true;
            else if (Math.Abs(handled.dust - toHandle.Dust) > 0.1f)
                return true;
            else if (Math.Abs(handled.condition - toHandle.Condition) > 0.1f)
                return true;

            return false;
        }
        private static bool CheckDifferences(ModCarPart handled, CarPart toHandle)
        {
            if (handled == null || toHandle == null) { return false;}

            if (handled.unmounted != toHandle.Unmounted)
                return true;
            if (handled.tunedID != toHandle.TunedID)
                return true;
            else if (Math.Abs(handled.condition - toHandle.Condition) > 0.01f)
                return true;
            else if (handled.switched != toHandle.Switched)
                return true;
            else if ((Math.Abs(handled.Dust - toHandle.Dust) > 0.01f))
                return true;
            else if ((Math.Abs(handled.dent - toHandle.Dent) > 0.01f))
                return true;
            else if (handled.paintType != (int)toHandle.PaintType)
                return true;
            else if (handled.paintData.ToGame() != toHandle.PaintData)
                return true;
            else if ((Math.Abs(handled.conditionPaint - toHandle.ConditionPaint) > 0.01f))
                return true;
            /*else if (handled.colors.isDifferent(toHandle.Color))
                return true;
            else if (handled.TintColor.isDifferent(toHandle.TintColor))
                return true;*/
            else if (handled.isTinted != toHandle.IsTinted)
                return true;
            else if (handled.livery != toHandle.Livery)
                return true;
            else if (Math.Abs(handled.liveryStrength - toHandle.LiveryStrength) > 0.01f)
                return true;
            else if (Math.Abs(handled.conditionStructure - toHandle.StructureCondition) > 0.01f)
                return true;
            else if (Math.Abs(handled.washFactor - toHandle.WashFactor) > 0.01f)
                return true;

            return false;
        }
    }
}