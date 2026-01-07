using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.Shared.Data;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Car;

public static class PartsReferencer
{
	public static IEnumerator GetPartReferences(ModCar car)
	{
		if (car.needResync) yield break;
		
		while (!GameData.isReady)
			yield return new WaitForSeconds(0.25f);

		yield return new WaitForSeconds(1f);
		yield return new WaitForEndOfFrame();

		car.CarPartInfo = new ModCarPartInfo();
		var getBodyPartCoroutine = GetBodyPartCoroutine(car);
		var getOtherPartCoroutine = GetOtherPartCoroutine(car);
		var getEnginePartCoroutine = GetEnginePartCoroutine(car);
		var getDriveshaftPartCoroutine = GetDriveshaftPartCoroutine(car);
		var getSuspensionPartCoroutine = GetSuspensionPartCoroutine(car);

		yield return getBodyPartCoroutine;
		yield return getOtherPartCoroutine;
		yield return getEnginePartCoroutine;
		yield return getDriveshaftPartCoroutine;
		yield return getSuspensionPartCoroutine;

		yield return new WaitForEndOfFrame();
		car.isReady = true;
		car.isFromServer = false;
		MelonLogger.Msg($"[PartsReferencer->GetPartReferences] {car.carID} is ready.");
	}

	private static IEnumerator GetSuspensionPartCoroutine(ModCar car)
	{
		yield return new WaitForEndOfFrame();

		var suspensions = new List<GameObject>();
		suspensions.Add(GameData.Instance.carLoaders[car.carLoaderID].s_frontCenter_h);
		suspensions.Add(GameData.Instance.carLoaders[car.carLoaderID].s_frontLeft_h);
		suspensions.Add(GameData.Instance.carLoaders[car.carLoaderID].s_frontRight_h);
		suspensions.Add(GameData.Instance.carLoaders[car.carLoaderID].s_rearCenter_h);
		suspensions.Add(GameData.Instance.carLoaders[car.carLoaderID].s_rearLeft_h);
		suspensions.Add(GameData.Instance.carLoaders[car.carLoaderID].s_rearRight_h);

		var reference = car.CarPartInfo.SuspensionPartsReferences;

		for (var i = 0; i < suspensions.Count; i++)
		{
			var suspensionParts = suspensions[i].GetComponentsInChildren<PartScript>().ToList();

			for (var j = 0; j < suspensionParts.Count; j++)
			{
				if (!reference.ContainsKey(i))
					reference.Add(i, new List<PartScript>());
				if (!reference[i].Contains(suspensionParts[j]))
					reference[i].Add(suspensionParts[j]);
			}
		}
	}

	private static IEnumerator GetDriveshaftPartCoroutine(ModCar car)
	{
		yield return new WaitForEndOfFrame();

		var driveshaft = GameData.Instance.carLoaders[car.carLoaderID].ds_h;
		if (driveshaft != null)
		{
			var driveshaftParts = driveshaft.GetComponentsInChildren<PartScript>().ToList();

			var reference = car.CarPartInfo.DriveshaftPartsReferences;

			for (var i = 0; i < driveshaftParts.Count; i++)
				if (!reference.ContainsKey(i))
					reference.Add(i, driveshaftParts[i]);
		}
	}

	private static IEnumerator GetEnginePartCoroutine(ModCar car)
	{
		yield return new WaitForEndOfFrame();

		var engine = GameData.Instance.carLoaders[car.carLoaderID].e_engine_h;
		var engineParts = engine.GetComponentsInChildren<PartScript>().ToList();

		var reference = car.CarPartInfo.EnginePartsReferences;

		for (var i = 0; i < engineParts.Count; i++)
			if (!reference.ContainsKey(i))
				reference.Add(i, engineParts[i]);
	}

	private static IEnumerator GetOtherPartCoroutine(ModCar car)
	{
		yield return new WaitForEndOfFrame();

		var partList = GameData.Instance.carLoaders[car.carLoaderID].Parts;
		var reference = car.CarPartInfo.OtherPartsReferences;

		for (var i = 0; i < partList.Count; i++)
		{
			var partObject = GameData.Instance.carLoaders[car.carLoaderID].Parts.ToArray()[i].p_handle;
			var parts = partObject.GetComponentsInChildren<PartScript>().ToList();

			for (var j = 0; j < parts.Count; j++)
			{
				if (!reference.ContainsKey(i))
					reference.Add(i, new List<PartScript>());
				if (!reference[i].Contains(parts[j]))
					reference[i].Add(parts[j]);
			}
		}
	}

	private static IEnumerator GetBodyPartCoroutine(ModCar car)
	{
		yield return new WaitForEndOfFrame();

		var bodyParts = GameData.Instance.carLoaders[car.carLoaderID].carParts.ToArray();
		var reference = car.CarPartInfo.BodyPartsReferences;

		for (var i = 0; i < bodyParts.Count; i++)
			if (!reference.ContainsKey(i))
				reference.Add(i, bodyParts[i]);
	}
}