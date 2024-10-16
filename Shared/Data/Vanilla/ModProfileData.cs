using System;
using CMS21Together.Shared.Data.Vanilla.Cars;
using CMS21Together.Shared.Data.Vanilla.GarageTool;
using CMS21Together.Shared.Data.Vanilla.Jobs;

namespace CMS21Together.Shared.Data.Vanilla;

[Serializable]
public class ModProfileData
{
	public string Name;
	public long LastSave;
	public uint PlayTime;
	public long BestRaceTime;
	public int TopSpeed;
	public long LastUID;
	public string BuildVersion;
	public bool FinishedTutorial;
	public ModDifficultyLevel Difficulty;
	public ModNewCarData[] carsInGarage;

	public ModNewCarData[] carsOnParking;

	//public RadioData jukeboxData;
	//  public GarageCustomizationData garageCustomizationData;
	// public NewWarehouseData warehouseData;
	public ModNewCarLifterData[] carLiftersData;
	public ModNewCarLoaderData carLoaderData;
	public ModNewUnlockedPosition unlockedPosition;
	public ModNewUpgradeSystemData upgradeForPointsData;
	public ModNewUpgradeSystemData upgradeForMoneyData;
	public ModNewMachines machines;
	public ModNewJobsData jobsData;

	public ModNewGlobalDataWrapper globalDataWrapper;
	//   public PlayerData PlayerData;
	//public ShopListItemData[] ShopListItemsData;
	// public PaintshopData PaintshopData;
	// public WindowTintData WindowTintData;

	public ModProfileData(ProfileData data)
	{
		Name = data.Name;
		LastSave = data.LastSave;
		PlayTime = data.PlayTime;
		BestRaceTime = data.BestRaceTime;
		TopSpeed = data.TopSpeed;
		LastUID = data.LastUID;
		BuildVersion = data.BuildVersion;
		FinishedTutorial = data.FinishedTutorial;
		Difficulty = (ModDifficultyLevel)data.Difficulty;

		// Copy cars in garage
		if (data.carsInGarage != null)
		{
			carsInGarage = new ModNewCarData[data.carsInGarage.Length];
			for (var i = 0; i < data.carsInGarage.Length; i++) carsInGarage[i] = new ModNewCarData(data.carsInGarage[i]);
		}

		// Copy cars on parking
		if (data.carsOnParking != null)
		{
			carsOnParking = new ModNewCarData[data.carsOnParking.Length];
			for (var i = 0; i < data.carsOnParking.Length; i++) carsOnParking[i] = new ModNewCarData(data.carsOnParking[i]);
		}

		// Copy car lifters data
		if (data.carLiftersData != null)
		{
			carLiftersData = new ModNewCarLifterData[data.carLiftersData.Length];
			for (var i = 0; i < data.carLiftersData.Length; i++) carLiftersData[i] = new ModNewCarLifterData(data.carLiftersData[i]);
		}

		if (data.carLoaderData != null) carLoaderData = new ModNewCarLoaderData(data.carLoaderData);
		// Copy unlocked position
		if (data.unlockedPosition != null) unlockedPosition = new ModNewUnlockedPosition(data.unlockedPosition);

		upgradeForPointsData = new ModNewUpgradeSystemData(data.upgradeForPointsData);
		upgradeForMoneyData = new ModNewUpgradeSystemData(data.upgradeForMoneyData);
		if (data.machines != null) machines = new ModNewMachines(data.machines);

		// Copy jobs data
		if (data.jobsData != null) jobsData = new ModNewJobsData(data.jobsData);
		// Copy global data wrapper
		if (data.globalDataWrapper != null) globalDataWrapper = new ModNewGlobalDataWrapper(data.globalDataWrapper);
	}

	public ProfileData ToGame()
	{
		var profileData = new ProfileData();

		// Copy basic data
		profileData.Name = Name;
		profileData.LastSave = LastSave;
		profileData.PlayTime = PlayTime;
		profileData.BestRaceTime = BestRaceTime;
		profileData.TopSpeed = TopSpeed;
		profileData.LastUID = LastUID;
		profileData.BuildVersion = BuildVersion;
		profileData.FinishedTutorial = FinishedTutorial;
		profileData.Difficulty = (DifficultyLevel)Difficulty;
		profileData.carsInGarage = CopyCarArray(carsInGarage);
		profileData.carsOnParking = CopyCarArray(carsOnParking);
		profileData.carLiftersData = CopyCarLifterArray(carLiftersData);
		if (carLoaderData != null) profileData.carLoaderData = carLoaderData.ToGame();
		if (unlockedPosition != null) profileData.unlockedPosition = unlockedPosition.ToGame();
		profileData.upgradeForPointsData = upgradeForPointsData.ToGame();
		profileData.upgradeForMoneyData = upgradeForMoneyData.ToGame();
		if (machines != null) profileData.machines = machines.ToGame();
		if (jobsData != null) profileData.jobsData = jobsData.ToGame();
		if (globalDataWrapper != null) profileData.globalDataWrapper = globalDataWrapper.ToGame();

		return profileData;
	}

	// Helper method to copy car array
	private NewCarData[] CopyCarArray(ModNewCarData[] carArray)
	{
		if (carArray != null)
		{
			var copiedArray = new NewCarData[carArray.Length];
			for (var i = 0; i < carArray.Length; i++)
				if (carArray[i] != null)
					copiedArray[i] = carArray[i].ToGame();
			return copiedArray;
		}

		return null;
	}

	// Helper method to copy car lifter array
	private NewCarLifterData[] CopyCarLifterArray(ModNewCarLifterData[] carLifterArray)
	{
		if (carLifterArray != null)
		{
			var copiedArray = new NewCarLifterData[carLifterArray.Length];
			for (var i = 0; i < carLifterArray.Length; i++)
				if (carLifterArray[i] != null)
					copiedArray[i] = carLifterArray[i].ToGame();
			return copiedArray;
		}

		return null;
	}
}