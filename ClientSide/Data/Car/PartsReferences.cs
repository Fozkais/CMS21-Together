using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.Shared.Data;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Car
{
    public static class PartsReferences
    {
        public static IEnumerator GetPartsReferences(int carLoaderID)
        {
            ModCar car = ClientData.LoadedCars[carLoaderID];
            car.partInfo = new ModPartInfo();

            yield return new WaitForEndOfFrame();

            if (car.isCarLoaded)
            {
                IEnumerator getOtherPartsCoroutine = GetOtherPartsReferences(car);
                IEnumerator getEnginePartsCoroutine = GetEnginePartsReferences(car);
                IEnumerator getDriveshaftPartsCoroutine = GetDriveshaftPartsReferences(car);
                IEnumerator getSuspensionPartsCoroutine = GetSuspensionPartsReferences(car);
                IEnumerator getBodyPartsCoroutine = GetBodyPartsReferences(car);

                yield return getOtherPartsCoroutine;
                yield return getEnginePartsCoroutine;
                yield return getDriveshaftPartsCoroutine;
                yield return getSuspensionPartsCoroutine;
                yield return getBodyPartsCoroutine;

                yield return new WaitForEndOfFrame();
                
                car.isReferenced = true;
                MelonLogger.Msg($"{car.carID} is referenced!");
                yield break;
            }
            MelonLogger.Msg($"{car.carID} asn't loaded properly!");
        }



        private static IEnumerator GetOtherPartsReferences(ModCar car)
        {
            yield return new WaitForEndOfFrame();

            var otherParts = GameData.Instance.carLoaders[car.carLoaderID].Parts;
            var reference = car.partInfo.OtherPartsReferences;

            for (int i = 0; i < otherParts.Count; i++)
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
            
            var driveshaft =GameData.Instance.carLoaders[car.carLoaderID].ds_h;
            var driveshaftParts = driveshaft.GetComponentsInChildren<PartScript>().ToList();

            var reference = car.partInfo.EnginePartsReferences;

            for (int i = 0; i < driveshaftParts.Count; i++) 
            {
                if(!reference.ContainsKey(i))
                    reference.Add(i, driveshaftParts[i]);
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
                if(suspensionParts.Count > 0)
                    reference.Add(i, new List<PartScript>());
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
            
            var reference = car.partInfo.BodyPartsReferences;
            var bodyParts = GameData.Instance.carLoaders[car.carLoaderID].carParts._items;

            for (int i = 0; i < bodyParts.Count; i++)
            {
                if(!reference.ContainsKey(i))
                    reference.Add(i, bodyParts[i]);
            }
        }
    }
}
