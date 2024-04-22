using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data
{
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
                for (int i = 0; i < data.carsInGarage.Length; i++)
                {
                    carsInGarage[i] = new ModNewCarData(data.carsInGarage[i]);
                }
            }

            // Copy cars on parking
            if (data.carsOnParking != null)
            {
                carsOnParking = new ModNewCarData[data.carsOnParking.Length];
                for (int i = 0; i < data.carsOnParking.Length; i++)
                {
                    carsOnParking[i] = new ModNewCarData(data.carsOnParking[i]);
                }
            }

            // Copy car lifters data
            if (data.carLiftersData != null)
            {
                carLiftersData = new ModNewCarLifterData[data.carLiftersData.Length];
                for (int i = 0; i < data.carLiftersData.Length; i++)
                {
                    carLiftersData[i] = new ModNewCarLifterData(data.carLiftersData[i]);
                }
            }
            
            if (data.carLoaderData != null)
            {
                carLoaderData = new ModNewCarLoaderData(data.carLoaderData);
            }
            // Copy unlocked position
            if (data.unlockedPosition != null)
            {
                unlockedPosition =  new ModNewUnlockedPosition(data.unlockedPosition);
            }
            
            upgradeForPointsData = new ModNewUpgradeSystemData(data.upgradeForPointsData);
            upgradeForMoneyData = new ModNewUpgradeSystemData(data.upgradeForMoneyData);
            if (data.machines != null)
            {
                machines = new ModNewMachines(data.machines);
            }

            // Copy jobs data
            if (data.jobsData != null)
            {
                jobsData = new ModNewJobsData(data.jobsData);
            }
            // Copy global data wrapper
            if (data.globalDataWrapper != null)
            {
                globalDataWrapper = new ModNewGlobalDataWrapper(data.globalDataWrapper);
            }
      }
      public ProfileData ToGame()
      {
        ProfileData profileData = new ProfileData();

        // Copy basic data
        profileData.Name = this.Name;
        profileData.LastSave = this.LastSave;
        profileData.PlayTime = this.PlayTime;
        profileData.BestRaceTime = this.BestRaceTime;
        profileData.TopSpeed = this.TopSpeed;
        profileData.LastUID = this.LastUID;
        profileData.BuildVersion = this.BuildVersion;
        profileData.FinishedTutorial = this.FinishedTutorial;
        profileData.Difficulty = (DifficultyLevel)this.Difficulty;
        profileData.carsInGarage = CopyCarArray(this.carsInGarage);
        profileData.carsOnParking = CopyCarArray(this.carsOnParking);
        profileData.carLiftersData = CopyCarLifterArray(this.carLiftersData);
        if (this.carLoaderData != null)
        {
            profileData.carLoaderData = this.carLoaderData.ToGame();
        }
        if (this.unlockedPosition != null)
        {
            profileData.unlockedPosition = this.unlockedPosition.ToGame();
        }
        profileData.upgradeForPointsData = this.upgradeForPointsData.ToGame();
        profileData.upgradeForMoneyData = this.upgradeForMoneyData.ToGame();
        if (this.machines != null)
        {
            profileData.machines = this.machines.ToGame();
        }
        if (this.jobsData != null)
        {
            profileData.jobsData = this.jobsData.ToGame();
        }
        if (this.globalDataWrapper != null)
        {
            profileData.globalDataWrapper = this.globalDataWrapper.ToGame();
        }

        return profileData;
    }

    // Helper method to copy car array
    private NewCarData[] CopyCarArray(ModNewCarData[] carArray)
    {
        if (carArray != null)
        {
            NewCarData[] copiedArray = new NewCarData[carArray.Length];
            for (int i = 0; i < carArray.Length; i++)
            {
                if (carArray[i] != null)
                {
                    copiedArray[i] = carArray[i].ToGame();
                }
            }
            return copiedArray;
        }
        return null;
    }

    // Helper method to copy car lifter array
    private NewCarLifterData[] CopyCarLifterArray(ModNewCarLifterData[] carLifterArray)
    {
        if (carLifterArray != null)
        {
            NewCarLifterData[] copiedArray = new NewCarLifterData[carLifterArray.Length];
            for (int i = 0; i < carLifterArray.Length; i++)
            {
                if (carLifterArray[i] != null)
                {
                    copiedArray[i] = carLifterArray[i].ToGame();
                }
            }
            return copiedArray;
        }
        return null;
    }


    }
}