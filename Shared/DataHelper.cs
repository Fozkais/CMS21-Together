using System;
using System.IO;
using System.Reflection;
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine;
using BinaryWriter = Il2CppSystem.IO.BinaryWriter;
using SeekOrigin = Il2CppSystem.IO.SeekOrigin;

namespace CMS21Together.Shared;

public class DataHelper // TODO: Finish This!
{
	public static Stream LoadContent(string assemblyPath)
	{
		var assembly = Assembly.GetExecutingAssembly();
		var stream = assembly.GetManifestResourceStream(assemblyPath);

		return stream;
	}

	public static Texture2D LoadCustomTexture(string path)
	{
		var stream = LoadContent(path);

		var reference = new Texture2D(2, 2);

		var buffer = new byte[stream.Length];
		stream.Read(buffer, 0, (int)stream.Length);

		ImageConversion.LoadImage(reference, buffer);

		if (reference != null)
			MelonLogger.Msg("Texture Loaded.");
		return reference;
	}

	public static Il2CppSystem.IO.Stream DeepCopy(Stream sourceStream)
	{
		if (sourceStream == null)
			throw new ArgumentNullException(nameof(sourceStream));

		// Sérialiser le stream source
		byte[] serializedData;
		using (var memoryStream = new MemoryStream())
		{
			sourceStream.CopyTo(memoryStream);
			serializedData = memoryStream.ToArray();
		}

		// Écrire les données sérialisées dans un nouveau Il2CppSystem.IO.Stream
		Il2CppSystem.IO.Stream newStream = new Il2CppSystem.IO.MemoryStream();
		var writer = new BinaryWriter(newStream);
		writer.Write(serializedData);
		writer.Flush();

		// Assurez-vous de remettre le curseur au début du nouveau stream
		newStream.Seek(0, SeekOrigin.Begin);

		return newStream;
	}

	public static ProfileData Copy(ProfileData data) // TODO: check for otherDatatypes
	{
		var copy = new ProfileData();
		copy.Name = data.Name;
		copy.machines = data.machines; //  TODO: check this
		copy.Difficulty = data.Difficulty;
		copy.inventoryData = data.inventoryData; //  TODO: check this
		copy.jobsData = data.jobsData;
		copy.jukeboxData = data.jukeboxData;
		copy.saveVersion = data.saveVersion;
		copy.unlockedPosition = data.unlockedPosition;
		copy.warehouseData = data.warehouseData; //  TODO: check this
		copy.BuildVersion = data.BuildVersion;
		copy.carLiftersData = data.carLiftersData; //  TODO: check this
		copy.carLoaderData = data.carLoaderData; //  TODO: check this
		copy.carsInGarage = new Il2CppReferenceArray<NewCarData>(data.carsInGarage.Length);
		for (var i = 0; i < data.carsInGarage.Length; i++) copy.carsInGarage[i] = Copy(data.carsInGarage[i]);
		copy.carsOnParking = new Il2CppReferenceArray<NewCarData>(data.carsOnParking.Length);
		for (var i = 0; i < data.carsOnParking.Length; i++) copy.carsOnParking[i] = Copy(data.carsOnParking[i]);
		copy.FinishedTutorial = data.FinishedTutorial;
		copy.garageCustomizationData = data.garageCustomizationData; //  TODO: check this
		copy.globalDataWrapper = data.globalDataWrapper; //  TODO: check this
		copy.LastSave = data.LastSave;
		copy.PaintshopData = data.PaintshopData; //  TODO: check this
		copy.PlayerData = data.PlayerData; //  TODO: check this
		copy.PlayTime = data.PlayTime;
		copy.TopSpeed = data.TopSpeed;
		copy.BestRaceTime = data.BestRaceTime;
		copy.upgradeForMoneyData = data.upgradeForMoneyData;
		copy.upgradeForPointsData = data.upgradeForPointsData;
		copy.WindowTintData = data.WindowTintData;
		copy.LastUID = data.LastUID;
		copy.ShopListItemsData = data.ShopListItemsData; //  TODO: check this

		return copy;
	}

	public static NewCarData Copy(NewCarData data)
	{
		var copy = new NewCarData();
		copy.index = data.index;
		copy.carToLoad = data.carToLoad;
		copy.color = data.color;
		copy.configVersion = data.configVersion;
		copy.customerCar = data.customerCar;
		copy.ecuData = data.ecuData;
		copy.engineSwap = data.engineSwap;
		copy.factoryColor = data.factoryColor;
		copy.gearRatio = data.gearRatio;
		copy.orderConnection = data.orderConnection;
		copy.rimsSize = data.rimsSize;
		copy.tiresSize = data.tiresSize;
		copy.wheelsWidth = data.wheelsWidth;
		copy.EngineData = data.EngineData;
		copy.factoryPaintType = data.factoryPaintType;
		copy.finalDriveRatio = data.finalDriveRatio;
		copy.FluidsData = data.FluidsData;
		copy.LightsOn = data.LightsOn;
		copy.measuredDragIndex = data.measuredDragIndex;
		copy.PaintData = data.PaintData;
		copy.PartData = data.PartData;
		copy.tiresET = data.tiresET;
		copy.TooolsData = data.TooolsData;
		copy.UId = data.UId;
		copy.WheelsAlignment = data.WheelsAlignment;
		copy.AdditionalCarRot = data.AdditionalCarRot;
		copy.BodyPartsData = data.BodyPartsData;
		copy.BonusPartsData = data.BonusPartsData;
		copy.CarInfoData = data.CarInfoData;
		copy.LicensePlatesData = data.LicensePlatesData;
		copy.HasCustomPaintType = data.HasCustomPaintType;
		copy.HeadlampLeftAlignmentData = data.HeadlampLeftAlignmentData;
		copy.HeadlampRightAlignmentData = data.HeadlampRightAlignmentData;

		return copy;
	}
}