using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.Shared.Data;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Car;

public static class CarReferences
{
    public static IEnumerator GetCarReferences(int carLoaderID)
    {
        while(ClientData.Instance.GameReady == false) // DO NOT REMOVE!
            yield return new WaitForSeconds(0.5f);

        ModCar car = ClientData.Instance.LoadedCars[carLoaderID];
        car.partInfo = new ModPartInfo();
        
        IEnumerator getBodyPartsCoroutine = GetBodyPartsReferences(car);
        IEnumerator getOtherPartsCoroutine = GetOtherPartsReferences(car);
        IEnumerator getEnginePartsCoroutine = GetEnginePartsReferences(car);
        IEnumerator getDriveshaftPartsCoroutine = GetDriveshaftPartsReferences(car);
        IEnumerator getSuspensionPartsCoroutine = GetSuspensionPartsReferences(car);

        yield return getBodyPartsCoroutine;
        yield return getOtherPartsCoroutine;
        yield return getEnginePartsCoroutine;
        yield return getDriveshaftPartsCoroutine;
        yield return getSuspensionPartsCoroutine;
        
        yield return new WaitForEndOfFrame();
            
        car.isReferenced = true;
        MelonLogger.Msg($"{car.carID} is referenced!");
    }

    private static IEnumerator GetOtherPartsReferences(ModCar car)
    {
        yield return new WaitForEndOfFrame();

        var partList = GameData.Instance.carLoaders[car.carLoaderID].Parts;
        var reference = car.partInfo.OtherPartsReferences;

        for (int i = 0; i < partList.Count; i++)
        {
            var partObject = GameData.Instance.carLoaders[car.carLoaderID].Parts._items[i].p_handle;
            var parts = partObject.GetComponentsInChildren<PartScript>().ToList();

            for (int j = 0; j < parts.Count; j++)
            {
                if(!reference.ContainsKey(i))
                    reference.Add(i, new List<PartScript>() {parts[j]});
                else if(!reference[i].Contains(parts[j]))
                    reference[i].Add(parts[j]);
            }
        }
    }

    private static IEnumerator GetEnginePartsReferences(ModCar car)
    {
        yield return new WaitForEndOfFrame();
        
        var engine =GameData.Instance.carLoaders[car.carLoaderID].e_engine_h;
        var engineParts = engine.GetComponentsInChildren<PartScript>().ToList();

        var reference = car.partInfo.EnginePartsReferences;

        for (int i = 0; i < engineParts.Count; i++) 
        {
            if(!reference.ContainsKey(i))
                reference.Add(i, engineParts[i]);
        }
    }
    
    private static IEnumerator GetDriveshaftPartsReferences(ModCar car)
    {
        yield return new WaitForEndOfFrame();
        
        var driveshaft = GameData.Instance.carLoaders[car.carLoaderID].ds_h;
        if (driveshaft != null)
        {
            var driveshaftParts = driveshaft.GetComponentsInChildren<PartScript>().ToList();

            var reference = car.partInfo.DriveshaftPartsReferences;

            for (int i = 0; i < driveshaftParts.Count; i++) 
            {
                if(!reference.ContainsKey(i))
                    reference.Add(i, driveshaftParts[i]);
            }
        }
    }
    
    private static IEnumerator GetSuspensionPartsReferences(ModCar car)
    {
        yield return new WaitForEndOfFrame();
        
        List<GameObject> suspensions = new List<GameObject>();
        suspensions.Add(GameData.Instance.carLoaders[car.carLoaderID].s_frontCenter_h);
        suspensions.Add(GameData.Instance.carLoaders[car.carLoaderID].s_frontLeft_h);
        suspensions.Add(GameData.Instance.carLoaders[car.carLoaderID].s_frontRight_h);
        suspensions.Add(GameData.Instance.carLoaders[car.carLoaderID].s_rearCenter_h);
        suspensions.Add(GameData.Instance.carLoaders[car.carLoaderID].s_rearLeft_h);
        suspensions.Add(GameData.Instance.carLoaders[car.carLoaderID].s_rearRight_h);

        var reference = car.partInfo.SuspensionPartsReferences;

        for (int i = 0; i < suspensions.Count; i++)
        {
            var suspensionParts = suspensions[i].GetComponentsInChildren<PartScript>().ToList();

            for (int j = 0; j < suspensionParts.Count; j++)
            {
                if (!reference.ContainsKey(i))
                    reference.Add(i, new List<PartScript>() { suspensionParts[j] });
                if (!reference[i].Contains(suspensionParts[j]))
                    reference[i].Add(suspensionParts[j]);
            }
        }
    }
    
    private static IEnumerator GetBodyPartsReferences(ModCar car)
    {
        yield return new WaitForEndOfFrame();
            
        var bodyParts = GameData.Instance.carLoaders[car.carLoaderID].carParts._items;
        var reference = car.partInfo.BodyPartsReferences;

        for (int i = 0; i < bodyParts.Count; i++)
        {
            if(!reference.ContainsKey(i))
                reference.Add(i, bodyParts[i]);
        }
    }
}