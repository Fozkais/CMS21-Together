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
                List<ModPartScript> suspensionPartsBuffer = null;
                List<ModCarPart> bodyPartsBuffer = null;
                
                object handleOtherPartsCoroutine;
                object handleEnginePartsCoroutine;
                object handleSuspensionpartsCoroutine;
                object handleBodyPartsCoroutine;
                
                if (!car.isFromServer && !car.isHandled)
                {
                    otherPartsBuffer = new List<ModPartScript>();
                    enginePartsBuffer = new List<ModPartScript>();
                    suspensionPartsBuffer = new List<ModPartScript>();
                    bodyPartsBuffer = new List<ModCarPart>();
                    
                    handleOtherPartsCoroutine = MelonCoroutines.Start(HandleOtherPartsCoroutine(car,otherPartsBuffer));
                    handleEnginePartsCoroutine = MelonCoroutines.Start(HandleEnginePartsCoroutine(car, enginePartsBuffer));
                    handleSuspensionpartsCoroutine = MelonCoroutines.Start(HandleSuspensionpartsCoroutine(car, suspensionPartsBuffer));
                    handleBodyPartsCoroutine = MelonCoroutines.Start(HandleBodyPartsCoroutine(car, bodyPartsBuffer));
                }
                else
                {
                    handleOtherPartsCoroutine = MelonCoroutines.Start(HandleOtherPartsCoroutine(car));
                    handleEnginePartsCoroutine = MelonCoroutines.Start(HandleEnginePartsCoroutine(car));
                    handleSuspensionpartsCoroutine = MelonCoroutines.Start(HandleSuspensionpartsCoroutine(car));
                    handleBodyPartsCoroutine = MelonCoroutines.Start(HandleBodyPartsCoroutine(car));
                }
                

                yield return handleOtherPartsCoroutine;
                yield return handleEnginePartsCoroutine;
                yield return handleSuspensionpartsCoroutine;
                yield return handleBodyPartsCoroutine;

                yield return new WaitForEndOfFrame();
                
                if (!car.isHandled)
                {
                    car.isHandled = true;
                    if (!car.isFromServer)
                    {
                        ClientSend.SendPartsScript(otherPartsBuffer, car.carLoaderID);
                        ClientSend.SendPartsScript(enginePartsBuffer, car.carLoaderID);
                        ClientSend.SendPartsScript(suspensionPartsBuffer, car.carLoaderID);
                        ClientSend.SendBodyParts(bodyPartsBuffer, car.carLoaderID);
                    }
                    else
                    {
                        yield return new WaitForEndOfFrame();
                        yield return new WaitForSeconds(0.5f);
                        yield return new WaitForEndOfFrame();
                        car.isFromServer = false;
                    }
                    MelonLogger.Msg($"{car.carID} is handled !");
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
                           // MelonLogger.Msg("Differences Found!");
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
                        //MelonLogger.Msg("Differences Found!");
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
                    if (CheckDifferences(handle[i], references[i])) // TODO: Modify when paint check added
                    {
                        //MelonLogger.Msg("Differences Found!");
                        ClientSend.SendBodyPart(car.carLoaderID, handle[i]);
                    }
                }
            }
        }
        
        
        private static bool CheckDifferences(ModPartScript handled, PartScript toHandle) // TODO:Add Paint check
        {
            bool hasDifferences = false;
            if (handled.unmounted != toHandle.IsUnmounted)
            {
                handled.unmounted = toHandle.IsUnmounted;
                hasDifferences = true;
            }

            return hasDifferences;
        }
        private static bool CheckDifferences(ModCarPart handled, CarPart toHandle) // TODO:Add Paint check
        {
            if (handled == null || toHandle == null) { return false;}
            
            bool hasDifferences = false;
            if (handled.unmounted != toHandle.Unmounted)
            {
                handled.unmounted = toHandle.Unmounted;
                hasDifferences = true;
            }

            if (handled.switched != toHandle.Switched)
            {
                handled.switched = toHandle.Switched;
                hasDifferences = true;
            }

            return hasDifferences;
        }
    }
}